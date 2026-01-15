using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class GrowUpMethods
{
    public static void ReadGrowUp(string folderPath, ref CreatureData creatureData)
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
        var growUpPrefab = PrefabManager.GetPrefab(data.GrownPrefab);
        if (growUpPrefab == null) return;
        
        component.m_growTime = data.GrowTime;
        component.m_inheritTame = data.InheritTame;
        component.m_grownPrefab = growUpPrefab;

        component.m_altGrownPrefabs = new();
        foreach (var altData in data.AltGrownPrefabs)
        {
            var prefab = PrefabManager.GetPrefab(altData.GrownPrefab);
            if (prefab == null) continue;
            component.m_altGrownPrefabs.Add(new Growup.GrownEntry()
            {
                m_prefab = prefab,
                m_weight = altData.Weight
            });
        }
    }
    
}