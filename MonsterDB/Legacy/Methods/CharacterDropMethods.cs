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
    
    public static void Update(GameObject critter, CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out CharacterDrop component)) return;
        var data = creatureData.m_characterDrops;
        List<CharacterDrop.Drop> list = new();
        foreach (var info in data)
        {
            var prefab = PrefabManager.GetPrefab(info.PrefabName);
            if (prefab == null) continue;
            list.Add(new CharacterDrop.Drop
            {
                m_prefab = prefab,
                m_amountMin = info.AmountMin,
                m_amountMax = info.AmountMax,
                m_chance = info.Chance,
                m_onePerPlayer = info.OnePerPlayer,
                m_levelMultiplier = info.LevelMultiplier,
                m_dontScale = info.DoNotScale
            });
        }

        component.m_drops = list;
    }
}