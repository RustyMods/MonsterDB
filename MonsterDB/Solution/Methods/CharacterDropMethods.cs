using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class CharacterDropMethods
{
    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out CharacterDrop component)) return;
        foreach (var drop in component.m_drops)
        {
            creatureData.m_characterDrops.Add(new CharacterDropData()
            {
                PrefabName = drop.m_prefab.name,
                AmountMin = drop.m_amountMin,
                AmountMax = drop.m_amountMax,
                Chance = drop.m_chance,
                OnePerPlayer = drop.m_onePerPlayer,
                LevelMultiplier = drop.m_levelMultiplier,
                DoNotScale = drop.m_dontScale
            });
        }
    }
    
    public static void Write(GameObject critter, string folderPath)
    {
        if (!critter.TryGetComponent(out CharacterDrop component)) return;
        List<CharacterDropData> dropData = new();
        foreach (var drop in component.m_drops)
        {
            dropData.Add(new CharacterDropData()
            {
                PrefabName = drop.m_prefab.name,
                AmountMin = drop.m_amountMin,
                AmountMax = drop.m_amountMax,
                Chance = drop.m_chance,
                OnePerPlayer = drop.m_onePerPlayer,
                LevelMultiplier = drop.m_levelMultiplier,
                DoNotScale = drop.m_dontScale
            });
        }

        string filePath = folderPath + Path.DirectorySeparatorChar + "CharacterDrop.yml";
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(dropData);
        File.WriteAllText(filePath, serial);
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
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
            var prefab = DataBase.TryGetGameObject(info.PrefabName);
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