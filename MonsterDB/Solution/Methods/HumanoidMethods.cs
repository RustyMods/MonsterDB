using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using MonsterDB.Solution.Behaviors;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;
using Object = UnityEngine.Object;

namespace MonsterDB.Solution.Methods;

public static class HumanoidMethods
{
    public static void Save(GameObject critter, string clonedFrom, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out Humanoid component)) return;
        creatureData.m_characterData = RecordCharacterData(component, clonedFrom, critter);;
        SaveEffectList(component.m_hitEffects, ref creatureData.m_effects.m_hitEffects);
        SaveEffectList(component.m_critHitEffects, ref creatureData.m_effects.m_critHitEffects);
        SaveEffectList(component.m_backstabHitEffects, ref creatureData.m_effects.m_backstabHitEffects);
        SaveEffectList(component.m_deathEffects, ref creatureData.m_effects.m_deathEffects);
        SaveEffectList(component.m_waterEffects, ref creatureData.m_effects.m_waterEffects);
        SaveEffectList(component.m_tarEffects, ref creatureData.m_effects.m_tarEffects);
        SaveEffectList(component.m_slideEffects, ref creatureData.m_effects.m_slideEffects);
        SaveEffectList(component.m_jumpEffects, ref creatureData.m_effects.m_jumpEffects);
        SaveEffectList(component.m_flyingContinuousEffect, ref creatureData.m_effects.m_flyingContinuousEffects);
        SaveEffectList(component.m_pickupEffects, ref creatureData.m_effects.m_pickupEffects);
        SaveEffectList(component.m_dropEffects, ref creatureData.m_effects.m_dropEffects);
        SaveEffectList(component.m_consumeItemEffects, ref creatureData.m_effects.m_consumeItemEffects);
        SaveEffectList(component.m_equipEffects, ref creatureData.m_effects.m_equipEffects);
        SaveEffectList(component.m_perfectBlockEffect, ref creatureData.m_effects.m_perfectBlockEffects);
        
        SaveItemData(component.m_defaultItems, ref creatureData.m_defaultItems);
        SaveItemData(component.m_randomWeapon, ref creatureData.m_randomWeapons);
        SaveItemData(component.m_randomArmor, ref creatureData.m_randomArmors);
        SaveItemData(component.m_randomShield, ref creatureData.m_randomShields);
        SaveRandomItems(component.m_randomItems, ref creatureData.m_randomItems);
        SaveRandomSets(component.m_randomSets, ref creatureData.m_randomSets);
        
        if (component.m_deathEffects != null)
        {
            foreach (var effect in component.m_deathEffects.m_effectPrefabs)
            {
                if (!effect.m_prefab.GetComponent<Ragdoll>()) continue;
                var scale = effect.m_prefab.transform.localScale;
                VisualMethods.ScaleData scaleData = new VisualMethods.ScaleData()
                {
                    x = scale.x, y = scale.y, z = scale.z
                };
                creatureData.m_ragdollScale = scaleData;
            }
        }
    }

    private static CharacterData RecordCharacterData(Humanoid component, string clonedFrom, GameObject critter)
    {
        CharacterData data = new()
        {
            ClonedFrom = clonedFrom,
            PrefabName = critter.name,
            Name = component.m_name,
            Group = component.m_group,
            Faction = component.m_faction.ToString(),
            Boss = component.m_boss,
            DoNotHideBossHUD = component.m_dontHideBossHud,
            BossEvent = component.m_bossEvent,
            DefeatSetGlobalKey = component.m_defeatSetGlobalKey,
            AISkipTarget = component.m_aiSkipTarget,
            CrouchSpeed = component.m_crouchSpeed,
            WalkSpeed = component.m_walkSpeed,
            Speed = component.m_speed,
            TurnSpeed = component.m_turnSpeed,
            RunSpeed = component.m_runSpeed,
            RunTurnSpeed = component.m_runTurnSpeed,
            FlySlowSpeed = component.m_flySlowSpeed,
            FlyFastSpeed = component.m_flyFastSpeed,
            FlyTurnSpeed = component.m_flyTurnSpeed,
            Acceleration = component.m_acceleration,
            JumpForce = component.m_jumpForce,
            JumpForceForward = component.m_jumpForceForward,
            JumpForceTiredFactor = component.m_jumpForceTiredFactor,
            AirControl = component.m_airControl,
            CanSwim = component.m_canSwim,
            SwimDepth = component.m_swimDepth,
            SwimTurnSpeed = component.m_swimTurnSpeed,
            SwimAcceleration = component.m_swimAcceleration,
            GroundTilt = component.m_groundTilt.ToString(),
            GroundTiltSpeed = component.m_groundTiltSpeed,
            Flying = component.m_flying,
            JumpStaminaUsage = component.m_jumpStaminaUsage,
            DisableWhileSleeping = component.m_disableWhileSleeping,
            TolerateWater = component.m_tolerateWater,
            TolerateFire = component.m_tolerateFire,
            TolerateSmoke = component.m_tolerateSmoke,
            TolerateTar = component.m_tolerateTar,
            Health = component.m_health,
            BluntResistance = component.m_damageModifiers.m_blunt.ToString(),
            SlashResistance = component.m_damageModifiers.m_slash.ToString(),
            PierceResistance = component.m_damageModifiers.m_pierce.ToString(),
            ChopResistance = component.m_damageModifiers.m_chop.ToString(),
            PickaxeResistance = component.m_damageModifiers.m_pickaxe.ToString(),
            FireResistance = component.m_damageModifiers.m_fire.ToString(),
            FrostResistance = component.m_damageModifiers.m_frost.ToString(),
            LightningResistance = component.m_damageModifiers.m_lightning.ToString(),
            PoisonResistance = component.m_damageModifiers.m_poison.ToString(),
            SpiritResistance = component.m_damageModifiers.m_spirit.ToString(),
            StaggerWhenBlocked = component.m_staggerWhenBlocked,
            StaggerDamageFactor = component.m_staggerDamageFactor
        };
        return data;
    }
    
    public static void Write(GameObject critter, string folderPath, string clonedFrom = "")
    {
        if (!critter.TryGetComponent(out Humanoid component)) return;
        CharacterData data = RecordCharacterData(component, clonedFrom, critter);
        string fileName = "Character.yml";
        string filePath = folderPath + Path.DirectorySeparatorChar + fileName;
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);

        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);

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
        
        WriteEffectList(component.m_hitEffects, hitEffectsPath);
        WriteEffectList(component.m_critHitEffects, critHitEffectsPath);
        WriteEffectList(component.m_backstabHitEffects, backstabHitEffectsPath);
        WriteEffectList(component.m_deathEffects, deathEffectsPath);
        WriteEffectList(component.m_waterEffects, waterEffectsPath);
        WriteEffectList(component.m_tarEffects, tarEffectsPath);
        WriteEffectList(component.m_slideEffects, slideEffectsPath);
        WriteEffectList(component.m_jumpEffects, jumpEffectsPath);
        WriteEffectList(component.m_flyingContinuousEffect, flyingContinuousEffectsPath);
        WriteEffectList(component.m_pickupEffects, pickupEffectsPath);
        WriteEffectList(component.m_dropEffects, dropEffectsPath);
        WriteEffectList(component.m_consumeItemEffects, consumeItemEffectsPath);
        WriteEffectList(component.m_equipEffects, equipEffectsPath);
        WriteEffectList(component.m_perfectBlockEffect, perfectBlockEffectsPath);

        if (component.m_deathEffects != null)
        {
            foreach (var effect in component.m_deathEffects.m_effectPrefabs)
            {
                if (!effect.m_prefab.GetComponent<Ragdoll>()) continue;
                var scale = effect.m_prefab.transform.localScale;
                VisualMethods.ScaleData scaleData = new VisualMethods.ScaleData()
                {
                    x = scale.x, y = scale.y, z = scale.z
                };
                string visualFolderPath = folderPath + Path.DirectorySeparatorChar + "Visual";
                if (!Directory.Exists(visualFolderPath)) Directory.CreateDirectory(visualFolderPath);
                string ragdollscalePath = visualFolderPath + Path.DirectorySeparatorChar + "RagdollScale.yml";
                File.WriteAllText(ragdollscalePath, serializer.Serialize(scaleData));
            }
        }

        string itemsPath = folderPath + Path.DirectorySeparatorChar + "Items";
        if (!Directory.Exists(itemsPath)) Directory.CreateDirectory(itemsPath);

        string defaultItemsPath = itemsPath + Path.DirectorySeparatorChar + "DefaultItems";
        string randomWeaponPath = itemsPath + Path.DirectorySeparatorChar + "RandomWeapon";
        string randomArmorPath = itemsPath + Path.DirectorySeparatorChar + "RandomArmor";
        string randomShieldPath = itemsPath + Path.DirectorySeparatorChar + "RandomShield";
        string randomSetsPath = itemsPath + Path.DirectorySeparatorChar + "RandomSets";
        string randomItemsPath = itemsPath + Path.DirectorySeparatorChar + "RandomItems";

        List<string> itemsDir = new()
        {
            defaultItemsPath, randomWeaponPath, randomArmorPath,
            randomShieldPath, randomSetsPath, randomItemsPath
        };
        foreach (string folder in itemsDir.Where(folder => !Directory.Exists(folder))) Directory.CreateDirectory(folder);
        
        WriteItemData(component.m_defaultItems, defaultItemsPath, critter);
        WriteItemData(component.m_randomWeapon, randomWeaponPath, critter);
        WriteItemData(component.m_randomArmor, randomArmorPath, critter);
        WriteItemData(component.m_randomShield, randomShieldPath, critter);
        WriteRandomItems(component.m_randomItems, randomItemsPath);
        WriteRandomSets(component.m_randomSets, randomSetsPath, critter);
    }

    private static void SaveRandomSets(Humanoid.ItemSet[]? sets, ref List<RandomItemSetsData> list)
    {
        if (sets is not { Length: > 0 }) return;
        foreach (Humanoid.ItemSet set in sets)
        {
            if (set.m_name.IsNullOrWhiteSpace()) continue;
            RandomItemSetsData data = new()
            {
                m_name = set.m_name
            };
            SaveItemData(set.m_items, ref data.m_items);
            list.Add(data);
        }
    }

    private static void WriteRandomSets(Humanoid.ItemSet[]? sets, string parentFolder, GameObject critter)
    {
        if (sets is not { Length: > 0 }) return;
        int count = 0;
        foreach (Humanoid.ItemSet set in sets)
        {
            if (set.m_name.IsNullOrWhiteSpace()) continue;
            string folderPath = parentFolder + Path.DirectorySeparatorChar + set.m_name;
            if (Directory.Exists(folderPath))
            {
                folderPath += "_" + count;
                ++count;
            }
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            WriteItemData(set.m_items, folderPath, critter);
        }
    }

    private static void SaveRandomItems(Humanoid.RandomItem[]? items, ref List<RandomItemData> list)
    {
        if (items is not { Length: > 0 }) return;
        foreach (Humanoid.RandomItem? item in items)
        {
            if (item.m_prefab == null) continue;
            list.Add(new RandomItemData()
            {
                PrefabName = item.m_prefab.name,
                Chance = item.m_chance
            });
        }
    }

    private static void WriteRandomItems(Humanoid.RandomItem[]? items, string parentFolder)
    {
        if (items is not { Length: > 0 }) return;
        List<RandomItemData> data = new();
        foreach (Humanoid.RandomItem? item in items)
        {
            if (item.m_prefab == null) continue;
            data.Add(new RandomItemData()
            {
                PrefabName = item.m_prefab.name,
                Chance = item.m_chance
            });
        }

        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        string filePath = parentFolder + Path.DirectorySeparatorChar + "RandomItems.yml";
        File.WriteAllText(filePath, serial);
    }

    public static AttackData RecordAttackData(ItemDrop component)
    {
        var shared = component.m_itemData.m_shared;
        var data = new AttackData();
        data.Name = component.name;
        data.AnimationState = shared.m_animationState.ToString();
        data.ToolTier = shared.m_toolTier;
        data.Damage = shared.m_damages.m_damage;
        data.Blunt = shared.m_damages.m_blunt;
        data.Slash = shared.m_damages.m_slash;
        data.Pierce = shared.m_damages.m_pierce;
        data.Chop = shared.m_damages.m_chop;
        data.Fire = shared.m_damages.m_fire;
        data.Frost = shared.m_damages.m_frost;
        data.Lightning = shared.m_damages.m_lightning;
        data.Poison = shared.m_damages.m_poison;
        data.Spirit = shared.m_damages.m_spirit;
        data.AttackForce = shared.m_attackForce;
        data.Dodgeable = shared.m_dodgeable;
        data.Blockable = shared.m_blockable;
        if (shared.m_spawnOnHit) data.SpawnOnHit = shared.m_spawnOnHit.name;
        if (shared.m_spawnOnHitTerrain) data.SpawnOnHitTerrain = shared.m_spawnOnHitTerrain.name;
        if (shared.m_attackStatusEffect) data.AttackStatusEffect = shared.m_attackStatusEffect.name;
        data.AttackStatusEffectChance = shared.m_attackStatusEffectChance;
        data.AttackType = shared.m_attack.m_attackType.ToString();
        data.AttackAnimation = shared.m_attack.m_attackAnimation;
        data.HitTerrain = shared.m_attack.m_hitTerrain;
        data.HitFriendly = shared.m_attack.m_hitFriendly;
        data.DamageMultiplier = shared.m_attack.m_damageMultiplier;
        data.DamageMultiplierPerMissingHP = shared.m_attack.m_damageMultiplierPerMissingHP;
        data.DamageMultiplierByTotalHealthMissing = shared.m_attack.m_damageMultiplierByTotalHealthMissing;
        data.ForceMultiplier = shared.m_attack.m_forceMultiplier;
        data.StaggerMultiplier = shared.m_attack.m_staggerMultiplier;
        data.RecoilPushback = shared.m_attack.m_recoilPushback;
        data.SelfDamage = shared.m_attack.m_selfDamage;
        data.AttackOriginJoint = shared.m_attack.m_attackOriginJoint;
        data.AttackRange = shared.m_attack.m_attackRange;
        data.AttackHeight = shared.m_attack.m_attackHeight;
        data.AttackOffset = shared.m_attack.m_attackOffset;
        if (shared.m_attack.m_spawnOnTrigger) data.SpawnOnTrigger = shared.m_attack.m_spawnOnTrigger.name;
        data.ToggleFlying = shared.m_attack.m_toggleFlying;
        data.Attach = shared.m_attack.m_attach;
        data.AttackAngle = shared.m_attack.m_attackAngle;
        data.AttackRayWidth = shared.m_attack.m_attackRayWidth;
        data.MaxYAngle = shared.m_attack.m_maxYAngle;
        data.LowerDamagePerHit = shared.m_attack.m_lowerDamagePerHit;
        data.HitThroughWalls = shared.m_attack.m_hitThroughWalls;
        data.AttackRangeMinimum = shared.m_aiAttackRangeMin;
        data.AttackInterval = shared.m_aiAttackInterval;
        data.AttackMaxAngle = shared.m_aiAttackMaxAngle;
        if (shared.m_attack.m_attackProjectile)
        {
            data.Projectile = shared.m_attack.m_attackProjectile.name;
        }

        return data;
    }

    private static void SaveItemData(GameObject[]? items, ref List<ItemAttackData> list)
    {
        if (items is not { Length: > 0 }) return;
        foreach (GameObject item in items)
        {
            if (item == null) continue;
            if (!item.TryGetComponent(out ItemDrop component)) continue;
            ItemAttackData itemAttackData = new ItemAttackData();
            AttackData data = RecordAttackData(component);
            itemAttackData.m_attackData = data;
            var shared = component.m_itemData.m_shared;
            SaveEffectList(shared.m_hitEffect, ref itemAttackData.m_effects.m_hitEffects);
            SaveEffectList(shared.m_hitTerrainEffect, ref itemAttackData.m_effects.m_hitTerrainEffects);
            SaveEffectList(shared.m_blockEffect, ref itemAttackData.m_effects.m_blockEffects);
            SaveEffectList(shared.m_startEffect, ref itemAttackData.m_effects.m_startEffects);
            SaveEffectList(shared.m_holdStartEffect, ref itemAttackData.m_effects.m_holdStartEffects);
            SaveEffectList(shared.m_equipEffect, ref itemAttackData.m_effects.m_equipEffects);
            SaveEffectList(shared.m_unequipEffect, ref itemAttackData.m_effects.m_unEquipEffects);
            SaveEffectList(shared.m_triggerEffect, ref itemAttackData.m_effects.m_triggerEffects);
            SaveEffectList(shared.m_trailStartEffect, ref itemAttackData.m_effects.m_trailStartEffects);
            list.Add(itemAttackData);
        }
    }

    private static void WriteItemData(GameObject[]? items, string parentFolder, GameObject critter)
    {
        if (items is not { Length: > 0 }) return;
        bool isClone = CreatureManager.IsClone(critter);
        ISerializer serializer = new SerializerBuilder().Build();
        int count = 0;
        foreach (GameObject item in items)
        {
            if (item == null) continue;
            if (!item.TryGetComponent(out ItemDrop component)) continue;
            ItemDrop.ItemData.SharedData shared = component.m_itemData.m_shared;
            AttackData data = RecordAttackData(component);
            if (isClone)
            {
                var prefix = critter.name + "_";
                var originalPrefabName = item.name.Replace(prefix, string.Empty);
                var prefab = DataBase.TryGetGameObject(originalPrefabName);
                if (prefab != null) data.OriginalPrefab = originalPrefabName;
            }
            string serial = serializer.Serialize(data);
            string folderPath = parentFolder + Path.DirectorySeparatorChar + item.name;
            if (Directory.Exists(folderPath))
            {
                folderPath += "_" + count;
                ++count;
            }
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string filePath = folderPath + Path.DirectorySeparatorChar + "ItemData.yml";
            File.WriteAllText(filePath, serial);

            string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
            if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);
            
            string hitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitEffects.yml";
            string hitTerrainEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitTerrainEffects.yml";
            string blockEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "BlockEffects.yml";
            string startEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "StartEffects.yml";
            string holdStartEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HoldStartEffects.yml";
            string equipEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "EquipEffects.yml";
            string unequipEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "UnequipEffects.yml";
            string triggerEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TriggerEffects.yml";
            string trailStartEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TrailStartEffects.yml";
            
            WriteEffectList(shared.m_hitEffect, hitEffectsPath);
            WriteEffectList(shared.m_hitTerrainEffect, hitTerrainEffectsPath);
            WriteEffectList(shared.m_blockEffect, blockEffectsPath);
            WriteEffectList(shared.m_startEffect, startEffectsPath);
            WriteEffectList(shared.m_holdStartEffect, holdStartEffectsPath);
            WriteEffectList(shared.m_equipEffect, equipEffectsPath);
            WriteEffectList(shared.m_unequipEffect, unequipEffectsPath);
            WriteEffectList(shared.m_triggerEffect, triggerEffectsPath);
            WriteEffectList(shared.m_trailStartEffect, trailStartEffectsPath);
        }
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
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

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out Humanoid component)) return;
        CharacterData data = creatureData.m_characterData;
        CharacterEffects effectData = creatureData.m_effects;
        Vector3 scale = GetScale(creatureData.m_scale);
        Vector3 ragDollScale = GetScale(creatureData.m_ragdollScale);
        
        component.m_name = data.Name;
        component.m_group = data.Group;
        if (Enum.TryParse(data.Faction, out Character.Faction faction))
        {
            component.m_faction = faction;
        }
        component.m_boss = data.Boss;
        component.m_dontHideBossHud = data.DoNotHideBossHUD;
        component.m_bossEvent = data.BossEvent;
        component.m_defeatSetGlobalKey = data.DefeatSetGlobalKey;
        component.m_aiSkipTarget = data.AISkipTarget;
        component.m_crouchSpeed = data.CrouchSpeed;
        component.m_walkSpeed = data.WalkSpeed;
        component.m_speed = data.Speed;
        component.m_turnSpeed = data.TurnSpeed;
        component.m_runSpeed = data.RunSpeed;
        component.m_runTurnSpeed = data.RunTurnSpeed;
        component.m_flySlowSpeed = data.FlySlowSpeed;
        component.m_flyFastSpeed = data.FlyFastSpeed;
        component.m_flyTurnSpeed = data.FlyTurnSpeed;
        component.m_acceleration = data.Acceleration;
        component.m_jumpForce = data.JumpForce;
        component.m_jumpForceForward = data.JumpForceForward;
        component.m_jumpForceTiredFactor = data.JumpForceTiredFactor;
        component.m_airControl = data.AirControl;
        component.m_canSwim = data.CanSwim;
        component.m_swimDepth = data.SwimDepth;
        component.m_swimTurnSpeed = data.SwimTurnSpeed;
        component.m_swimAcceleration = data.SwimAcceleration;
        if (Enum.TryParse(data.GroundTilt, out Character.GroundTiltType groundTiltType))
        {
            component.m_groundTilt = groundTiltType;
        }

        component.m_swimSpeed = data.SwimSpeed;
        component.m_groundTiltSpeed = data.GroundTiltSpeed;
        component.m_flying = data.Flying;
        component.m_jumpStaminaUsage = data.JumpStaminaUsage;
        component.m_disableWhileSleeping = data.DisableWhileSleeping;
        component.m_tolerateWater = data.TolerateWater;
        component.m_tolerateFire = data.TolerateFire;
        component.m_tolerateSmoke = data.TolerateSmoke;
        component.m_tolerateTar = data.TolerateTar;
        component.m_health = data.Health;
        if (Enum.TryParse(data.BluntResistance, out HitData.DamageModifier blunt))
        {
            component.m_damageModifiers.m_blunt = blunt;
        }
        if (Enum.TryParse(data.SlashResistance, out HitData.DamageModifier slash))
        {
            component.m_damageModifiers.m_slash = slash;
        }

        if (Enum.TryParse(data.PierceResistance, out HitData.DamageModifier pierce))
        {
            component.m_damageModifiers.m_pierce = pierce;
        }

        if (Enum.TryParse(data.ChopResistance, out HitData.DamageModifier chop))
        {
            component.m_damageModifiers.m_chop = chop;
        }

        if (Enum.TryParse(data.PickaxeResistance, out HitData.DamageModifier pickaxe))
        {
            component.m_damageModifiers.m_pickaxe = pickaxe;
        }

        if (Enum.TryParse(data.FireResistance, out HitData.DamageModifier fire))
        {
            component.m_damageModifiers.m_fire = fire;
        }

        if (Enum.TryParse(data.FrostResistance, out HitData.DamageModifier frost))
        {
            component.m_damageModifiers.m_frost = frost;
        }

        if (Enum.TryParse(data.LightningResistance, out HitData.DamageModifier lightning))
        {
            component.m_damageModifiers.m_lightning = lightning;
        }

        if (Enum.TryParse(data.PoisonResistance, out HitData.DamageModifier poison))
        {
            component.m_damageModifiers.m_poison = poison;
        }

        if (Enum.TryParse(data.SpiritResistance, out HitData.DamageModifier spirit))
        {
            component.m_damageModifiers.m_spirit = spirit;
        }
        component.m_staggerWhenBlocked = data.StaggerWhenBlocked;
        component.m_staggerDamageFactor = data.StaggerDamageFactor;
        UpdateEffectList(effectData.m_hitEffects, ref component.m_hitEffects, scale);
        UpdateEffectList(effectData.m_critHitEffects, ref component.m_critHitEffects, scale);
        UpdateEffectList(effectData.m_backstabHitEffects, ref component.m_backstabHitEffects, scale);
        UpdateEffectList(effectData.m_deathEffects, ref component.m_deathEffects, ragDollScale);
        UpdateEffectList(effectData.m_waterEffects, ref component.m_waterEffects, scale);
        UpdateEffectList(effectData.m_tarEffects, ref component.m_tarEffects, scale);
        UpdateEffectList(effectData.m_slideEffects, ref component.m_slideEffects, scale);
        UpdateEffectList(effectData.m_jumpEffects, ref component.m_jumpEffects, scale);
        UpdateEffectList(effectData.m_flyingContinuousEffects, ref component.m_flyingContinuousEffect, scale);
        UpdateEffectList(effectData.m_pickupEffects, ref component.m_pickupEffects, scale);
        UpdateEffectList(effectData.m_dropEffects, ref component.m_dropEffects, scale);
        UpdateEffectList(effectData.m_consumeItemEffects, ref component.m_consumeItemEffects, scale);
        UpdateEffectList(effectData.m_equipEffects, ref component.m_equipEffects, scale);
        UpdateEffectList(effectData.m_perfectBlockEffects, ref component.m_perfectBlockEffect, scale);

        if (critter.GetComponent<Human>())
        {
            CreateItems(ref component.m_defaultItems, creatureData.m_defaultItems, scale, critter);
            CreateItems(ref component.m_randomWeapon, creatureData.m_randomWeapons, scale, critter);
            CreateItems(ref component.m_randomShield, creatureData.m_randomShields, scale, critter);
            CreateItems(ref component.m_randomArmor, creatureData.m_randomArmors, scale, critter);
            CreateRandomSets(ref component.m_randomSets, creatureData.m_randomSets, scale, critter);
            UpdateRandomItems(ref component.m_randomItems, creatureData.m_randomItems, scale);
        }
        else
        {
            UpdateItems(ref component.m_defaultItems, creatureData.m_defaultItems, scale);
            UpdateItems(ref component.m_randomWeapon, creatureData.m_randomWeapons, scale);
            UpdateItems(ref component.m_randomShield, creatureData.m_randomShields, scale);
            UpdateItems(ref component.m_randomArmor, creatureData.m_randomArmors, scale);
            UpdateRandomSets(ref component.m_randomSets, creatureData.m_randomSets, scale);
            UpdateRandomItems(ref component.m_randomItems, creatureData.m_randomItems, scale);
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

    private static void CreateItems(ref GameObject[] list, List<ItemAttackData> itemAttackDataList, Vector3 scale, GameObject critter)
    {
        if (itemAttackDataList.Count <= 0) return;
        List<GameObject> items = new();
        foreach (ItemAttackData itemAttackData in itemAttackDataList)
        {
            AttackData data = itemAttackData.m_attackData;
            ItemEffects effects = itemAttackData.m_effects;
            string prefabName = data.Name;

            GameObject? prefab = DataBase.TryGetGameObject(prefabName);
            if (prefab == null) continue;
            if (!prefab.GetComponent<ItemDrop>()) continue;

            GameObject clone = Object.Instantiate(prefab, MonsterDBPlugin.m_root.transform, false);
            clone.name = critter.name + "_" + prefabName;

            if (!clone.TryGetComponent(out ItemDrop component)) continue;
            ScaleItem(prefab, scale);
            if (Enum.TryParse(data.AnimationState, out ItemDrop.ItemData.AnimationState state))
            {
                component.m_itemData.m_shared.m_animationState = state;
            }

            component.m_itemData.m_shared.m_toolTier = data.ToolTier;
            component.m_itemData.m_shared.m_damages.m_damage = data.Damage;
            component.m_itemData.m_shared.m_damages.m_blunt = data.Blunt;
            component.m_itemData.m_shared.m_damages.m_slash = data.Slash;
            component.m_itemData.m_shared.m_damages.m_pierce = data.Pierce;
            component.m_itemData.m_shared.m_damages.m_chop = data.Chop;
            component.m_itemData.m_shared.m_damages.m_pickaxe = data.Pickaxe;
            component.m_itemData.m_shared.m_damages.m_fire = data.Fire;
            component.m_itemData.m_shared.m_damages.m_frost = data.Frost;
            component.m_itemData.m_shared.m_damages.m_lightning = data.Lightning;
            component.m_itemData.m_shared.m_damages.m_poison = data.Poison;
            component.m_itemData.m_shared.m_damages.m_spirit = data.Spirit;
            component.m_itemData.m_shared.m_attackForce = data.AttackForce;
            component.m_itemData.m_shared.m_dodgeable = data.Dodgeable;
            component.m_itemData.m_shared.m_blockable = data.Blockable;
            if (TryGetGameObject(data.SpawnOnHit, out GameObject spawnOnHit))
            {
                component.m_itemData.m_shared.m_spawnOnHit = spawnOnHit;
            }

            if (TryGetGameObject(data.SpawnOnHitTerrain, out GameObject spawnOnHitTerrain))
            {
                component.m_itemData.m_shared.m_spawnOnHitTerrain = spawnOnHitTerrain;
            }

            if (TryGetStatusEffect(data.AttackStatusEffect, out StatusEffect attackStatusEffect))
            {
                component.m_itemData.m_shared.m_attackStatusEffect = attackStatusEffect;
            }
            component.m_itemData.m_shared.m_attackStatusEffectChance = data.AttackStatusEffectChance;
            if (Enum.TryParse(data.AttackType, out Attack.AttackType attackType))
            {
                component.m_itemData.m_shared.m_attack.m_attackType = attackType;
            }
            component.m_itemData.m_shared.m_attack.m_attackAnimation = data.AttackAnimation;
            component.m_itemData.m_shared.m_attack.m_hitTerrain = data.HitTerrain;
            component.m_itemData.m_shared.m_attack.m_hitFriendly = data.HitFriendly;
            component.m_itemData.m_shared.m_attack.m_damageMultiplier = data.DamageMultiplier;
            component.m_itemData.m_shared.m_attack.m_damageMultiplierPerMissingHP = data.DamageMultiplierPerMissingHP;
            component.m_itemData.m_shared.m_attack.m_damageMultiplierByTotalHealthMissing = data.DamageMultiplierByTotalHealthMissing;
            component.m_itemData.m_shared.m_attack.m_forceMultiplier = data.ForceMultiplier;
            component.m_itemData.m_shared.m_attack.m_staggerMultiplier = data.StaggerMultiplier;
            component.m_itemData.m_shared.m_attack.m_recoilPushback = data.RecoilPushback;
            component.m_itemData.m_shared.m_attack.m_selfDamage = data.SelfDamage;
            component.m_itemData.m_shared.m_attack.m_attackOriginJoint = data.AttackOriginJoint;
            component.m_itemData.m_shared.m_attack.m_attackRange = data.AttackRange;
            component.m_itemData.m_shared.m_attack.m_attackHeight = data.AttackHeight;
            component.m_itemData.m_shared.m_attack.m_attackOffset = data.AttackOffset;
            if (TryGetGameObject(data.SpawnOnTrigger, out GameObject spawnOnTrigger))
            {
                component.m_itemData.m_shared.m_attack.m_spawnOnTrigger = spawnOnTrigger;
            }
            component.m_itemData.m_shared.m_attack.m_toggleFlying = data.ToggleFlying;
            component.m_itemData.m_shared.m_attack.m_attach = data.Attach;
            component.m_itemData.m_shared.m_attack.m_attackAngle = data.AttackAngle;
            component.m_itemData.m_shared.m_attack.m_attackRayWidth = data.AttackRayWidth;
            component.m_itemData.m_shared.m_attack.m_maxYAngle = data.MaxYAngle;
            component.m_itemData.m_shared.m_attack.m_lowerDamagePerHit = data.LowerDamagePerHit;
            component.m_itemData.m_shared.m_attack.m_hitThroughWalls = data.HitThroughWalls;
            component.m_itemData.m_shared.m_aiAttackRangeMin = data.AttackRangeMinimum;
            component.m_itemData.m_shared.m_aiAttackInterval = data.AttackInterval;
            component.m_itemData.m_shared.m_aiAttackMaxAngle = data.AttackMaxAngle;
            if (TryGetGameObject(data.Projectile, out GameObject projectile))
            {
                component.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;
            }
            
            UpdateEffectList(effects.m_hitEffects, ref component.m_itemData.m_shared.m_hitEffect, scale);
            UpdateEffectList(effects.m_hitTerrainEffects, ref component.m_itemData.m_shared.m_hitTerrainEffect, scale);
            UpdateEffectList(effects.m_blockEffects, ref component.m_itemData.m_shared.m_blockEffect, scale);
            UpdateEffectList(effects.m_startEffects, ref component.m_itemData.m_shared.m_startEffect, scale);
            UpdateEffectList(effects.m_holdStartEffects, ref component.m_itemData.m_shared.m_holdStartEffect, scale);
            UpdateEffectList(effects.m_equipEffects, ref component.m_itemData.m_shared.m_equipEffect, scale);
            UpdateEffectList(effects.m_unEquipEffects, ref component.m_itemData.m_shared.m_unequipEffect, scale);
            UpdateEffectList(effects.m_triggerEffects, ref component.m_itemData.m_shared.m_triggerEffect, scale);
            UpdateEffectList(effects.m_trailStartEffects, ref component.m_itemData.m_shared.m_trailStartEffect, scale);
            ItemDataMethods.m_clonedItems[clone.name] = clone;
            // RegisterToZNetScene(clone);
            // RegisterToObjectDB(clone);
            items.Add(prefab);
        }
        list = items.ToArray();
    }

    private static void UpdateItems(ref GameObject[] list, List<ItemAttackData> itemAttackDataList, Vector3 scale)
    {
        if (itemAttackDataList.Count <= 0) return;
        List<GameObject> items = new();
        foreach (ItemAttackData itemAttackData in itemAttackDataList)
        {
            AttackData data = itemAttackData.m_attackData;
            ItemEffects effects = itemAttackData.m_effects;
            string prefabName = data.Name;
            
            GameObject? prefab = DataBase.TryGetGameObject(prefabName);
            if (prefab == null) continue;
            if (!prefab.TryGetComponent(out ItemDrop component)) continue;
            ScaleItem(prefab, scale);
            if (Enum.TryParse(data.AnimationState, true, out ItemDrop.ItemData.AnimationState state))
            {
                component.m_itemData.m_shared.m_animationState = state;
            }

            component.m_itemData.m_shared.m_toolTier = data.ToolTier;
            component.m_itemData.m_shared.m_damages.m_damage = data.Damage;
            component.m_itemData.m_shared.m_damages.m_blunt = data.Blunt;
            component.m_itemData.m_shared.m_damages.m_slash = data.Slash;
            component.m_itemData.m_shared.m_damages.m_pierce = data.Pierce;
            component.m_itemData.m_shared.m_damages.m_chop = data.Chop;
            component.m_itemData.m_shared.m_damages.m_pickaxe = data.Pickaxe;
            component.m_itemData.m_shared.m_damages.m_fire = data.Fire;
            component.m_itemData.m_shared.m_damages.m_frost = data.Frost;
            component.m_itemData.m_shared.m_damages.m_lightning = data.Lightning;
            component.m_itemData.m_shared.m_damages.m_poison = data.Poison;
            component.m_itemData.m_shared.m_damages.m_spirit = data.Spirit;
            component.m_itemData.m_shared.m_attackForce = data.AttackForce;
            component.m_itemData.m_shared.m_dodgeable = data.Dodgeable;
            component.m_itemData.m_shared.m_blockable = data.Blockable;
            if (TryGetGameObject(data.SpawnOnHit, out GameObject spawnOnHit))
            {
                component.m_itemData.m_shared.m_spawnOnHit = spawnOnHit;
            }

            if (TryGetGameObject(data.SpawnOnHitTerrain, out GameObject spawnOnHitTerrain))
            {
                component.m_itemData.m_shared.m_spawnOnHitTerrain = spawnOnHitTerrain;
            }

            if (TryGetStatusEffect(data.AttackStatusEffect, out StatusEffect attackStatusEffect))
            {
                component.m_itemData.m_shared.m_attackStatusEffect = attackStatusEffect;
            }
            component.m_itemData.m_shared.m_attackStatusEffectChance = data.AttackStatusEffectChance;
            if (Enum.TryParse(data.AttackType, out Attack.AttackType attackType))
            {
                component.m_itemData.m_shared.m_attack.m_attackType = attackType;
            }
            component.m_itemData.m_shared.m_attack.m_attackAnimation = data.AttackAnimation;
            component.m_itemData.m_shared.m_attack.m_hitTerrain = data.HitTerrain;
            component.m_itemData.m_shared.m_attack.m_hitFriendly = data.HitFriendly;
            component.m_itemData.m_shared.m_attack.m_damageMultiplier = data.DamageMultiplier;
            component.m_itemData.m_shared.m_attack.m_damageMultiplierPerMissingHP = data.DamageMultiplierPerMissingHP;
            component.m_itemData.m_shared.m_attack.m_damageMultiplierByTotalHealthMissing = data.DamageMultiplierByTotalHealthMissing;
            component.m_itemData.m_shared.m_attack.m_forceMultiplier = data.ForceMultiplier;
            component.m_itemData.m_shared.m_attack.m_staggerMultiplier = data.StaggerMultiplier;
            component.m_itemData.m_shared.m_attack.m_recoilPushback = data.RecoilPushback;
            component.m_itemData.m_shared.m_attack.m_selfDamage = data.SelfDamage;
            component.m_itemData.m_shared.m_attack.m_attackOriginJoint = data.AttackOriginJoint;
            component.m_itemData.m_shared.m_attack.m_attackRange = data.AttackRange;
            component.m_itemData.m_shared.m_attack.m_attackHeight = data.AttackHeight;
            component.m_itemData.m_shared.m_attack.m_attackOffset = data.AttackOffset;
            if (TryGetGameObject(data.SpawnOnTrigger, out GameObject spawnOnTrigger))
            {
                component.m_itemData.m_shared.m_attack.m_spawnOnTrigger = spawnOnTrigger;
            }
            component.m_itemData.m_shared.m_attack.m_toggleFlying = data.ToggleFlying;
            component.m_itemData.m_shared.m_attack.m_attach = data.Attach;
            component.m_itemData.m_shared.m_attack.m_attackAngle = data.AttackAngle;
            component.m_itemData.m_shared.m_attack.m_attackRayWidth = data.AttackRayWidth;
            component.m_itemData.m_shared.m_attack.m_maxYAngle = data.MaxYAngle;
            component.m_itemData.m_shared.m_attack.m_lowerDamagePerHit = data.LowerDamagePerHit;
            component.m_itemData.m_shared.m_attack.m_hitThroughWalls = data.HitThroughWalls;
            component.m_itemData.m_shared.m_aiAttackRangeMin = data.AttackRangeMinimum;
            component.m_itemData.m_shared.m_aiAttackInterval = data.AttackInterval;
            component.m_itemData.m_shared.m_aiAttackMaxAngle = data.AttackMaxAngle;
            if (TryGetGameObject(data.Projectile, out GameObject projectile))
            {
                component.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;
            }
            
            UpdateEffectList(effects.m_hitEffects, ref component.m_itemData.m_shared.m_hitEffect, scale);
            UpdateEffectList(effects.m_hitTerrainEffects, ref component.m_itemData.m_shared.m_hitTerrainEffect, scale);
            UpdateEffectList(effects.m_blockEffects, ref component.m_itemData.m_shared.m_blockEffect, scale);
            UpdateEffectList(effects.m_startEffects, ref component.m_itemData.m_shared.m_startEffect, scale);
            UpdateEffectList(effects.m_holdStartEffects, ref component.m_itemData.m_shared.m_holdStartEffect, scale);
            UpdateEffectList(effects.m_equipEffects, ref component.m_itemData.m_shared.m_equipEffect, scale);
            UpdateEffectList(effects.m_unEquipEffects, ref component.m_itemData.m_shared.m_unequipEffect, scale);
            UpdateEffectList(effects.m_triggerEffects, ref component.m_itemData.m_shared.m_triggerEffect, scale);
            UpdateEffectList(effects.m_trailStartEffects, ref component.m_itemData.m_shared.m_trailStartEffect, scale);
            
            items.Add(prefab);
        }

        list = items.ToArray();
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

    private static void CreateRandomSets(ref Humanoid.ItemSet[] list, List<RandomItemSetsData> data, Vector3 scale, GameObject critter)
    {
        if (data.Count <= 0) return;
        List<Humanoid.ItemSet> sets = new();
        foreach (RandomItemSetsData setData in data)
        {
            string setName = setData.m_name;
            Humanoid.ItemSet set = new()
            {
                m_name = setName
            };
            CreateItems(ref set.m_items, setData.m_items, scale, critter);
            sets.Add(set);
        }

        list = sets.ToArray();
    }

    private static void UpdateRandomSets(ref Humanoid.ItemSet[] list, List<RandomItemSetsData> data, Vector3 scale)
    {
        if (data.Count <= 0) return;
        List<Humanoid.ItemSet> sets = new();
        foreach (RandomItemSetsData setData in data)
        {
            string setName = setData.m_name;
            Humanoid.ItemSet set = new()
            {
                m_name = setName
            };
            UpdateItems(ref set.m_items, setData.m_items, scale);
            sets.Add(set);
        }

        list = sets.ToArray();
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

    private static void UpdateRandomItems(ref Humanoid.RandomItem[] list, List<RandomItemData> randomItemDataList, Vector3 scale)
    {
        if (randomItemDataList.Count <= 0) return;
        List<Humanoid.RandomItem> items = new();
        foreach (RandomItemData randomItemData in randomItemDataList)
        {
            GameObject? prefab = DataBase.TryGetGameObject(randomItemData.PrefabName);
            if (prefab == null) continue;
            ScaleItem(prefab, scale);
            items.Add(new Humanoid.RandomItem()
            {
                m_prefab = prefab,
                m_chance = randomItemData.Chance
            });
        }
        
        list = items.ToArray();

    }

    private static void ScaleItem(GameObject prefab, Vector3 scale)
    {
        Transform attach = prefab.transform.Find("attach");
        if (!attach) return;
        attach.transform.localScale = scale;
    }

    private static void SetProjectile(string folderPath, ref Attack attack)
    {
        if (!Directory.Exists(folderPath)) return;
        string[] projectiles = Directory.GetFiles(folderPath);
        if (projectiles.Length != 1) return;
        var deserializer = new DeserializerBuilder().Build();

        string projectilePath = projectiles[0];
        var data = deserializer.Deserialize<ProjectileData>(File.ReadAllText(projectilePath));
        var prefab = ZNetScene.instance.GetPrefab(data.Name);
        if (!prefab) return;
        if (!prefab.TryGetComponent(out Projectile component)) return;
        component.m_damage.m_damage = data.Damage;
        component.m_damage.m_blunt = data.Blunt;
        component.m_damage.m_slash = data.Slash;
        component.m_damage.m_pierce = data.Pierce;
        component.m_damage.m_chop = data.Chop;
        component.m_damage.m_pickaxe = data.Pickaxe;
        component.m_damage.m_fire = data.Fire;
        component.m_damage.m_frost = data.Frost;
        component.m_damage.m_lightning = data.Lightning;
        component.m_damage.m_poison = data.Poison;
        component.m_damage.m_spirit = data.Spirit;
        component.m_aoe = data.AOE;
        component.m_dodgeable = data.Dodgeable;
        component.m_blockable = data.Blockable;
        component.m_attackForce = data.AttackForce;
        component.m_backstabBonus = data.BackstabBonus;
        component.m_statusEffect = data.StatusEffect;
        component.m_healthReturn = data.HealthReturn;
        component.m_canHitWater = data.CanHitWater;
        component.m_ttl = data.Duration;
        component.m_gravity = data.Gravity;
        component.m_drag = data.Drag;
        component.m_rayRadius = data.RayRadius;
        component.m_stayAfterHitStatic = data.StayAfterHitStatic;
        component.m_stayAfterHitDynamic = data.StayAfterHitDynamic;
        component.m_stayTTL = data.StayDuration;
        component.m_bounce = data.Bounce;
        component.m_bounceOnWater = data.BounceOnWater;
        component.m_bouncePower = data.BouncePower;
        component.m_bounceRoughness = data.BounceRoughness;
        component.m_maxBounces = data.MaxBounces;
        component.m_minBounceVel = data.MinBounceVelocity;
        component.m_respawnItemOnHit = data.RespawnItemOnHit;
        component.m_spawnOnTtl = data.SpawnOnDuration;
        if (TryGetGameObject(data.SpawnOnHit, out GameObject spawnOnHit))
        {
            component.m_spawnOnHit = spawnOnHit;
        }
        component.m_spawnOnHitChance = data.SpawnOnHitChance;
        component.m_spawnCount = data.SpawnCount;
        component.m_randomSpawnOnHit.Clear();
        foreach (var spawn in data.RandomSpawnOnHit)
        {
            if (TryGetGameObject(spawn, out GameObject randomSpawn))
            {
                component.m_randomSpawnOnHit.Add(randomSpawn);
            }
        }
        component.m_randomSpawnOnHitCount = data.RandomSpawnOnHitCount;
        component.m_randomSpawnSkipLava = data.RandomSpawnSkipLava;
        component.m_staticHitOnly = data.StaticHitOnly;
        component.m_groundHitOnly = data.GroundHitOnly;
        component.m_spawnRandomRotation = data.SpawnRandomRotation;
        
        attack.m_attackProjectile = prefab;
    }

    private static bool TryGetGameObject(string prefabName, out GameObject prefab)
    {
        prefab = null!;
        if (prefabName.IsNullOrWhiteSpace()) return false;
        GameObject? gameObject = DataBase.TryGetGameObject(prefabName);
        if (gameObject == null) return false;
        prefab = gameObject;
        return true;
    }

    private static bool TryGetStatusEffect(string effectName, out StatusEffect statusEffect)
    {
        statusEffect = null!;
        if (effectName.IsNullOrWhiteSpace()) return false;
        StatusEffect effect = ObjectDB.instance.GetStatusEffect(effectName.GetStableHashCode());
        return effect != null;
    }
    
    public static void CloneRagDoll(GameObject critter, Dictionary<string, Material> ragDollMats)
    {
        if (!critter.TryGetComponent(out Humanoid component)) return;
        CloneEffectList(critter, ref component.m_deathEffects, ragDollMats);
    }

    public static void CloneItems(GameObject critter)
    {
        if (!critter.TryGetComponent(out Humanoid component)) return;
        if (critter.GetComponent<Human>()) return;
        Dictionary<string, GameObject> clonedItems = new();
        CloneAttacks(ref component.m_defaultItems, critter, ref clonedItems);
        CloneAttacks(ref component.m_randomWeapon, critter, ref clonedItems);
        CloneAttacks(ref component.m_randomArmor, critter, ref clonedItems);
        CloneAttacks(ref component.m_randomShield, critter, ref clonedItems);
        CloneRandomSets(ref component.m_randomSets, critter, ref clonedItems);
        CloneRandomItems(ref component.m_randomItems, critter, ref clonedItems);
    }

    private static void CloneRandomSets(ref Humanoid.ItemSet[] sets, GameObject critter, ref Dictionary<string, GameObject> clonedItems)
    {
        foreach (Humanoid.ItemSet set in sets)
        {
            CloneAttacks(ref set.m_items, critter, ref clonedItems);
        }
    }

    private static void CloneRandomItems(ref Humanoid.RandomItem[] items, GameObject critter, ref Dictionary<string, GameObject> clonedItems)
    {
        if (items.Length <= 0) return;
        List<Humanoid.RandomItem> newItems = new();
        foreach (var item in items)
        {
            newItems.Add(new Humanoid.RandomItem()
            {
                m_chance = item.m_chance,
                m_prefab = CloneAttack(item.m_prefab, critter, ref clonedItems)
            });
        }

        items = newItems.ToArray();
    }

    private static void CloneAttacks(ref GameObject[] items, GameObject critter, ref Dictionary<string, GameObject> clonedItems)
    {
        if (items.Length <= 0) return;
        List<GameObject> newItems = new();
        foreach (var item in items)
        {
            newItems.Add(CloneAttack(item, critter, ref clonedItems));
        }

        items = newItems.ToArray();
    }

    private static GameObject CloneAttack(GameObject item, GameObject critter,
        ref Dictionary<string, GameObject> clonedItems)
    {
        var name = critter.name + "_" + item.name;
        if (clonedItems.TryGetValue(name, out GameObject alreadyCloned)) return alreadyCloned;
        var clone = Object.Instantiate(item, MonsterDBPlugin.m_root.transform, false);
        clone.name = name;
        ItemDataMethods.m_clonedItems[name] = clone;
        // RegisterToObjectDB(clone);
        // RegisterToZNetScene(clone);
        return clone;
    }
}