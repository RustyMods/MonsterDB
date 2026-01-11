using System;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class AnimalAIMethods
{
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
}