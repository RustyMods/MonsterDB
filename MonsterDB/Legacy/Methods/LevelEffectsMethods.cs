using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class LevelEffectsMethods
{
    
    public static void ReadLevelEffects(string folderPath, ref CreatureData creatureData)
    {
        var filePath = folderPath + Path.DirectorySeparatorChar + "LevelEffects.yml";
        if (!File.Exists(filePath)) return;
        var deserializer = new DeserializerBuilder().Build();
        try
        {
            var serial = File.ReadAllText(filePath);
            creatureData.m_levelEffects = deserializer.Deserialize<List<LevelEffectData>>(serial);
        }
        catch
        {
            Helpers.LogParseFailure(filePath);
        }
    }
}