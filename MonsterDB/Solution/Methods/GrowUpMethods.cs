using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class GrowUpMethods
{
    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out Growup component)) return;
        creatureData.m_growUp = RecordData(component);
    }

    private static GrowUpData RecordData(Growup component)
    {
        var data = new GrowUpData();
        data.GrowTime = component.m_growTime;
        data.InheritTame = component.m_inheritTame;
        if (component.m_grownPrefab) data.GrownPrefab = component.m_grownPrefab.name;
        foreach (var alt in component.m_altGrownPrefabs)
        {
            if (!alt.m_prefab) continue;
            data.AltGrownPrefabs.Add(new AltGrownData()
            {
                GrownPrefab = alt.m_prefab.name,
                Weight = alt.m_weight
            });
        }
        return data;
    }

    public static void Write(GameObject critter, string folderPath)
    {
        if (!critter.TryGetComponent(out Growup component)) return;
        var data = RecordData(component);

        string filePath = folderPath + Path.DirectorySeparatorChar + "GrowUp.yml";
        var serializer = new SerializerBuilder().Build();
        var serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);
    }

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

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        GrowUpData data = creatureData.m_growUp;
        if (!critter.TryGetComponent(out Growup component)) return;
        var growUpPrefab = DataBase.TryGetGameObject(data.GrownPrefab);
        if (growUpPrefab == null) return;
        
        component.m_growTime = data.GrowTime;
        component.m_inheritTame = data.InheritTame;
        component.m_grownPrefab = growUpPrefab;

        component.m_altGrownPrefabs = new();
        foreach (var altData in data.AltGrownPrefabs)
        {
            var prefab = DataBase.TryGetGameObject(altData.GrownPrefab);
            if (prefab == null) continue;
            component.m_altGrownPrefabs.Add(new Growup.GrownEntry()
            {
                m_prefab = prefab,
                m_weight = altData.Weight
            });
        }
    }
}