using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonsterDB.Solution.Methods;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

public static class LegacyManager
{
    private static readonly string LegacyFolderPath;
    public static readonly Dictionary<string, CreatureData> creaturesToConvert;

    static LegacyManager()
    {
        LegacyFolderPath = Path.Combine(ConfigManager.DirectoryPath, "Legacy");
        creaturesToConvert = new Dictionary<string, CreatureData>();
    }

    public static void ConvertAll(Terminal.ConsoleEventArgs args)
    {
        if (creaturesToConvert.Count <= 0)
        {
            args.Context.LogWarning("No legacy files found.");
            return;
        }
        Load();
        args.Context.AddString($"Converted {creaturesToConvert.Count} legacy file into v.0.2.x format");
    }

    public static void Convert(Terminal context, string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            context.LogWarning("Invalid parameters");
            return;
        }

        if (!creaturesToConvert.TryGetValue(input, out CreatureData data))
        {
            context.AddString($"No legacy creature data found for {input}");
            return;
        }

        Convert(data);
    }

    public static void Convert(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        if (!creaturesToConvert.TryGetValue(prefabName, out CreatureData data))
        {
            args.Context.LogWarning($"No legacy creature data found for {prefabName}");
            return;
        }

        Convert(data);
        args.Context.AddString($"Converted {prefabName} legacy file into v.0.2.x format");
    }

    public static List<string> GetConversionOptions(int i, string word) => i switch
    {
        2 => creaturesToConvert.Keys.ToList(),
        _ => new List<string>()
    };
    
    public static void Start()
    {
        ImportDirs();
        ImportFiles();

        if (creaturesToConvert.Count > 0)
        {
            MonsterDBPlugin.LogInfo($"Loaded {creaturesToConvert.Count} legacy files. Use command convert_all, to export to new file structure.");
        }
    }

    private static void Load()
    {
        foreach (CreatureData? data in creaturesToConvert.Values)
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

        creaturesToConvert[data.m_characterData.PrefabName] = data;

        return true;
    }

    private static void ImportDirs()
    {
        List<string> paths = new();
        string creaturePath = Path.Combine(ConfigManager.DirectoryPath, "Creatures");
        string clonePath = Path.Combine(ConfigManager.DirectoryPath, "Clones");
        if (Directory.Exists(LegacyFolderPath)) paths.AddRange(Directory.GetDirectories(LegacyFolderPath));
        if (Directory.Exists(clonePath)) paths.AddRange(Directory.GetDirectories(clonePath));
        if (Directory.Exists(creaturePath)) paths.AddRange(Directory.GetDirectories(creaturePath));
        foreach (string dir in paths)
        {
            Read(dir);
        }
    }
    
    private static void ImportFiles()
    {
        if (!Directory.Exists(LegacyFolderPath)) return;
        string[] files = Directory.GetFiles(LegacyFolderPath, "*.yml");
        if (files.Length <= 0) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        int count = 0;
        foreach (string filePath in files)
        {
            try
            {
                string text = File.ReadAllText(filePath);
                CreatureData data = deserializer.Deserialize<CreatureData>(text);
                creaturesToConvert[data.m_characterData.PrefabName] = data;
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
            prefab = CreatureManager.Clone(prefab, data.m_characterData.PrefabName, false);
            
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

        List<ItemAttackData> itemsToClone = data.GetAllItems();
        itemsToClone.RemoveAll(x => string.IsNullOrEmpty(x.m_attackData.OriginalPrefab));

        foreach (ItemAttackData? item in itemsToClone)
        {
            GameObject? itemPrefab = PrefabManager.GetPrefab(item.m_attackData.OriginalPrefab);
            if (itemPrefab == null) continue;
            ItemManager.TryClone(itemPrefab, item.m_attackData.Name, out _, false);
        }

        CreatureManager.TrySave(prefab, out Base? original, isClone, cloneFrom);
        LoadManager.originals.Remove(prefab.name);
        
        Update(prefab, data);
        CreatureManager.Write(prefab, isClone, cloneFrom);
        
        if (original != null)
        {
            LoadManager.originals[prefab.name] = original;
        }
    }
}