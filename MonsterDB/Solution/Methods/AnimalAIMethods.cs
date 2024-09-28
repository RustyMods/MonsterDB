using System;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class AnimalAIMethods
{
    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out AnimalAI component)) return;
        creatureData.m_animalAIData = RecordAIData(component);
        SaveEffectList(component.m_alertedEffects, ref creatureData.m_effects.m_alertedEffects);
        SaveEffectList(component.m_idleSound, ref creatureData.m_effects.m_idleSounds);
    }
    
    public static void Write(GameObject critter, string folderPath)
    {
        if (!critter.TryGetComponent(out AnimalAI component)) return;
        AnimalAIData data = RecordAIData(component);

        string filePath = folderPath + Path.DirectorySeparatorChar + "AnimalAI.yml";
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);

        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);

        string alertedEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "AlertedEffects.yml";
        string idleSoundPath = effectsFolder + Path.DirectorySeparatorChar + "IdleSounds.yml";
        
        WriteEffectList(component.m_alertedEffects, alertedEffectsPath);
        WriteEffectList(component.m_idleSound, idleSoundPath);
    }

    private static AnimalAIData RecordAIData(AnimalAI component)
    {
        return new AnimalAIData
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
        };
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "AnimalAI.yml";
        if (!File.Exists(filePath)) return;
        string serial = File.ReadAllText(filePath);
        if (serial.IsNullOrWhiteSpace()) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            AnimalAIData data = deserializer.Deserialize<AnimalAIData>(serial);
            creatureData.m_animalAIData = data;
            string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
            if (!Directory.Exists(effectsFolder)) return;
            string alertedEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "AlertedEffects.yml";
            string idleSoundPath = effectsFolder + Path.DirectorySeparatorChar + "IdleSounds.yml";

            ReadEffectInfo(alertedEffectsPath, ref creatureData.m_effects.m_alertedEffects);
            ReadEffectInfo(idleSoundPath, ref creatureData.m_effects.m_idleSounds);
        }
        catch
        {
            LogParseFailure(filePath);
        }
    }

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        AnimalAIData data = creatureData.m_animalAIData;
        Vector3 scale = GetScale(creatureData.m_scale);
        CharacterEffects effectData = creatureData.m_effects;
        
        if (!critter.TryGetComponent(out AnimalAI component)) return;
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
        
        UpdateEffectList(effectData.m_alertedEffects, ref component.m_alertedEffects, scale);
        UpdateEffectList(effectData.m_idleSounds, ref component.m_alertedEffects, scale);

    }
}