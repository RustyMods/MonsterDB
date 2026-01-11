using System.Collections.Generic;
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
}