using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB.Solution;

public static class Initialization
{
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix(ObjectDB __instance)
        {
            if (!__instance || !ZNetScene.instance) return;
            HumanMan.Create();
            CloneAll();
            UpdateAll();
            SpawnMan.UpdateSpawnData();
        }
    }

    public static void ReadLocalFiles()
    {
        if (!Directory.Exists(CreatureManager.m_folderPath)) Directory.CreateDirectory(CreatureManager.m_folderPath);
        if (!Directory.Exists(CreatureManager.m_creatureFolderPath)) Directory.CreateDirectory(CreatureManager.m_creatureFolderPath);
        string[] creatureFolders = Directory.GetDirectories(CreatureManager.m_creatureFolderPath);
        int count = 0;
        foreach (string folder in creatureFolders)
        {
            string name = folder.Replace(CreatureManager.m_creatureFolderPath, string.Empty).Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
            CreatureManager.Read(name);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Registered {count} creature directories");
        count = 0;
        if (!Directory.Exists(CreatureManager.m_cloneFolderPath)) Directory.CreateDirectory(CreatureManager.m_cloneFolderPath);
        string[] cloneFolders = Directory.GetDirectories(CreatureManager.m_cloneFolderPath);
        foreach (string folder in cloneFolders)
        {
            string name = folder.Replace(CreatureManager.m_cloneFolderPath, string.Empty).Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
            CreatureManager.Read(name, true);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Registered {count} cloned creature directories");
    }

    public static void RemoveAll()
    {
        int count = 0;

        foreach (GameObject clone in CreatureManager.m_clones)
        {
            CreatureManager.Delete(clone, true);
            ++count;
        }
        CreatureManager.m_clones.Clear();
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Removed {count} clones");
    }

    public static void CloneAll()
    {
        int count = 0;

        foreach (KeyValuePair<string, CreatureData> kvp in CreatureManager.m_data)
        {
            string originalCreature = kvp.Value.m_characterData.ClonedFrom;
            if (originalCreature.IsNullOrWhiteSpace()) continue;
            GameObject? prefab = DataBase.TryGetGameObject(originalCreature);
            if (prefab == null) continue;
            string name = kvp.Value.m_characterData.PrefabName;
            CreatureManager.Clone(prefab, name, false);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Cloned {count} creatures");
    }

    public static void UpdateAll()
    {
        if (!ZNetScene.instance || !ObjectDB.instance) return;
        int count = 0;
        foreach (var kvp in CreatureManager.m_data)
        {
            GameObject? creature = DataBase.TryGetGameObject(kvp.Value.m_characterData.PrefabName);
            if (creature == null) continue;
            CreatureManager.Update(creature);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Updated {count} creatures");
    }
}