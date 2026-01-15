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
    public static void ReadTameable(string folderPath, ref CreatureData creatureData)
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

        Vector3 scale = creatureData.m_scale.ToRef();
        if (!critter.TryGetComponent(out Tameable component))
        {
            return;
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
}