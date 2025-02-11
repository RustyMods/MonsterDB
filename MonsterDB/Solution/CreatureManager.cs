﻿using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using MonsterDB.Solution.Methods;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution;

public static class CreatureManager
{
    public static readonly string m_folderPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "MonsterDB";
    public static readonly string m_creatureFolderPath = m_folderPath + Path.DirectorySeparatorChar + "Creatures";
    public static readonly string m_cloneFolderPath = m_folderPath + Path.DirectorySeparatorChar + "Clones";
    private static readonly string m_exportFolderPath = m_folderPath + Path.DirectorySeparatorChar + "Export";
    public static readonly string m_importFolderPath = m_folderPath + Path.DirectorySeparatorChar + "Import";

    private static readonly CustomSyncedValue<string> m_serverDataFiles = new(MonsterDBPlugin.ConfigSync, "MonsterDB_ServerFiles", "");
    public static readonly Dictionary<string, CreatureData> m_originalData = new();
    public static Dictionary<string, CreatureData> m_data = new();
    public static readonly Dictionary<string, CreatureData> m_localData = new();
    public static readonly Dictionary<string, GameObject> m_clones = new();

    private static FileSystemWatcher m_creatureWatcher = null!;
    private static FileSystemWatcher m_cloneWatcher = null!;

    private static bool m_writing;
    
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static class ZNet_Awake_Patch
    {
        private static void Postfix() => UpdateServer();
    }

