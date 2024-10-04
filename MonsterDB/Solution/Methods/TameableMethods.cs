using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;
using Object = UnityEngine.Object;

namespace MonsterDB.Solution.Methods;

public static class TameableMethods
{
    private static readonly List<string> m_newTames = new();
    private static readonly List<string> m_newHumanoids = new();
    private static readonly List<string> m_newMonsterAI = new();

    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out Tameable component)) return;
        creatureData.m_tameable = RecordTameData(component);
        SaveEffectList(component.m_tamedEffect, ref creatureData.m_effects.m_tamedEffects);
        SaveEffectList(component.m_sootheEffect, ref creatureData.m_effects.m_soothEffects);
        SaveEffectList(component.m_petEffect, ref creatureData.m_effects.m_petEffects);
        SaveEffectList(component.m_petEffect, ref creatureData.m_effects.m_unSummonEffects);
    }
    
    public static void Write(GameObject critter, string folderPath)
    {
        if (!critter.TryGetComponent(out Tameable component)) return;
        string filePath = folderPath + Path.DirectorySeparatorChar + "Tameable.yml";
        TameableData data = RecordTameData(component);

        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);
        
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);

        string tamedEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TamedEffects.yml";
        string soothEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "SoothEffects.yml";
        string petEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "PetEffects.yml";
        string unSummonEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "UnsummonEffects.yml";
        
        WriteEffectList(component.m_tamedEffect, tamedEffectsPath);
        WriteEffectList(component.m_sootheEffect, soothEffectsPath);
        WriteEffectList(component.m_petEffect, petEffectsPath);
        WriteEffectList(component.m_unSummonEffect, unSummonEffectsPath);
    }

    private static TameableData RecordTameData(Tameable component)
    {
        return new TameableData
        {
            FedDuration = component.m_fedDuration,
            TamingTime = component.m_tamingTime,
            StartTamed = component.m_startsTamed,
            Commandable = component.m_commandable,
            UnsummonDistance = component.m_unsummonDistance,
            UnsummonOnOwnerLogoutSeconds = component.m_unsummonOnOwnerLogoutSeconds,
            LevelUpOwnerSkill = component.m_levelUpOwnerSkill.ToString(),
            LevelUpFactor = component.m_levelUpFactor,
            DropSaddleOnDeath = component.m_dropSaddleOnDeath,
            RandomStartingName = component.m_randomStartingName
        };
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "Tameable.yml";
        if (!File.Exists(filePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string serial = File.ReadAllText(filePath);
        if (serial.IsNullOrWhiteSpace()) return;
        try
        {
            TameableData data = deserializer.Deserialize<TameableData>(serial);
            creatureData.m_tameable = data;
        }
        catch
        {
            LogParseFailure(filePath);
        }
        
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) return;
        
        string tamedEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TamedEffects.yml";
        string soothEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "SoothEffects.yml";
        string petEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "PetEffects.yml";
        string unSummonEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "UnsummonEffects.yml";
        
        ReadEffectInfo(tamedEffectsPath, ref creatureData.m_effects.m_tamedEffects);
        ReadEffectInfo(soothEffectsPath, ref creatureData.m_effects.m_soothEffects);
        ReadEffectInfo(petEffectsPath, ref creatureData.m_effects.m_petEffects);
        ReadEffectInfo(unSummonEffectsPath, ref creatureData.m_effects.m_unSummonEffects);
    }

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        TameableData data = creatureData.m_tameable;
        if (data.TamingTime == 0f)
        {
            RemoveTameComponent(critter);
            return;
        }
        Vector3 scale = GetScale(creatureData.m_scale);
        if (!critter.TryGetComponent(out Tameable component))
        {
            if (critter.TryGetComponent(out AnimalAI animalAI))
            {
                ConvertMonsterAI(critter, animalAI);
            }

            if (critter.TryGetComponent(out Character character))
            {
                ConvertToHumanoid(critter, character);
            }
            component = critter.AddComponent<Tameable>();
            if (!m_newTames.Contains(critter.name)) m_newTames.Add(critter.name);
        }
        component.m_fedDuration = data.FedDuration;
        component.m_tamingTime = data.TamingTime;
        component.m_startsTamed = data.StartTamed;
        component.m_commandable = data.Commandable;
        component.m_unsummonDistance = data.UnsummonDistance;
        component.m_unsummonOnOwnerLogoutSeconds = data.UnsummonOnOwnerLogoutSeconds;
        if (Enum.TryParse(data.LevelUpOwnerSkill, out Skills.SkillType skillType))
        {
            component.m_levelUpOwnerSkill = skillType;
        }
        component.m_levelUpFactor = data.LevelUpFactor;
        component.m_dropSaddleOnDeath = data.DropSaddleOnDeath;
        component.m_randomStartingName = data.RandomStartingName;
        
        UpdateEffectList(creatureData.m_effects.m_tamedEffects, ref component.m_tamedEffect, scale);
        UpdateEffectList(creatureData.m_effects.m_soothEffects, ref component.m_sootheEffect, scale);
        UpdateEffectList(creatureData.m_effects.m_petEffects, ref component.m_petEffect, scale);
        UpdateEffectList(creatureData.m_effects.m_unSummonEffects, ref component.m_unSummonEffect, scale);
    }

    private static void RemoveTameComponent(GameObject critter)
    {
        if (m_newTames.Contains(critter.name) || HumanMan.m_newHumans.ContainsKey(critter.name))
        {
            if (critter.TryGetComponent(out Tameable tameable))
            {
                Object.Destroy(tameable);
                m_newTames.Remove(critter.name);
            }
        }

        if (m_newHumanoids.Contains(critter.name))
        {
            if (critter.TryGetComponent(out Humanoid humanoid))
            {
                ConvertToCharacter(critter, humanoid);
            }
        }

        if (m_newMonsterAI.Contains(critter.name))
        {
            if (critter.TryGetComponent(out MonsterAI monsterAI))
            {
                ConvertToAnimalAI(critter, monsterAI);
            }
        }
    }

    private static void ConvertToCharacter(GameObject critter, Humanoid original)
    {
        Character component = critter.AddComponent<Character>();
        component.m_eye = original.m_eye;
        component.m_name = original.m_name;
        component.m_group = original.m_group;
        component.m_faction = original.m_faction;
        component.m_boss = original.m_boss;
        component.m_dontHideBossHud = original.m_dontHideBossHud;
        component.m_bossEvent = original.m_bossEvent;
        component.m_defeatSetGlobalKey = original.m_defeatSetGlobalKey;
        component.m_aiSkipTarget = original.m_aiSkipTarget;
        component.m_crouchSpeed = original.m_crouchSpeed;
        component.m_walkSpeed = original.m_walkSpeed;
        component.m_speed = original.m_speed;
        component.m_turnSpeed = original.m_turnSpeed;
        component.m_runSpeed = original.m_runSpeed;
        component.m_runTurnSpeed = original.m_runTurnSpeed;
        component.m_flySlowSpeed = original.m_flySlowSpeed;
        component.m_flyFastSpeed = original.m_flyFastSpeed;
        component.m_flyTurnSpeed = original.m_flyTurnSpeed;
        component.m_acceleration = original.m_acceleration;
        component.m_jumpForce = original.m_jumpForce;
        component.m_jumpForceForward = original.m_jumpForceForward;
        component.m_jumpForceTiredFactor = original.m_jumpForceTiredFactor;
        component.m_airControl = original.m_airControl;
        component.m_canSwim = original.m_canSwim;
        component.m_swimDepth = original.m_swimDepth;
        component.m_swimTurnSpeed = original.m_swimTurnSpeed;
        component.m_swimAcceleration = original.m_swimAcceleration;
        component.m_groundTilt = original.m_groundTilt;
        component.m_groundTiltSpeed = original.m_groundTiltSpeed;
        component.m_flying = original.m_flying;
        component.m_jumpStaminaUsage = original.m_jumpStaminaUsage;
        component.m_disableWhileSleeping = original.m_disableWhileSleeping;
        component.m_hitEffects = original.m_hitEffects;
        component.m_critHitEffects = original.m_critHitEffects;
        component.m_backstabHitEffects = original.m_backstabHitEffects;
        component.m_deathEffects = original.m_deathEffects;
        component.m_waterEffects = original.m_waterEffects;
        component.m_tarEffects = original.m_tarEffects;
        component.m_slideEffects = original.m_slideEffects;
        component.m_jumpEffects = original.m_jumpEffects;
        component.m_flyingContinuousEffect = original.m_flyingContinuousEffect;
        component.m_tolerateWater = original.m_tolerateWater;
        component.m_tolerateFire = original.m_tolerateFire;
        component.m_tolerateSmoke = original.m_tolerateSmoke;
        component.m_tolerateTar = original.m_tolerateTar;
        component.m_health = original.m_health;
        component.m_damageModifiers = original.m_damageModifiers;
        component.m_weakSpots = original.m_weakSpots;
        component.m_staggerWhenBlocked = original.m_staggerWhenBlocked;
        component.m_staggerDamageFactor = original.m_staggerDamageFactor;
        Object.Destroy(original);
        m_newHumanoids.Remove(critter.name);
    }
    private static void ConvertToHumanoid(GameObject critter, Character original)
    {
        Humanoid component = critter.AddComponent<Humanoid>();
        component.m_eye = original.m_eye;
        component.m_name = original.m_name;
        component.m_group = original.m_group;
        component.m_faction = original.m_faction;
        component.m_boss = original.m_boss;
        component.m_dontHideBossHud = original.m_dontHideBossHud;
        component.m_bossEvent = original.m_bossEvent;
        component.m_defeatSetGlobalKey = original.m_defeatSetGlobalKey;
        component.m_aiSkipTarget = original.m_aiSkipTarget;
        component.m_crouchSpeed = original.m_crouchSpeed;
        component.m_walkSpeed = original.m_walkSpeed;
        component.m_speed = original.m_speed;
        component.m_turnSpeed = original.m_turnSpeed;
        component.m_runSpeed = original.m_runSpeed;
        component.m_runTurnSpeed = original.m_runTurnSpeed;
        component.m_flySlowSpeed = original.m_flySlowSpeed;
        component.m_flyFastSpeed = original.m_flyFastSpeed;
        component.m_flyTurnSpeed = original.m_flyTurnSpeed;
        component.m_acceleration = original.m_acceleration;
        component.m_jumpForce = original.m_jumpForce;
        component.m_jumpForceForward = original.m_jumpForceForward;
        component.m_jumpForceTiredFactor = original.m_jumpForceTiredFactor;
        component.m_airControl = original.m_airControl;
        component.m_canSwim = original.m_canSwim;
        component.m_swimDepth = original.m_swimDepth;
        component.m_swimTurnSpeed = original.m_swimTurnSpeed;
        component.m_swimAcceleration = original.m_swimAcceleration;
        component.m_groundTilt = original.m_groundTilt;
        component.m_groundTiltSpeed = original.m_groundTiltSpeed;
        component.m_flying = original.m_flying;
        component.m_jumpStaminaUsage = original.m_jumpStaminaUsage;
        component.m_disableWhileSleeping = original.m_disableWhileSleeping;
        component.m_hitEffects = original.m_hitEffects;
        component.m_critHitEffects = original.m_critHitEffects;
        component.m_backstabHitEffects = original.m_backstabHitEffects;
        component.m_deathEffects = original.m_deathEffects;
        component.m_waterEffects = original.m_waterEffects;
        component.m_tarEffects = original.m_tarEffects;
        component.m_slideEffects = original.m_slideEffects;
        component.m_jumpEffects = original.m_jumpEffects;
        component.m_flyingContinuousEffect = original.m_flyingContinuousEffect;
        component.m_tolerateWater = original.m_tolerateWater;
        component.m_tolerateFire = original.m_tolerateFire;
        component.m_tolerateSmoke = original.m_tolerateSmoke;
        component.m_tolerateTar = original.m_tolerateTar;
        component.m_health = original.m_health;
        component.m_damageModifiers = original.m_damageModifiers;
        component.m_weakSpots = original.m_weakSpots;
        component.m_staggerWhenBlocked = original.m_staggerWhenBlocked;
        component.m_staggerDamageFactor = original.m_staggerDamageFactor;
        // component.m_minLavaMaskThreshold = original.m_minLavaMaskThreshold;
        // component.m_maxLavaMaskThreshold = original.m_maxLavaMaskThreshold;
        // component.m_heatBuildupBase = original.m_heatBuildupBase;
        // component.m_heatBuildupWater = original.m_heatBuildupWater;
        // component.m_heatCooldownBase = original.m_heatCooldownBase;
        // component.m_heatWaterTouchMultiplier = original.m_heatWaterTouchMultiplier;
        // component.m_lavaDamageTickInterval = original.m_lavaDamageTickInterval;
        // component.m_heatLevelFirstDamageThreshold = original.m_heatLevelFirstDamageThreshold;
        // component.m_lavaFirstDamage = original.m_lavaFirstDamage;
        // component.m_lavaFullDamage = original.m_lavaFullDamage;
        // component.m_lavaAirDamageHeight = original.m_lavaAirDamageHeight;
        // component.m_dayHeatGainRunning = original.m_dayHeatGainRunning;
        // component.m_dayHeatGainStill = original.m_dayHeatGainStill;
        // component.m_dayHeatEquipmentStop = original.m_dayHeatEquipmentStop;
        // component.m_lavaSlowMax = original.m_lavaSlowMax;
        // component.m_lavaSlowHeight = original.m_lavaSlowHeight;
        // component.m_lavaHeatEffects = original.m_lavaHeatEffects;
        component.m_defaultItems = new List<GameObject>().ToArray();
        component.m_randomWeapon = new List<GameObject>().ToArray();
        component.m_randomArmor =  new List<GameObject>().ToArray();
        component.m_randomShield = new List<GameObject>().ToArray();
        component.m_randomSets = new List<Humanoid.ItemSet>().ToArray();
        component.m_randomItems = new List<Humanoid.RandomItem>().ToArray();
        Object.Destroy(original);
        m_newHumanoids.Add(critter.name);
    }
    private static void ConvertToAnimalAI(GameObject critter, MonsterAI original)
    {
        AnimalAI component = critter.AddComponent<AnimalAI>();
        component.m_viewRange = original.m_viewRange;
        component.m_viewAngle = original.m_viewAngle;
        component.m_hearRange = original.m_hearRange;
        component.m_mistVision = original.m_mistVision;
        component.m_alertedEffects = original.m_alertedEffects;
        component.m_idleSound = original.m_idleSound;
        component.m_idleSoundInterval = original.m_idleSoundInterval;
        component.m_idleSoundChance = original.m_idleSoundChance;
        component.m_pathAgentType = original.m_pathAgentType;
        component.m_moveMinAngle = original.m_moveMinAngle;
        component.m_smoothMovement = original.m_smoothMovement;
        component.m_serpentMovement = original.m_serpentMovement;
        component.m_serpentTurnRadius = original.m_serpentTurnRadius;
        component.m_jumpInterval = original.m_jumpInterval;
        component.m_randomCircleInterval = original.m_randomCircleInterval;
        component.m_randomMoveInterval = original.m_randomMoveInterval;
        component.m_randomMoveRange = original.m_randomMoveRange;
        component.m_randomFly = original.m_randomFly;
        component.m_chanceToTakeoff = original.m_chanceToTakeoff;
        component.m_chanceToLand = original.m_chanceToLand;
        component.m_groundDuration = original.m_groundDuration;
        component.m_airDuration = original.m_airDuration;
        component.m_maxLandAltitude = original.m_maxLandAltitude;
        component.m_takeoffTime = original.m_takeoffTime;
        component.m_flyAltitudeMin = original.m_flyAltitudeMin;
        component.m_flyAltitudeMax = original.m_flyAltitudeMax;
        component.m_flyAbsMinAltitude = original.m_flyAbsMinAltitude;
        component.m_avoidFire = original.m_avoidFire;
        component.m_afraidOfFire = original.m_afraidOfFire;
        component.m_avoidWater = original.m_avoidWater;
        component.m_avoidLava = original.m_avoidLava;
        component.m_skipLavaTargets = original.m_skipLavaTargets;
        component.m_avoidLavaFlee = original.m_avoidLavaFlee;
        component.m_aggravatable = original.m_aggravatable;
        component.m_passiveAggresive = original.m_passiveAggresive;
        component.m_spawnMessage = original.m_spawnMessage;
        component.m_deathMessage = original.m_deathMessage;
        component.m_alertedMessage = original.m_alertedMessage;
        component.m_fleeRange = original.m_fleeRange;
        component.m_fleeAngle = original.m_fleeAngle;
        component.m_fleeInterval = original.m_fleeInterval;
        Object.Destroy(original);
        m_newMonsterAI.Remove(critter.name);
    }
    private static void ConvertMonsterAI(GameObject critter, AnimalAI original)
    {
        MonsterAI component = critter.AddComponent<MonsterAI>();
        component.m_viewRange = original.m_viewRange;
        component.m_viewAngle = original.m_viewAngle;
        component.m_hearRange = original.m_hearRange;
        component.m_mistVision = original.m_mistVision;
        component.m_alertedEffects = original.m_alertedEffects;
        component.m_idleSound = original.m_idleSound;
        component.m_idleSoundInterval = original.m_idleSoundInterval;
        component.m_idleSoundChance = original.m_idleSoundChance;
        component.m_pathAgentType = original.m_pathAgentType;
        component.m_moveMinAngle = original.m_moveMinAngle;
        component.m_smoothMovement = original.m_smoothMovement;
        component.m_serpentMovement = original.m_serpentMovement;
        component.m_serpentTurnRadius = original.m_serpentTurnRadius;
        component.m_jumpInterval = original.m_jumpInterval;
        component.m_randomCircleInterval = original.m_randomCircleInterval;
        component.m_randomMoveInterval = original.m_randomMoveInterval;
        component.m_randomMoveRange = original.m_randomMoveRange;
        component.m_randomFly = original.m_randomFly;
        component.m_chanceToTakeoff = original.m_chanceToTakeoff;
        component.m_chanceToLand = original.m_chanceToLand;
        component.m_groundDuration = original.m_groundDuration;
        component.m_airDuration = original.m_airDuration;
        component.m_maxLandAltitude = original.m_maxLandAltitude;
        component.m_takeoffTime = original.m_takeoffTime;
        component.m_flyAltitudeMin = original.m_flyAltitudeMin;
        component.m_flyAltitudeMax = original.m_flyAltitudeMax;
        component.m_flyAbsMinAltitude = original.m_flyAbsMinAltitude;
        component.m_avoidFire = original.m_avoidFire;
        component.m_afraidOfFire = original.m_afraidOfFire;
        component.m_avoidWater = original.m_avoidWater;
        component.m_avoidLava = original.m_avoidLava;
        component.m_skipLavaTargets = original.m_skipLavaTargets;
        component.m_avoidLavaFlee = original.m_avoidLavaFlee;
        component.m_aggravatable = original.m_aggravatable;
        component.m_passiveAggresive = original.m_passiveAggresive;
        component.m_spawnMessage = original.m_spawnMessage;
        component.m_deathMessage = original.m_deathMessage;
        component.m_alertedMessage = original.m_alertedMessage;
        component.m_fleeRange = original.m_fleeRange;
        component.m_fleeAngle = original.m_fleeAngle;
        component.m_fleeInterval = original.m_fleeInterval;
        component.m_consumeItems = new List<ItemDrop>();
        Object.Destroy(original);
        m_newMonsterAI.Add(critter.name);
    }
}