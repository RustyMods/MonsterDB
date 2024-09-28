using System;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class MonsterAIMethods
{
    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out MonsterAI component)) return;
        creatureData.m_monsterAIData = RecordAIData(component);
        SaveEffectList(component.m_alertedEffects, ref creatureData.m_effects.m_alertedEffects);
        SaveEffectList(component.m_idleSound, ref creatureData.m_effects.m_idleSounds);
        SaveEffectList(component.m_wakeupEffects, ref creatureData.m_effects.m_wakeupEffects);
    }
    
    public static void Write(GameObject critter, string folderPath)
    {
        if (!critter.TryGetComponent(out MonsterAI component)) return;
        MonsterAIData data = RecordAIData(component);

        string filePath = folderPath + Path.DirectorySeparatorChar + "MonsterAI.yml";
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);

        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);

        string alertedEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "AlertedEffects.yml";
        string idleSoundPath = effectsFolder + Path.DirectorySeparatorChar + "IdleSounds.yml";
        string wakeupEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "WakeupEffects.yml";
        
        WriteEffectList(component.m_alertedEffects, alertedEffectsPath);
        WriteEffectList(component.m_idleSound, idleSoundPath);
        WriteEffectList(component.m_wakeupEffects, wakeupEffectsPath);
    }

    private static MonsterAIData RecordAIData(MonsterAI component)
    {
        MonsterAIData data = new()
        {
            ViewRange = component.m_viewRange,
            ViewAngle = component.m_viewAngle,
            HearRange = component.m_hearRange,
            MistVision = component.m_mistVision,
            IdleSoundInterval = component.m_idleSoundInterval,
            IdleSoundChance = component.m_idleSoundChance,
            PathAgentType = component.m_pathAgentType.ToString(),
            MoveMinAngle = component.m_moveMinAngle,
            SmoothMovement = component.m_smoothMovement,
            SerpentMovement = component.m_serpentMovement,
            SerpentTurnRadius = component.m_serpentTurnRadius,
            JumpInterval = component.m_jumpInterval,
            RandomCircleInterval = component.m_randomCircleInterval,
            RandomMoveInterval = component.m_randomMoveInterval,
            RandomMoveRange = component.m_randomMoveRange,
            RandomFly = component.m_randomFly,
            ChanceToTakeOff = component.m_chanceToTakeoff,
            ChanceToLand = component.m_chanceToLand,
            GroundDuration = component.m_groundDuration,
            AirDuration = component.m_airDuration,
            MaxLandAltitude = component.m_maxLandAltitude,
            TakeoffTime = component.m_takeoffTime,
            FlyAltitudeMin = component.m_flyAltitudeMin,
            FlyAltitudeMax = component.m_flyAltitudeMax,
            FlyAbsMinAltitude = component.m_flyAbsMinAltitude,
            AvoidFire = component.m_avoidFire,
            AfraidOfFire = component.m_afraidOfFire,
            AvoidWater = component.m_avoidWater,
            AvoidLava = component.m_avoidLava,
            SkipLavaTargets = component.m_skipLavaTargets,
            Aggravatable = component.m_aggravatable,
            PassiveAggressive = component.m_passiveAggresive,
            SpawnMessage = component.m_spawnMessage,
            DeathMessage = component.m_deathMessage,
            AlertedMessage = component.m_alertedMessage,
            FleeRange = component.m_fleeRange,
            FleeAngle = component.m_fleeAngle,
            FleeInterval = component.m_fleeInterval,
            AlertRange = component.m_alertRange,
            FleeIfHurtWhenTargetCannotBeReached = component.m_fleeIfHurtWhenTargetCantBeReached,
            FleeUnreachableSinceAttack = component.m_fleeUnreachableSinceAttacking,
            FleeUnreachableSinceHurt = component.m_fleeUnreachableSinceHurt,
            FleeIfNotAlerted = component.m_fleeIfNotAlerted,
            FleeIfLowHealth = component.m_fleeIfLowHealth,
            FleeInLava = component.m_fleeInLava,
            CirculateWhileCharging = component.m_circulateWhileCharging,
            CirculateWhileChargingFlying = component.m_circulateWhileChargingFlying,
            EnableHuntPlayer = component.m_enableHuntPlayer,
            AttackPlayerObjects = component.m_attackPlayerObjects,
            PrivateAreaTriggerThreshold = component.m_privateAreaTriggerTreshold,
            InterceptTimeMax = component.m_interceptTimeMax,
            InterceptTimeMin = component.m_interceptTimeMin,
            MaxChaseDistance = component.m_maxChaseDistance,
            MinAttackInterval = component.m_minAttackInterval,
            CircleTargetInterval = component.m_circleTargetInterval,
            CircleTargetDuration = component.m_circleTargetDuration,
            CircleTargetDistance = component.m_circleTargetDistance,
            Sleeping = component.m_sleeping,
            WakeupRange = component.m_wakeupRange,
            NoiseWakeup = component.m_noiseWakeup,
            MaxNoiseWakeupRange = component.m_maxNoiseWakeupRange,
            WakeupDelayMin = component.m_wakeUpDelayMin,
            WakeupDelayMax = component.m_wakeUpDelayMax,
            AvoidLand = component.m_avoidLand,
            ConsumeRange = component.m_consumeRange,
            ConsumeSearchRange = component.m_consumeSearchRange,
            ConsumeSearchInterval = component.m_consumeSearchInterval
        };
        foreach(ItemDrop item in component.m_consumeItems) data.ConsumeItems.Add(item.name);
        return data;
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "MonsterAI.yml";
        if (!File.Exists(filePath)) return;
        string serial = File.ReadAllText(filePath);
        if (serial.IsNullOrWhiteSpace()) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            MonsterAIData data = deserializer.Deserialize<MonsterAIData>(serial);
            creatureData.m_monsterAIData = data;
        }
        catch
        {
            LogParseFailure(filePath);
        }
        
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) return;
        string alertedEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "AlertedEffects.yml";
        string idleSoundPath = effectsFolder + Path.DirectorySeparatorChar + "IdleSounds.yml";
        string wakeupEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "WakeupEffects.yml";
        
        ReadEffectInfo(alertedEffectsPath, ref creatureData.m_effects.m_alertedEffects);
        ReadEffectInfo(idleSoundPath, ref creatureData.m_effects.m_idleSounds);
        ReadEffectInfo(wakeupEffectsPath, ref creatureData.m_effects.m_wakeupEffects);
    }

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out MonsterAI component)) return;
        MonsterAIData data = creatureData.m_monsterAIData;
        Vector3 scale = new Vector3(creatureData.m_scale.x, creatureData.m_scale.y, creatureData.m_scale.z);
        component.m_viewRange = data.ViewRange;
        component.m_viewAngle = data.ViewAngle;
        component.m_hearRange = data.HearRange;
        component.m_mistVision = data.MistVision;
        component.m_idleSoundInterval = data.IdleSoundInterval;
        component.m_idleSoundChance = data.IdleSoundChance;
        if (!Enum.TryParse(data.PathAgentType, true, out Pathfinding.AgentType agentType))
        {
            component.m_pathAgentType = agentType;
        }
        component.m_moveMinAngle = data.MoveMinAngle;
        component.m_smoothMovement = data.SmoothMovement;
        component.m_serpentMovement = data.SerpentMovement;
        component.m_serpentTurnRadius = data.SerpentTurnRadius;
        component.m_jumpInterval = data.JumpInterval;
        component.m_randomCircleInterval = data.RandomCircleInterval;
        component.m_randomMoveInterval = data.RandomMoveInterval;
        component.m_randomMoveRange = data.RandomMoveRange;
        component.m_randomFly = data.RandomFly;
        component.m_chanceToTakeoff = data.ChanceToTakeOff;
        component.m_chanceToLand = data.ChanceToLand;
        component.m_groundDuration = data.GroundDuration;
        component.m_airDuration = data.AirDuration;
        component.m_maxLandAltitude = data.MaxLandAltitude;
        component.m_takeoffTime = data.TakeoffTime;
        component.m_flyAltitudeMin = data.FlyAltitudeMin;
        component.m_flyAltitudeMax = data.FlyAltitudeMax;
        component.m_flyAbsMinAltitude = data.FlyAbsMinAltitude;
        component.m_avoidFire = data.AvoidFire;
        component.m_afraidOfFire = data.AfraidOfFire;
        component.m_avoidWater = data.AvoidWater;
        component.m_avoidLava = data.AvoidLava;
        component.m_skipLavaTargets = data.SkipLavaTargets;
        component.m_aggravatable = data.Aggravatable;
        component.m_passiveAggresive = data.PassiveAggressive;
        component.m_spawnMessage = data.SpawnMessage;
        component.m_deathMessage = data.DeathMessage;
        component.m_alertedMessage = data.AlertedMessage;
        component.m_fleeRange = data.FleeRange;
        component.m_fleeAngle = data.FleeAngle;
        component.m_fleeInterval = data.FleeInterval;
        component.m_alertRange = data.AlertRange;
        component.m_fleeIfHurtWhenTargetCantBeReached = data.FleeIfHurtWhenTargetCannotBeReached;
        component.m_fleeUnreachableSinceAttacking = data.FleeUnreachableSinceAttack;
        component.m_fleeUnreachableSinceHurt = data.FleeUnreachableSinceHurt;
        component.m_fleeIfNotAlerted = data.FleeIfNotAlerted;
        component.m_fleeTimeSinceHurt = data.FleeTimeSinceHurt;
        component.m_fleeIfLowHealth = data.FleeIfLowHealth;
        component.m_fleeInLava = data.FleeInLava;
        component.m_circulateWhileCharging = data.CirculateWhileCharging;
        component.m_circulateWhileChargingFlying = data.CirculateWhileChargingFlying;
        component.m_enableHuntPlayer = data.EnableHuntPlayer;
        component.m_attackPlayerObjects = data.AttackPlayerObjects;
        component.m_privateAreaTriggerTreshold = data.PrivateAreaTriggerThreshold;
        component.m_interceptTimeMax = data.InterceptTimeMax;
        component.m_interceptTimeMin = data.InterceptTimeMin;
        component.m_maxChaseDistance = data.MaxChaseDistance;
        component.m_minAttackInterval = data.MinAttackInterval;
        component.m_circleTargetInterval = data.CircleTargetInterval;
        component.m_circleTargetDuration = data.CircleTargetDuration;
        component.m_circleTargetDistance = data.CircleTargetDistance;
        component.m_sleeping = data.Sleeping;
        component.m_wakeupRange = data.WakeupRange;
        component.m_noiseWakeup = data.NoiseWakeup;
        component.m_maxNoiseWakeupRange = data.MaxNoiseWakeupRange;
        component.m_wakeUpDelayMin = data.WakeupDelayMin;
        component.m_wakeUpDelayMax = data.WakeupDelayMax;
        component.m_avoidLand = data.AvoidLand;
        component.m_consumeRange = data.ConsumeRange;
        component.m_consumeSearchRange = data.ConsumeSearchRange;
        component.m_consumeSearchInterval = data.ConsumeSearchInterval;
        component.m_consumeItems = new();
        foreach (string prefabName in data.ConsumeItems)
        {
            GameObject? prefab = DataBase.TryGetGameObject(prefabName);
            if (prefab == null) continue;
            if (!prefab.TryGetComponent(out ItemDrop itemDrop)) continue;
            component.m_consumeItems.Add(itemDrop);
        }
        UpdateEffectList(creatureData.m_effects.m_alertedEffects, ref component.m_alertedEffects, scale);
        UpdateEffectList(creatureData.m_effects.m_idleSounds, ref component.m_idleSound, scale);
        UpdateEffectList(creatureData.m_effects.m_wakeupEffects, ref component.m_wakeupEffects, scale);
    }
}