    private static void UpdateServer()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Server: Updating server sync creature & clones");
        ISerializer serializer = new SerializerBuilder().Build();
        m_serverDataFiles.Value = serializer.Serialize(m_data);
    }

    public static bool IsClone(GameObject creature) => m_clones.ContainsKey(creature.name);
    
    public static void Setup()
    {
        m_serverDataFiles.ValueChanged += ReadServerFiles;
        SetupFileWatch();
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_creatureFolderPath)) Directory.CreateDirectory(m_creatureFolderPath);
        if (!Directory.Exists(m_cloneFolderPath)) Directory.CreateDirectory(m_cloneFolderPath);
        if (!Directory.Exists(m_importFolderPath)) Directory.CreateDirectory(m_importFolderPath);
        if (!Directory.Exists(m_exportFolderPath)) Directory.CreateDirectory(m_exportFolderPath);
    }

    private static void SetupFileWatch()
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_creatureFolderPath)) Directory.CreateDirectory(m_creatureFolderPath);
        if (!Directory.Exists(m_cloneFolderPath)) Directory.CreateDirectory(m_cloneFolderPath);
        if (!Directory.Exists(m_importFolderPath)) Directory.CreateDirectory(m_importFolderPath);
        m_creatureWatcher = new FileSystemWatcher(m_creatureFolderPath, "*.yml")
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };

        m_creatureWatcher.Changed += OnCreatureFileChange;

        m_cloneWatcher = new FileSystemWatcher(m_cloneFolderPath, "*.yml")
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };

        m_cloneWatcher.Changed += OnCloneFileChange;

        FileSystemWatcher m_importFileWatcher = new FileSystemWatcher(m_importFolderPath, "*.yml")
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        m_importFileWatcher.Changed += OnImportFileChange;
    }

    private static void OnImportFileChange(object sender, FileSystemEventArgs e)
    {
        if (!MonsterDBPlugin.AutoUpdate() || m_writing) return;
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            string fileName = Path.GetFileName(e.Name);
            var deserializer = new DeserializerBuilder().Build();
            var serial = File.ReadAllText(e.FullPath);
            var data = deserializer.Deserialize<CreatureData>(serial);
            var prefab = DataBase.TryGetGameObject(data.m_characterData.PrefabName);
            if (prefab == null) return;
            m_data[data.m_characterData.PrefabName] = data;
            UpdateServer();
            Update(prefab);
            MonsterDBPlugin.MonsterDBLogger.LogDebug($"{prefab.name} data changed: {fileName}");
        }
        catch
        {
            Helpers.LogParseFailure(e.FullPath);
        }
    }

    private static void OnCloneFileChange(object sender, FileSystemEventArgs e)
    {
        if (!MonsterDBPlugin.AutoUpdate() || m_writing) return;
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string fileName = Path.GetFileName(e.Name);
        string cloneName = GetCloneFolderName(e.FullPath);
        GameObject? prefab = DataBase.TryGetGameObject(cloneName);
        if (prefab == null) return;
        Read(cloneName, true);
        Update(prefab);
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"{cloneName} data changed: {fileName}");
    }

    private static void OnCreatureFileChange(object sender, FileSystemEventArgs e)
    {
        if (!MonsterDBPlugin.AutoUpdate() || m_writing) return;
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string fileName = Path.GetFileName(e.Name);
        string creatureName = GetCreatureFolderName(e.FullPath);
        GameObject? prefab = DataBase.TryGetGameObject(creatureName);
        if (prefab == null) return;
        Read(creatureName);
        Update(prefab);
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"{creatureName} data changed: {fileName}");
    }

    private static string GetCloneFolderName(string fullPath)
    {
        return fullPath.Replace(m_cloneFolderPath, string.Empty).Split(Path.DirectorySeparatorChar)[1];
    }

    private static string GetCreatureFolderName(string fullPath)
    {
        return fullPath.Replace(m_creatureFolderPath, string.Empty).Split(Path.DirectorySeparatorChar)[1];
    }
    
    private static void ReadServerFiles()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (m_serverDataFiles.Value.IsNullOrWhiteSpace()) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            Initialization.ResetAll();
            m_data = deserializer.Deserialize<Dictionary<string, CreatureData>>(m_serverDataFiles.Value);
            Initialization.RemoveAllClones();
            Initialization.CloneAll();
            Initialization.UpdateAll();
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Server updated MonsterDB Files");
        }
        catch
        {
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Failed to parse server synced data");
        }
    }

    public static void Delete(GameObject critter, bool loop = true)
    {
        string ragDollName = critter.name + "_ragdoll";
        GameObject? ragDoll = DataBase.TryGetGameObject(ragDollName);
        if (ragDoll != null) Delete(ragDoll, loop);
        Helpers.RemoveFromZNetScene(critter);
        Helpers.RemoveFromObjectDB(critter);
        if (!loop) m_clones.Remove(critter.name);
        Transform clone = MonsterDBPlugin.m_root.transform.Find(critter.name);
        if (!clone) return;
        Object.Destroy(clone.gameObject);
    }

    private static void Save(GameObject critter, string cloneFrom)
    {
        if (m_originalData.ContainsKey(critter.name)) return;
        CreatureData data = new CreatureData();
        VisualMethods.Save(critter, ref data);
        HumanoidMethods.Save(critter, cloneFrom, ref data);
        CharacterMethods.Save(critter, cloneFrom, ref data);
        MonsterAIMethods.Save(critter, ref data);
        AnimalAIMethods.Save(critter, ref data);
        CharacterDropMethods.Save(critter, ref data);
        TameableMethods.Save(critter, ref data);
        ProcreationMethods.Save(critter, ref data);
        NPCTalkMethods.Save(critter, ref data);
        GrowUpMethods.Save(critter, ref data);
        LevelEffectsMethods.Save(critter, ref data);
        m_originalData[critter.name] = data;
    }

    public static bool Write(GameObject critter, out string folderPath, bool clone = false, string clonedFrom = "", bool writeAll = false)
    {
        m_writing = true;
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_creatureFolderPath)) Directory.CreateDirectory(m_creatureFolderPath);
        folderPath = (clone ? m_cloneFolderPath : m_creatureFolderPath) + Path.DirectorySeparatorChar + critter.name;
        if (writeAll && Directory.Exists(folderPath))
        {
            MonsterDBPlugin.MonsterDBLogger.LogDebug(critter.name + " files already exist, skipping");
            m_writing = false;
            return false;
        }
        if (Directory.Exists(folderPath)) Directory.Delete(folderPath, true);
        Directory.CreateDirectory(folderPath);
        VisualMethods.Write(critter, folderPath);
        HumanoidMethods.Write(critter, folderPath, clonedFrom);
        CharacterMethods.Write(critter, folderPath, clonedFrom);
        MonsterAIMethods.Write(critter,folderPath);
        AnimalAIMethods.Write(critter, folderPath);
        CharacterDropMethods.Write(critter, folderPath);
        TameableMethods.Write(critter, folderPath);
        ProcreationMethods.Write(critter, folderPath);
        NPCTalkMethods.Write(critter, folderPath);
        GrowUpMethods.Write(critter, folderPath);
        LevelEffectsMethods.Write(critter, folderPath);
        m_writing = false;
        return true;
    }

    public static bool Read(string creatureName, bool isClone = false)
    {
        string folderPath = (isClone ? m_cloneFolderPath : m_creatureFolderPath) + Path.DirectorySeparatorChar + creatureName;
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("Failed to find folder: ");
            Debug.LogWarning(folderPath);
            return false;
        }
        
        CreatureData data = new();
        VisualMethods.Read(folderPath, ref data);
        HumanoidMethods.Read(folderPath, ref data);
        CharacterMethods.Read(folderPath, ref data);
        MonsterAIMethods.Read(folderPath, ref data);
        AnimalAIMethods.Read(folderPath, ref data);
        CharacterDropMethods.Read(folderPath, ref data);
        TameableMethods.Read(folderPath, ref data);
        ProcreationMethods.Read(folderPath, ref data);
        NPCTalkMethods.Read(folderPath, ref data);
        GrowUpMethods.Read(folderPath, ref data);
        LevelEffectsMethods.Read(folderPath, ref data);
        
        m_data[creatureName] = data;
        m_localData[creatureName] = data;
        UpdateServer();
        return true;
    }

    public static bool Update(GameObject critter, bool local = false)
    {
        if (local)
        {
            if (!m_localData.TryGetValue(critter.name, out CreatureData data))
            {
                Debug.LogWarning("Failed to find: " + critter.name);
                return false;
            }
            Save(critter, data.m_characterData.ClonedFrom);
            Update(critter, data);
        }
        else
        {
            if (!m_data.TryGetValue(critter.name, out CreatureData data))
            {
                Debug.LogWarning("Failed to find: " + critter.name);
                return false;
            }
            Save(critter, data.m_characterData.ClonedFrom);
            Update(critter, data);
        }

        return true;
    }

    private static void Update(GameObject critter, CreatureData data)
    {
        VisualMethods.Update(critter, data);
        TameableMethods.Update(critter, data);
        HumanoidMethods.Update(critter, data);
        CharacterMethods.Update(critter, data);
        MonsterAIMethods.Update(critter, data);
        AnimalAIMethods.Update(critter, data);
        CharacterDropMethods.Update(critter, data);
        ProcreationMethods.Update(critter, data);
        NPCTalkMethods.Update(critter, data);
        GrowUpMethods.Update(critter, data);
        LevelEffectsMethods.Update(critter, data);
    }

    public static void Reset(GameObject critter)
    {
        if (!m_originalData.TryGetValue(critter.name, out CreatureData data)) return;
        Update(critter, data);
        MonsterDBPlugin.MonsterDBLogger.LogInfo("Reset: " + critter.name);
    }

    public static void Clone(GameObject critter, string name, bool saveToDisk = true, bool cloneAttacks = true)
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_cloneFolderPath)) Directory.CreateDirectory(m_cloneFolderPath);
        GameObject? clone;
        if (critter.name == "Human")
        {
            clone = HumanMan.Create(name);
            if (clone == null) return;
        }
        else
        {
            clone = Object.Instantiate(critter, MonsterDBPlugin.m_root.transform, false);
            clone.name = name;
            Dictionary<string, Material> ragDollMats = VisualMethods.CloneMaterials(clone);
            CharacterMethods.CloneRagDoll(clone, ragDollMats);
            HumanoidMethods.CloneRagDoll(clone, ragDollMats);
            m_clones[clone.name] = clone;
            Helpers.RegisterToZNetScene(clone);
        }
        if (cloneAttacks) HumanoidMethods.CloneItems(clone);
        if (!saveToDisk) return;
        Write(clone, out string folderPath, true, critter.name);
        Read(clone.name, true);
        MonsterDBPlugin.MonsterDBLogger.LogInfo($"Cloned {critter.name} as {clone.name}");
        MonsterDBPlugin.MonsterDBLogger.LogInfo(folderPath);
    }

    public static void Export(string creatureName)
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_exportFolderPath)) Directory.CreateDirectory(m_exportFolderPath);
        if (!Directory.Exists(m_importFolderPath)) Directory.CreateDirectory(m_importFolderPath);
        if (!m_data.TryGetValue(creatureName, out CreatureData data)) return;
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        string filePath = m_exportFolderPath + Path.DirectorySeparatorChar + creatureName + ".yml";
        File.WriteAllText(filePath, serial);
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Exported creature data: ");
        MonsterDBPlugin.MonsterDBLogger.LogDebug(filePath);
    }

    public static void Import()
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_importFolderPath)) Directory.CreateDirectory(m_importFolderPath);
        string[] files = Directory.GetFiles(m_importFolderPath);
        if (files.Length <= 0) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        int count = 0;
        foreach (string filePath in files)
        {
            try
            {
                string serial = File.ReadAllText(filePath);
                CreatureData data = deserializer.Deserialize<CreatureData>(serial);
                string creatureName = data.m_characterData.PrefabName;
                m_data[creatureName] = data;
                ++count;
            }
            catch
            {
                Helpers.LogParseFailure(filePath);
            }
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Imported {count} creature data files");
    }
}