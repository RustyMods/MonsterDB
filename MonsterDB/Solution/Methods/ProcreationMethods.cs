﻿using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class ProcreationMethods
{
    public static List<string> m_oldProcreators = new();
    public static List<string> m_removedOldProcreators = new();
    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out Procreation component)) return;
        creatureData.m_procreation = RecordProcreationData(component);
        SaveEffectList(component.m_birthEffects, ref creatureData.m_effects.m_birthEffects);
        SaveEffectList(component.m_loveEffects, ref creatureData.m_effects.m_loveEffects);
        if (!m_oldProcreators.Contains(critter.name)) m_oldProcreators.Add(critter.name);
    }
    
    public static void Write(GameObject critter, string folderPath)
    {
        if (!critter.TryGetComponent(out Procreation component)) return;
        string filePath = folderPath + Path.DirectorySeparatorChar + "Procreation.yml";
        ISerializer serializer = new SerializerBuilder().Build();
        ProcreationData data = RecordProcreationData(component);
        string serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);
        
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);
        string birthEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "BirthEffects.yml";
        string loveEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "LoveEffects.yml";
        WriteEffectList(component.m_birthEffects, birthEffectsPath);
        WriteEffectList(component.m_loveEffects, loveEffectsPath);
    }

    private static ProcreationData RecordProcreationData(Procreation component)
    {
        return new ProcreationData
        {
            UpdateInterval = component.m_updateInterval,
            TotalCheckRange = component.m_totalCheckRange,
            MaxCreatures = component.m_maxCreatures,
            PartnerCheckRange = component.m_partnerCheckRange,
            PregnancyChance = component.m_pregnancyChance,
            PregnancyDuration = component.m_pregnancyDuration,
            RequiredLovePoints = component.m_requiredLovePoints,
            Offspring = component.m_offspring.name,
            MinOffspringLevel = component.m_minOffspringLevel,
            SpawnOffset = component.m_spawnOffset
        };
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "Procreation.yml";
        if (!File.Exists(filePath)) return;
        string serial = File.ReadAllText(filePath);
        if (serial.IsNullOrWhiteSpace()) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            ProcreationData data = deserializer.Deserialize<ProcreationData>(serial);
            creatureData.m_procreation = data;
        }
        catch
        {
            LogParseFailure(filePath);
        }
        
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) return;
        string birthEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "BirthEffects.yml";
        string loveEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "LoveEffects.yml";
        ReadEffectInfo(birthEffectsPath, ref creatureData.m_effects.m_birthEffects);
        ReadEffectInfo(loveEffectsPath, ref creatureData.m_effects.m_loveEffects);
    }

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        ProcreationData data = creatureData.m_procreation;
        if (data.Offspring.IsNullOrWhiteSpace())
        {
            if (RemoveComponent(critter)) return;
        }
        if (!critter.GetComponent<Tameable>() || !critter.GetComponent<Character>() || !critter.GetComponent<BaseAI>()) return;
        Vector3 scale = GetScale(creatureData.m_scale);
        GameObject? offspring = DataBase.TryGetGameObject(data.Offspring);
        if (offspring == null) return;
        if (!critter.TryGetComponent(out Procreation component))
        {
            component = critter.AddComponent<Procreation>();
            if (m_removedOldProcreators.Contains(critter.name)) m_removedOldProcreators.Remove(critter.name);
        }
        component.m_updateInterval = data.UpdateInterval;
        component.m_totalCheckRange = data.TotalCheckRange;
        component.m_maxCreatures = data.MaxCreatures;
        component.m_partnerCheckRange = data.PartnerCheckRange;
        component.m_pregnancyChance = data.PregnancyChance;
        component.m_pregnancyDuration = data.PregnancyDuration;
        component.m_requiredLovePoints = data.RequiredLovePoints;
        component.m_offspring = offspring;
        component.m_minOffspringLevel = data.MinOffspringLevel;
        component.m_spawnOffset = data.SpawnOffset;
        UpdateEffectList(creatureData.m_effects.m_birthEffects, ref component.m_birthEffects, scale);
        UpdateEffectList(creatureData.m_effects.m_loveEffects, ref component.m_loveEffects, scale);
    }

    private static bool RemoveComponent(GameObject critter)
    {
        if (!m_oldProcreators.Contains(critter.name)) return false;
        if (!critter.TryGetComponent(out Procreation component)) return false;
        Object.Destroy(component);
        m_removedOldProcreators.Add(critter.name);
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Procreation removed from {critter.name} that originally had one");
        return true;
    }
}