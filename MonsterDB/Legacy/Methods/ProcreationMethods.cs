using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class ProcreationMethods
{
    public static void ReadProcreation(string folderPath, ref CreatureData creatureData)
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
        if (!critter.GetComponent<Tameable>() || !critter.GetComponent<Character>() || !critter.GetComponent<BaseAI>()) return;
        if (!critter.TryGetComponent(out Procreation component))
        {
            return;
        }
        Vector3 scale =creatureData.m_scale.ToRef();
        GameObject? offspring = PrefabManager.GetPrefab(data.Offspring);
        if (offspring == null) return;
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
}