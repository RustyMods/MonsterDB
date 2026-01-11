using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class LevelEffectsMethods
{
    private static readonly int Hue = Shader.PropertyToID("_Hue");
    private static readonly int Saturation = Shader.PropertyToID("_Saturation");
    private static readonly int Value = Shader.PropertyToID("_Value");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    
    public static void Read(string folderPath, ref CreatureData creatureData)
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