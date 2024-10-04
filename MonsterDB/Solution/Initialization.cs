using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using MonsterDB.Solution.Methods;
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
            HumanMan.Create("Human");
            CloneAll(true);
            UpdateAll(true);
            SpawnMan.UpdateSpawnData();
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    private static class Game_Logout_Patch
    {
        private static void Prefix()
        {
            ResetAll();
            RemoveAllClones();
            SpawnMan.ClearSpawns();
        }
    }

    public static void ReadLocalFiles()
    {
        if (!Directory.Exists(CreatureManager.m_folderPath)) Directory.CreateDirectory(CreatureManager.m_folderPath);
        if (!Directory.Exists(CreatureManager.m_creatureFolderPath)) Directory.CreateDirectory(CreatureManager.m_creatureFolderPath);
        if (!Directory.Exists(CreatureManager.m_cloneFolderPath)) Directory.CreateDirectory(CreatureManager.m_cloneFolderPath);
        
        CreatureManager.Import();
        ReadCreatureDirectory();
        ReadCloneDirectory();
    }

    private static void ReadCreatureDirectory()
    {
        string[] creatureFolders = Directory.GetDirectories(CreatureManager.m_creatureFolderPath);
        int count = 0;
        foreach (string folder in creatureFolders)
        {
            string name = Path.GetFileName(folder);
            CreatureManager.Read(name);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Registered {count} creature directories");
    }

    private static void ReadCloneDirectory()
    {
        int count = 0;
        string[] cloneFolders = Directory.GetDirectories(CreatureManager.m_cloneFolderPath);
        foreach (string folder in cloneFolders)
        {
            string name = Path.GetFileName(folder);
            CreatureManager.Read(name, true);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Registered {count} cloned creature directories");
    }
    

    public static void ResetAll()
    {
        int count = 0;

        foreach (KeyValuePair<string, CreatureData> kvp in CreatureManager.m_data)
        {
            GameObject? prefab = DataBase.TryGetGameObject(kvp.Value.m_characterData.PrefabName);
            if (prefab == null) continue;
            CreatureManager.Reset(prefab);
            ++count;
        }
        
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Reset {count} creature data");
    }

    public static void RemoveAllClones()
    {
        int count = 0;

        foreach (KeyValuePair<string, GameObject> kvp in CreatureManager.m_clones)
        {
            CreatureManager.Delete(kvp.Value, true);
            ++count;
        }
        CreatureManager.m_clones.Clear();
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Removed {count} clones");
    }

    public static void CloneAll(bool local = false)
    {
        int count = 0;

        foreach (KeyValuePair<string, CreatureData> kvp in local ? CreatureManager.m_localData : CreatureManager.m_data)
        {
            CloneAllItems(kvp.Value);
            string originalCreature = kvp.Value.m_characterData.ClonedFrom;
            if (originalCreature.IsNullOrWhiteSpace()) continue;
            GameObject? prefab = DataBase.TryGetGameObject(originalCreature);
            if (prefab == null) continue;
            string name = kvp.Value.m_characterData.PrefabName;
            CreatureManager.Clone(prefab, name, false, false);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Cloned {count} creatures");
    }

    private static void CloneAllItems(CreatureData data)
    {
        CloneItems(data.m_defaultItems);
        CloneItems(data.m_randomWeapons);
        CloneItems(data.m_randomShields);
        CloneItems(data.m_randomArmors);
        foreach (RandomItemSetsData set in data.m_randomSets) CloneItems(set.m_items);
    }

    private static void CloneItems(List<ItemAttackData> data)
    {
        foreach (ItemAttackData itemData in data)
        {
            if (itemData.m_attackData.OriginalPrefab.IsNullOrWhiteSpace()) continue;
            GameObject? item = DataBase.TryGetGameObject(itemData.m_attackData.OriginalPrefab);
            if (item == null) continue;
            ItemDataMethods.Clone(item, itemData.m_attackData.Name, false);
        }
    }
    

    public static void UpdateAll(bool local = false)
    {
        if (!ZNetScene.instance || !ObjectDB.instance) return;
        int count = 0;
        foreach (KeyValuePair<string, CreatureData> kvp in local ? CreatureManager.m_localData : CreatureManager.m_data)
        {
            GameObject? creature = DataBase.TryGetGameObject(kvp.Value.m_characterData.PrefabName);
            if (creature == null) continue;
            CreatureManager.Update(creature, local);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Updated {count} creatures");
    }
}