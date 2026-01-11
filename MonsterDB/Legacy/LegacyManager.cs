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
        foreach (var data in creaturesToConvert)
        {
            Convert(data);
        }
    }
    
    private static bool Read(string folderPath)
    {
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
        
        creaturesToConvert.Add(data);

        return true;
    }

    private static void ImportDirs()
    {
        string[] dirs = Directory.GetDirectories(LegacyFolderPath);
        foreach (string dir in dirs)
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

    private static void Convert(CreatureData data)
    {
        string prefabName = string.IsNullOrEmpty(data.m_characterData.ClonedFrom) ? data.m_characterData.PrefabName : data.m_characterData.ClonedFrom;
        GameObject? prefab = PrefabManager.GetPrefab(prefabName);
        if (prefab == null)
        {
            MonsterDBPlugin.LogWarning($"Legacy Conversion: Failed to find prefab: {prefabName}");
            return;
        }
        
        Character? c = prefab.GetComponent<Character>();
        BaseAI ai = prefab.GetComponent<BaseAI>();
        if (c == null || ai == null)
        {
            MonsterDBPlugin.LogWarning("Invalid prefab, missing Character or AI component");
            return;
        }

        string filePath = Path.Combine(FileManager.ExportFolder,
            $"{data.m_characterData.PrefabName}_Legacy_Converted.yml");
        
        if (c is Human && ai is MonsterAI)
        {
            BaseHuman creature = new BaseHuman();
            creature.Setup(prefab, !string.IsNullOrEmpty(data.m_characterData.ClonedFrom), data.m_characterData.ClonedFrom);
            Set(creature, data);
            var text = ConfigManager.Serialize(creature);
            File.WriteAllText(filePath, text);
            MonsterDBPlugin.LogInfo($"Converted Legacy: {creature.Prefab} to {filePath}");
        }
        else if (c is Humanoid && ai is MonsterAI)
        {
            BaseHumanoid creature = new BaseHumanoid();
            creature.Setup(prefab, !string.IsNullOrEmpty(data.m_characterData.ClonedFrom), data.m_characterData.ClonedFrom);
            Set(creature, data);
            var text = ConfigManager.Serialize(creature);
            File.WriteAllText(filePath, text);
            MonsterDBPlugin.LogInfo($"Converted Legacy: {creature.Prefab} to {filePath}");
        }
        else if (ai is AnimalAI)
        {
            BaseCharacter creature = new BaseCharacter();
            creature.Setup(prefab, !string.IsNullOrEmpty(data.m_characterData.ClonedFrom), data.m_characterData.ClonedFrom);
            Set(creature, data);
            var text = ConfigManager.Serialize(creature);
            File.WriteAllText(filePath, text);
            MonsterDBPlugin.LogInfo($"Converted Legacy: {creature.Prefab} to {filePath}");  
        }
    }

    private static void Set(BaseCharacter data, CreatureData old)
    {
        old.m_characterData.Set(ref data.Character);
        old.m_animalAIData.Set(ref data.AI);
        old.m_effects.Set(ref data.Character);

        SetBase(data, old);
    }

    private static void SetBase(Base data, CreatureData old)
    {
        data.GameVersion = Version.GetVersionString();
        data.ModVersion = "0.1.5";
        data.Prefab = old.m_characterData.PrefabName;
        data.ClonedFrom = old.m_characterData.ClonedFrom;
        data.IsCloned = !string.IsNullOrEmpty(old.m_characterData.ClonedFrom);

        if (data.IsCloned && data.SpawnData != null)
        {
            foreach (var spawn in data.SpawnData)
            {
                spawn.m_prefab = data.Prefab;
            }
        }
        
        old.m_tameable.Set(ref data.Tameable);
        old.m_effects.Set(ref data.Tameable);
        if (data.Drops != null && old.m_characterDrops.Count != 0)
        {
            data.Drops.m_drops = old.m_characterDrops.Select(x => x.ToRef()).ToList();
        }
        if (data.Procreation != null)
        {
            old.m_procreation.Set(ref data.Procreation);
        }
        if (data.NPCTalk != null)
        {
            old.m_npcTalk.Set(ref data.NPCTalk);
        }

        if (data.GrowUp != null)
        {
            old.m_growUp.Set(ref data.GrowUp);
        }

        if (data.Visuals != null)
        {
            if (old.m_levelEffects.Count != 0)
            {
                data.Visuals.m_levelSetups = old.m_levelEffects.Select(x => x.ToRef()).ToList();
            }

            data.Visuals.m_scale = old.m_scale.ToRef();
        }
        
        SetMaterials(data, old);
    }

    private static void SetMaterials(Base data, CreatureData old)
    {
        if (data.Visuals == null || data.Visuals.m_renderers == null) return;
        foreach (RendererRef renderer in data.Visuals.m_renderers)
        {
            if (renderer.m_materials != null)
            {
                foreach (MaterialRef mat in renderer.m_materials)
                {
                    if (old.m_materials.TryGetValue(mat.m_name, out VisualMethods.MaterialData? d))
                    {
                        d.Set(mat);
                    }
                }
            }
        }
    }

    private static void SetBaseHumanoid(BaseHumanoid data, CreatureData old)
    {
        if (data.Character != null)
        {
            List<ItemAttackData> items = new();
            if (old.m_defaultItems.Count != 0)
            {
                data.Character.m_defaultItems = old.m_defaultItems.Select(x => x.m_attackData.Name).ToArray();
                items.AddRange(old.m_defaultItems);
            }

            if (old.m_randomWeapons.Count != 0)
            {
                data.Character.m_randomWeapon = old.m_randomWeapons.Select(x => x.m_attackData.Name).ToArray();
                items.AddRange(old.m_randomWeapons);
            }

            if (old.m_randomArmors.Count != 0)
            {
                data.Character.m_randomArmor = old.m_randomArmors.Select(x => x.m_attackData.Name).ToArray();
                items.AddRange(old.m_randomArmors);
            }

            if (old.m_randomShields.Count != 0)
            {
                data.Character.m_randomShield = old.m_randomShields.Select(x => x.m_attackData.Name).ToArray();
                items.AddRange(old.m_randomShields);
            }

            if (old.m_randomItems.Count != 0)
            {
                data.Character.m_randomItems = old.m_randomItems.Select(x => x.ToRef()).ToArray();
            }

            if (old.m_randomSets.Count != 0)
            {
                data.Character.m_randomSets = old.m_randomSets.Select(x => x.ToRef()).ToArray();
                items.Add(old.m_randomSets.SelectMany(i => i.m_items).ToArray());
            }

            foreach (ItemAttackData? item in items)
            {
                if (!string.IsNullOrEmpty(item.m_attackData.OriginalPrefab))
                {
                    GameObject? prefab = PrefabManager.GetPrefab(item.m_attackData.OriginalPrefab);
                    if (prefab == null) continue;
                    BaseItem reference = new BaseItem();
                    reference.Setup(prefab, true, item.m_attackData.OriginalPrefab);
                    if (reference.ItemData != null)
                    {
                        item.Set(reference.ItemData);
                        reference.ItemData.m_prefab = item.m_attackData.Name;
                    }
                    reference.ModVersion = "0.1.5";
                    reference.Prefab = item.m_attackData.Name;

                    string dir = Path.Combine(FileManager.ExportFolder, old.m_characterData.PrefabName + " items");
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    var filePath = Path.Combine(dir,
                        $"{item.m_attackData.Name}_Legacy_Converted.yml");
                    string text = ConfigManager.Serialize(reference);
                    File.WriteAllText(filePath, text);
                }
            }

            Dictionary<string, ItemAttackData> dict = items.ToDictionary(f => f.m_attackData.Name);

            if (data.Character.m_attacks != null)
            {
                foreach (ItemDataSharedRef item in data.Character.m_attacks)
                {
                    if (dict.TryGetValue(item.m_prefab, out ItemAttackData? itemData))
                    {
                        itemData.Set(item);
                    }
                }
            }
        }
    }

    private static void Set(BaseHuman data, CreatureData old)
    {
        old.m_characterData.Set(ref data.Character);
        old.m_monsterAIData.Set(ref data.AI);
        old.m_effects.Set(ref data.Character);
        old.m_effects.Set(ref data.AI);
        SetBase(data, old);
        SetBaseHumanoid(data, old);
    }

    private static void Set(BaseHumanoid data, CreatureData old)
    {
        old.m_characterData.Set(ref data.Character);
        old.m_monsterAIData.Set(ref data.AI);
        old.m_effects.Set(ref data.Character);
        old.m_effects.Set(ref data.AI);

        SetBase(data, old);
        SetBaseHumanoid(data, old);
    }
}