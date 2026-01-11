using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class Helpers
{
    public static void ReadEffectInfo(string filePath, ref List<EffectInfo> effectList)
    {
        if (!File.Exists(filePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string serial = File.ReadAllText(filePath);
        if (serial.IsNullOrWhiteSpace()) return;
        try
        {
            effectList = deserializer.Deserialize<List<EffectInfo>>(serial);
        }
        catch
        {
            LogParseFailure(filePath);
        }
    }

    public static void LogParseFailure(string filePath)
    {
        MonsterDBPlugin.LogDebug("Failed to parse file:");
        MonsterDBPlugin.LogDebug(filePath);
    }
}