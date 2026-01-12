using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;
using Object = UnityEngine.Object;

namespace MonsterDB.Solution.Methods;

public static class HumanoidMethods
{
    public static void ReadHumanoid(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "Character.yml";
        if (!File.Exists(filePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            CharacterData data = deserializer.Deserialize<CharacterData>(File.ReadAllText(filePath));
            creatureData.m_characterData = data;
        }
        catch
        {
            LogParseFailure(filePath);
        }
        
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (Directory.Exists(effectsFolder))
        {
            string hitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitEffects.yml";
            string critHitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "CritHitEffects.yml";
            string backstabHitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "BackstabHitEffects.yml";
            string deathEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "DeathEffects.yml";
            string waterEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "WaterEffects.yml";
            string tarEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TarEffects.yml";
            string slideEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "SlideEffects.yml";
            string jumpEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "JumpEffects.yml";
            string flyingContinuousEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "FlyingContinuousEffects.yml";
            string pickupEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "PickupEffects.yml";
            string dropEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "DropEffects.yml";
            string consumeItemEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "ConsumeItemEffects.yml";
            string equipEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "EquipEffects.yml";
            string perfectBlockEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "PerfectBlockEffects.yml";

            ReadEffectInfo(hitEffectsPath, ref creatureData.m_effects.m_hitEffects);
            ReadEffectInfo(critHitEffectsPath, ref creatureData.m_effects.m_critHitEffects);
            ReadEffectInfo(backstabHitEffectsPath, ref creatureData.m_effects.m_backstabHitEffects);
            ReadEffectInfo(deathEffectsPath, ref creatureData.m_effects.m_deathEffects);
            ReadEffectInfo(waterEffectsPath, ref creatureData.m_effects.m_waterEffects);
            ReadEffectInfo(tarEffectsPath, ref creatureData.m_effects.m_tarEffects);
            ReadEffectInfo(slideEffectsPath, ref creatureData.m_effects.m_slideEffects);
            ReadEffectInfo(jumpEffectsPath, ref creatureData.m_effects.m_jumpEffects);
            ReadEffectInfo(flyingContinuousEffectsPath, ref creatureData.m_effects.m_flyingContinuousEffects);
            ReadEffectInfo(pickupEffectsPath, ref creatureData.m_effects.m_pickupEffects);
            ReadEffectInfo(dropEffectsPath, ref creatureData.m_effects.m_dropEffects);
            ReadEffectInfo(consumeItemEffectsPath, ref creatureData.m_effects.m_consumeItemEffects);
            ReadEffectInfo(equipEffectsPath, ref creatureData.m_effects.m_equipEffects);
            ReadEffectInfo(perfectBlockEffectsPath, ref creatureData.m_effects.m_perfectBlockEffects);
        }
        string itemsPath = folderPath + Path.DirectorySeparatorChar + "Items";
        if (Directory.Exists(itemsPath))
        {
            string defaultItemsPath = itemsPath + Path.DirectorySeparatorChar + "DefaultItems";
            string randomWeaponPath = itemsPath + Path.DirectorySeparatorChar + "RandomWeapon";
            string randomArmorPath = itemsPath + Path.DirectorySeparatorChar + "RandomArmor";
            string randomShieldPath = itemsPath + Path.DirectorySeparatorChar + "RandomShield";
            string randomSetsPath = itemsPath + Path.DirectorySeparatorChar + "RandomSets";
            string randomItemsPath = itemsPath + Path.DirectorySeparatorChar + "RandomItems";
            
            ReadItems(defaultItemsPath, ref creatureData.m_defaultItems);
            ReadItems(randomWeaponPath, ref creatureData.m_randomWeapons);
            ReadItems(randomArmorPath, ref creatureData.m_randomArmors);
            ReadItems(randomShieldPath, ref creatureData.m_randomShields);
            ReadRandomSets(randomSetsPath, ref creatureData.m_randomSets);
            ReadRandomItems(randomItemsPath, ref creatureData.m_randomItems);
        }
    }

    private static void ReadItems(string folderPath, ref List<ItemAttackData> serverData)
    {
        if (!Directory.Exists(folderPath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string[] folders = Directory.GetDirectories(folderPath);
        foreach (string folder in folders)
        {
            string filePath = folder + Path.DirectorySeparatorChar + "ItemData.yml";
            if (!File.Exists(filePath)) continue;
            string serial = File.ReadAllText(filePath);
            if (serial.IsNullOrWhiteSpace()) continue;
            try
            {
                AttackData data = deserializer.Deserialize<AttackData>(serial);
                ItemAttackData itemAttackData = new() { m_attackData = data };
                ReadAllEffectInfo(folder, ref itemAttackData);

                serverData.Add(itemAttackData);
            }
            catch
            {
                LogParseFailure(filePath);
            }
        }
    }

    private static void ReadAllEffectInfo(string folderPath, ref ItemAttackData itemAttackData)
    {
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) return;
        string hitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitEffects.yml";
        string hitTerrainEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitTerrainEffects.yml";
        string blockEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "BlockEffects.yml";
        string startEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "StartEffects.yml";
        string holdStartEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HoldStartEffects.yml";
        string equipEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "EquipEffects.yml";
        string unequipEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "UnequipEffects.yml";
        string triggerEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TriggerEffects.yml";
        string trailStartEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TrailStartEffects.yml";
            
        ReadEffectInfo(hitEffectsPath, ref itemAttackData.m_effects.m_hitEffects);
        ReadEffectInfo(hitTerrainEffectsPath, ref itemAttackData.m_effects.m_hitTerrainEffects);
        ReadEffectInfo(blockEffectsPath, ref itemAttackData.m_effects.m_blockEffects);
        ReadEffectInfo(startEffectsPath, ref itemAttackData.m_effects.m_startEffects);
        ReadEffectInfo(holdStartEffectsPath, ref itemAttackData.m_effects.m_holdStartEffects);
        ReadEffectInfo(equipEffectsPath, ref itemAttackData.m_effects.m_equipEffects);
        ReadEffectInfo(unequipEffectsPath, ref itemAttackData.m_effects.m_unEquipEffects);
        ReadEffectInfo(triggerEffectsPath, ref itemAttackData.m_effects.m_triggerEffects);
        ReadEffectInfo(trailStartEffectsPath, ref itemAttackData.m_effects.m_trailStartEffects);
    }
    

    private static void ReadRandomSets(string folderPath,ref List<RandomItemSetsData> serverData)
    {
        if (!Directory.Exists(folderPath)) return;
        string[] folders = Directory.GetDirectories(folderPath);
        List<RandomItemSetsData> sets = new();
        foreach (string folder in folders)
        {
            string? setName = Path.GetFileName(folder);
            if (setName == null) continue;
            RandomItemSetsData set = new RandomItemSetsData
            {
                m_name = setName
            };
            ReadItems(folder, ref set.m_items);
            sets.Add(set);
        }

        serverData = sets;
    }
    

    private static void ReadRandomItems(string folderPath, ref List<RandomItemData> serverData)
    {
        if (!Directory.Exists(folderPath)) return;
        string filePath = folderPath + Path.DirectorySeparatorChar + "RandomItems.yml";
        if (!File.Exists(filePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string serial = File.ReadAllText(filePath);
        try
        {
            serverData = deserializer.Deserialize<List<RandomItemData>>(serial);
        }
        catch
        {
            LogParseFailure(filePath);
        }
    }
}