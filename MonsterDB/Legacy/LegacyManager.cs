using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonsterDB.Solution.Methods;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution;

public static class LegacyManager
{
    private static readonly string LegacyFolderPath;
    private static readonly List<CreatureData> creaturesToConvert;

    static LegacyManager()
    {
        LegacyFolderPath = Path.Combine(ConfigManager.DirectoryPath, "Legacy");
        creaturesToConvert = new List<CreatureData>();
    }
    
    public static void Setup()
    {
        if (!Directory.Exists(LegacyFolderPath)) Directory.CreateDirectory(LegacyFolderPath);

        ImportDirs();
        ImportFiles();

        if (creaturesToConvert.Count > 0)
        {
            MonsterDBPlugin.LogInfo($"Loaded {creaturesToConvert.Count} legacy files. Use command convert_all, to export to new file structure.");
        }

        Command convert = new Command("convert_all", "", _ =>
        {
            if (creaturesToConvert.Count <= 0) return true;

            Load();
            
            return true;
        });
    }

    private static void Load()
    {
        foreach (CreatureData? data in creaturesToConvert)
        {
            Convert(data);
        }
    }
    
    private static bool Read(string folderPath)
    {
        CreatureData data = new();
        VisualMethods.ReadVisuals(folderPath, ref data);
        HumanoidMethods.ReadHumanoid(folderPath, ref data);
        CharacterMethods.ReadCharacter(folderPath, ref data);
        MonsterAIMethods.ReadMonsterAI(folderPath, ref data);
        AnimalAIMethods.ReadAnimalAI(folderPath, ref data);
        CharacterDropMethods.ReadCharacterDrops(folderPath, ref data);
        TameableMethods.ReadTameable(folderPath, ref data);
        ProcreationMethods.ReadProcreation(folderPath, ref data);
        NPCTalkMethods.ReadNPCTalk(folderPath, ref data);
        GrowUpMethods.ReadGrowUp(folderPath, ref data);
        LevelEffectsMethods.ReadLevelEffects(folderPath, ref data);
        
        creaturesToConvert.Add(data);

        return true;
    }

    private static void ImportDirs()
    {
        List<string> paths = new();
        string creaturePath = Path.Combine(ConfigManager.DirectoryPath, "Creatures");
        string clonePath = Path.Combine(ConfigManager.DirectoryPath, "Clones");
        paths.AddRange(Directory.GetDirectories(LegacyFolderPath));
        if (Directory.Exists(clonePath)) paths.AddRange(Directory.GetDirectories(clonePath));
        if (Directory.Exists(creaturePath)) paths.AddRange(Directory.GetDirectories(creaturePath));
        foreach (string dir in paths)
        {
            Read(dir);
        }
    }
    
    private static void ImportFiles()
    {
        if (!Directory.Exists(LegacyFolderPath)) Directory.CreateDirectory(LegacyFolderPath);
        string[] files = Directory.GetFiles(LegacyFolderPath, "*.yml");
        if (files.Length <= 0) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        int count = 0;
        foreach (string filePath in files)
        {
            try
            {
                string serial = File.ReadAllText(filePath);
                CreatureData data = deserializer.Deserialize<CreatureData>(serial);
                creaturesToConvert.Add(data);
                ++count;
            }
            catch
            {
                MonsterDBPlugin.LogWarning($"Failed to deserialize file: {Path.GetFileName(filePath)}");
            }
        }
        MonsterDBPlugin.LogDebug($"Imported {count} creature data files");
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

    private static void Convert(CreatureData data)
    {
        bool isClone = !string.IsNullOrEmpty(data.m_characterData.ClonedFrom);
        string cloneFrom = data.m_characterData.ClonedFrom;
        string prefabName = string.IsNullOrEmpty(data.m_characterData.ClonedFrom) ? data.m_characterData.PrefabName : data.m_characterData.ClonedFrom;
        GameObject? prefab = PrefabManager.GetPrefab(prefabName);
        if (prefab == null)
        {
            MonsterDBPlugin.LogWarning($"Legacy Conversion: Failed to find prefab: {prefabName}");
            return;
        }

        if (isClone)
        {
            CreatureManager.Clone(prefab, data.m_characterData.PrefabName, false);
            prefab = PrefabManager.GetPrefab(data.m_characterData.PrefabName);
            
            string ragdollName = $"MDB_{data.m_characterData.PrefabName}_ragdoll";
            foreach (EffectInfo? info in data.m_effects.m_deathEffects)
            {
                if (info.PrefabName.EndsWith("ragdoll"))
                {
                    info.PrefabName = ragdollName;
                }
            }
        }

        if (prefab == null) return;

        List<ItemAttackData> items = data.GetAllItems();
        items.RemoveAll(x => string.IsNullOrEmpty(x.m_attackData.OriginalPrefab));

        foreach (ItemAttackData? item in items)
        {
            GameObject? itemPrefab = PrefabManager.GetPrefab(item.m_attackData.OriginalPrefab);
            if (itemPrefab == null) continue;
            ItemManager.TryClone(itemPrefab, item.m_attackData.Name, out _, false);
        }

        CreatureManager.TrySave(prefab, out Base? original, isClone, cloneFrom);
        SyncManager.originals.Remove(prefab.name);
        
        Update(prefab, data);
        CreatureManager.Write(prefab, isClone, cloneFrom);
        
        if (original != null)
        {
            SyncManager.originals[prefab.name] = original;
        }
    }
}