using System;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class MonsterAIMethods
{
    public static void ReadMonsterAI(string folderPath, ref CreatureData creatureData)
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
}