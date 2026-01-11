using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class GrowUpMethods
{
    public static void Read(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "GrowUp.yml";
        if (!File.Exists(filePath)) return;
        string serial = File.ReadAllText(filePath);
        try
        {
            var deserializer = new DeserializerBuilder().Build();
            var data = deserializer.Deserialize<GrowUpData>(serial);
            creatureData.m_growUp = data;
        }
        catch
        {
            Helpers.LogParseFailure(filePath);
        }
    }

    
}