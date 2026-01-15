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
    
    public static void Update(GameObject critter, CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out Humanoid component)) return;
        
        CharacterData data = creatureData.m_characterData;
        CharacterEffects effectData = creatureData.m_effects;
        Vector3 scale = creatureData.m_scale.ToRef();
        Vector3 ragDollScale = creatureData.m_ragdollScale.ToRef();
        component.m_name = data.Name;
        component.m_group = data.Group;
        if (Enum.TryParse(data.Faction, true, out Character.Faction faction))
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
        if (Enum.TryParse(data.GroundTilt, true, out Character.GroundTiltType groundTiltType))
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
        if (Enum.TryParse(data.BluntResistance, true, out HitData.DamageModifier blunt))
        {
            component.m_damageModifiers.m_blunt = blunt;
        }
        if (Enum.TryParse(data.SlashResistance, true, out HitData.DamageModifier slash))
        {
            component.m_damageModifiers.m_slash = slash;
        }

        if (Enum.TryParse(data.PierceResistance, true, out HitData.DamageModifier pierce))
        {
            component.m_damageModifiers.m_pierce = pierce;
        }

        if (Enum.TryParse(data.ChopResistance, true, out HitData.DamageModifier chop))
        {
            component.m_damageModifiers.m_chop = chop;
        }

        if (Enum.TryParse(data.PickaxeResistance, true, out HitData.DamageModifier pickaxe))
        {
            component.m_damageModifiers.m_pickaxe = pickaxe;
        }

        if (Enum.TryParse(data.FireResistance, true, out HitData.DamageModifier fire))
        {
            component.m_damageModifiers.m_fire = fire;
        }

        if (Enum.TryParse(data.FrostResistance, true, out HitData.DamageModifier frost))
        {
            component.m_damageModifiers.m_frost = frost;
        }

        if (Enum.TryParse(data.LightningResistance, true, out HitData.DamageModifier lightning))
        {
            component.m_damageModifiers.m_lightning = lightning;
        }

        if (Enum.TryParse(data.PoisonResistance, true, out HitData.DamageModifier poison))
        {
            component.m_damageModifiers.m_poison = poison;
        }

        if (Enum.TryParse(data.SpiritResistance, true, out HitData.DamageModifier spirit))
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

        component.m_defaultItems = new List<GameObject>().ToArray();
        component.m_randomWeapon = new List<GameObject>().ToArray();
        component.m_randomShield = new List<GameObject>().ToArray();
        component.m_randomArmor = new List<GameObject>().ToArray();
        component.m_randomSets = new List<Humanoid.ItemSet>().ToArray();
        component.m_randomItems = new List<Humanoid.RandomItem>().ToArray();

        UpdateItems(ref component.m_defaultItems, creatureData.m_defaultItems, scale, creatureData);
        UpdateItems(ref component.m_randomWeapon, creatureData.m_randomWeapons, scale, creatureData);
        UpdateItems(ref component.m_randomShield, creatureData.m_randomShields, scale, creatureData);
        UpdateItems(ref component.m_randomArmor, creatureData.m_randomArmors, scale, creatureData);
        UpdateRandomSets(ref component.m_randomSets, creatureData.m_randomSets, scale, creatureData);
        UpdateRandomItems(ref component.m_randomItems, creatureData.m_randomItems, scale);
    }
    
    private static void UpdateItems(ref GameObject[] list, List<ItemAttackData> itemAttackDataList, Vector3 scale, CreatureData creatureData)
    {
        if (itemAttackDataList.Count <= 0) return;
        List<GameObject> items = new();
        foreach (ItemAttackData itemAttackData in itemAttackDataList)
        {
            AttackData data = itemAttackData.m_attackData;
            ItemEffects effects = itemAttackData.m_effects;
            string prefabName = data.Name;

            if (PrefabManager.GetPrefab(prefabName) is not { } prefab || !prefab.TryGetComponent(out ItemDrop component)) continue;
            ScaleItem(prefab, scale);
            UpdateMaterial(itemAttackData.m_attackData.MaterialOverride, creatureData.m_materials, component.gameObject);
            
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
            
            component.m_itemData.m_shared.m_spawnOnHit = null;
            component.m_itemData.m_shared.m_spawnOnHitTerrain = null;
            component.m_itemData.m_shared.m_attackStatusEffect = null;

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

            component.m_itemData.m_shared.m_attack.m_spawnOnTrigger = null;
            
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

            component.m_itemData.m_shared.m_attack.m_attackProjectile = null;
            
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
    
    private static void UpdateMaterial(string materialName,Dictionary<string, VisualMethods.MaterialData> materialDataList, GameObject prefab)
    {
        if (materialName.IsNullOrWhiteSpace()) return;
        if (!materialDataList.TryGetValue(materialName, out VisualMethods.MaterialData materialData)) return;
        if (prefab.GetComponentInChildren<Renderer>() is not { } renderer) return;
        List<Material> newMats = new List<Material>();
        foreach (var material in renderer.sharedMaterials)
        {
            var mat = new Material(material);
            mat.mainTexture = TextureManager.GetTexture(materialData._MainTex, mat.mainTexture);
            if (mat.HasProperty(ShaderRef._EmissionColor)) material.SetColor(ShaderRef._EmissionColor, VisualMethods.GetColor(materialData._EmissionColor));
            if (mat.HasProperty(ShaderRef._Hue)) material.SetFloat(ShaderRef._Hue, materialData._Hue);
            if (mat.HasProperty(ShaderRef._Value)) material.SetFloat(ShaderRef._Value, materialData._Value);
            if (mat.HasProperty(ShaderRef._Saturation)) material.SetFloat(ShaderRef._Saturation, materialData._Saturation);
            newMats.Add(mat);
        }

        renderer.sharedMaterials = newMats.ToArray();
        renderer.materials = newMats.ToArray();
    }
    
    private static bool TryGetGameObject(string prefabName, out GameObject prefab)
    {
        prefab = null!;
        if (prefabName.IsNullOrWhiteSpace()) return false;
        GameObject? gameObject = PrefabManager.GetPrefab(prefabName);
        if (gameObject == null) return false;
        prefab = gameObject;
        return true;
    }
    
    private static bool TryGetStatusEffect(string effectName, out StatusEffect statusEffect)
    {
        statusEffect = null!;
        if (effectName.IsNullOrWhiteSpace()) return false;
        statusEffect = PrefabManager.GetStatusEffect(effectName);
        return statusEffect != null;
    }
    
    public static void UpdateRandomSets(ref Humanoid.ItemSet[] list, List<RandomItemSetsData> data, Vector3 scale, CreatureData creatureData)
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
            UpdateItems(ref set.m_items, setData.m_items, scale, creatureData);
            sets.Add(set);
        }

        list = sets.ToArray();
    }
    
    public static void UpdateRandomItems(ref Humanoid.RandomItem[] list, List<RandomItemData> randomItemDataList, Vector3 scale)
    {
        if (randomItemDataList.Count <= 0) return;
        List<Humanoid.RandomItem> items = new();
        foreach (RandomItemData randomItemData in randomItemDataList)
        {
            GameObject? prefab = PrefabManager.GetPrefab(randomItemData.PrefabName);
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
}