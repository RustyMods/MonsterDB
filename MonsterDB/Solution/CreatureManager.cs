using System.Collections.Generic;
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

    private static readonly CustomSyncedValue<string> m_serverDataFiles = new(MonsterDBPlugin.ConfigSync, "MonsterDB_ServerFiles", "");
    private static readonly Dictionary<string, CreatureData> m_originalData = new();
    public static Dictionary<string, CreatureData> m_data = new();
    public static readonly List<GameObject> m_clones = new();

    private static FileSystemWatcher m_creatureWatcher = null!;
    private static FileSystemWatcher m_cloneWatcher = null!;


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

    public static bool IsClone(GameObject creature) => m_clones.Contains(creature);
    
    public static void Setup()
    {
        m_serverDataFiles.ValueChanged += ReadServerFiles;
        SetupFileWatch();
    }

    private static void SetupFileWatch()
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_creatureFolderPath)) Directory.CreateDirectory(m_creatureFolderPath);
        if (!Directory.Exists(m_cloneFolderPath)) Directory.CreateDirectory(m_cloneFolderPath);
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
    }

    private static void OnCloneFileChange(object sender, FileSystemEventArgs e)
    {
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
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            m_data = deserializer.Deserialize<Dictionary<string, CreatureData>>(m_serverDataFiles.Value);
            Initialization.RemoveAll();
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
        var ragdollName = critter.name + "_ragdoll";
        var ragdoll = DataBase.TryGetGameObject(ragdollName);
        if (ragdoll != null)
        {
            Delete(ragdoll, loop);
        }
        Helpers.RemoveFromZNetScene(critter);
        Helpers.RemoveFromObjectDB(critter);
        if (!loop) m_clones.Remove(critter);
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
        m_originalData[critter.name] = data;
    }

    public static void Write(GameObject critter, out string folderPath, bool clone = false, string clonedFrom = "")
    {
        m_creatureWatcher.EnableRaisingEvents = false;
        m_cloneWatcher.EnableRaisingEvents = false;
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_creatureFolderPath)) Directory.CreateDirectory(m_creatureFolderPath);
        folderPath = (clone ? m_cloneFolderPath : m_creatureFolderPath) + Path.DirectorySeparatorChar + critter.name;

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
        
        m_creatureWatcher.EnableRaisingEvents = true;
        m_cloneWatcher.EnableRaisingEvents = true;
    }

    public static void Read(string creatureName, bool isClone = false)
    {
        string folderPath = (isClone ? m_cloneFolderPath : m_creatureFolderPath) + Path.DirectorySeparatorChar + creatureName;
        if (!Directory.Exists(folderPath)) return;
        
        CreatureData creatureData = new();
        VisualMethods.Read(folderPath, ref creatureData);
        HumanoidMethods.Read(folderPath, ref creatureData);
        CharacterMethods.Read(folderPath, ref creatureData);
        MonsterAIMethods.Read(folderPath, ref creatureData);
        AnimalAIMethods.Read(folderPath, ref creatureData);
        CharacterDropMethods.Read(folderPath, ref creatureData);
        TameableMethods.Read(folderPath, ref creatureData);
        ProcreationMethods.Read(folderPath, ref creatureData);
        NPCTalkMethods.Read(folderPath, ref creatureData);
        m_data[creatureName] = creatureData;
        UpdateServer();
    }

    public static void Update(GameObject critter)
    {
        if (!m_data.TryGetValue(critter.name, out CreatureData data)) return;
        Save(critter, data.m_characterData.ClonedFrom);
        VisualMethods.Update(critter, data);
        TameableMethods.Update(critter, data);
        HumanoidMethods.Update(critter, data);
        CharacterMethods.Update(critter, data);
        MonsterAIMethods.Update(critter, data);
        AnimalAIMethods.Update(critter, data);
        CharacterDropMethods.Update(critter, data);
        ProcreationMethods.Update(critter, data);
        NPCTalkMethods.Update(critter, data);
    }

    public static void Reset(GameObject critter)
    {
        if (!m_originalData.TryGetValue(critter.name, out CreatureData data)) return;
        VisualMethods.Update(critter, data);
        HumanoidMethods.Update(critter, data);
        CharacterMethods.Update(critter, data);
        MonsterAIMethods.Update(critter, data);
        AnimalAIMethods.Update(critter, data);
        CharacterDropMethods.Update(critter, data);
        TameableMethods.Update(critter, data);
        ProcreationMethods.Update(critter, data);
        NPCTalkMethods.Update(critter, data);
        MonsterDBPlugin.MonsterDBLogger.LogInfo("Reset: " + critter.name);
    }

    public static void Clone(GameObject critter, string name, bool saveToDisk = true)
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_cloneFolderPath)) Directory.CreateDirectory(m_cloneFolderPath);
        
        GameObject clone = Object.Instantiate(critter, MonsterDBPlugin.m_root.transform, false);
        clone.name = name;
        Dictionary<string, Material> ragDollMats = VisualMethods.CloneMaterials(clone);
        CharacterMethods.CloneRagDoll(clone, ragDollMats);
        HumanoidMethods.CloneRagDoll(clone, ragDollMats);
        HumanoidMethods.CloneItems(clone);
        if (saveToDisk)
        {
            Write(clone, out string folderPath, true, critter.name);
            Read(clone.name, true);
            MonsterDBPlugin.MonsterDBLogger.LogInfo($"Cloned {critter.name} as {clone.name}");
            MonsterDBPlugin.MonsterDBLogger.LogInfo(folderPath);
        }
        Helpers.RegisterToZNetScene(clone);
        m_clones.Add(clone);
    }
}