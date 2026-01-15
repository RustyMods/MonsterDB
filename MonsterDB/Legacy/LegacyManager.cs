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
            ItemManager.Clone(itemPrefab, item.m_attackData.Name, false);
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
            foreach (SpawnDataRef spawn in data.SpawnData)
            {
                spawn.m_name = $"MDB {data.Prefab} Spawn Data";
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
            old.m_effects.Set(ref data.Procreation);
        }
        if (data.NPCTalk != null)
        {
            old.m_npcTalk.Set(ref data.NPCTalk);
            old.m_effects.Set(ref data.NPCTalk);
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

                    if (!string.IsNullOrEmpty(old.m_characterData.ClonedFrom))
                    {
                        mat.m_name = $"MDB_{old.m_characterData.PrefabName}_{mat.m_name}";
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
                    ItemManager.Clone(prefab, item.m_attackData.Name);
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