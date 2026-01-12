using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class CharacterDropMethods
{
    public static void ReadCharacterDrops(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "CharacterDrop.yml";
        if (!File.Exists(filePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string serial = File.ReadAllText(filePath);
        if (serial.IsNullOrWhiteSpace()) return;
        try
        {
            List<CharacterDropData> data = deserializer.Deserialize<List<CharacterDropData>>(serial);
            creatureData.m_characterDrops = data;
        }
        catch
        {
            LogParseFailure(filePath);
        }
    }
}