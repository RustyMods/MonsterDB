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
}