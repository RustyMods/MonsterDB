using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.DataBase;

public static class SpawnData
{
    private static SpawnSystemList m_spawnSystemList = null!;

    private static Dictionary<string, MonsterSpawnData> m_spawnData = new();

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
    private static class SpawnSystem_Patch
    {
        public static void Postfix(SpawnSystem __instance)
        {
            if (!__instance) return;
            if (__instance.m_spawnLists.Contains(m_spawnSystemList)) return;
            __instance.m_spawnLists.Add(m_spawnSystemList);
        }
    }

    public static void ReadSpawnFiles()
    {
        if (!ZNet.instance.IsServer()) return;
        string[] files = Directory.GetFiles(Paths.SpawnPath, "*.yml");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        int count = 0;
        foreach (string file in files)
        {
            string text = File.ReadAllText(file);
            MonsterSpawnData data = deserializer.Deserialize<MonsterSpawnData>(text);
            m_spawnData[data.m_name] = data;
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Server: Registered " + count + " spawn data files");
        
        UpdateServerFiles();
        
    }

    private static void UpdateServerFiles()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(m_spawnData);
        ServerSync.UpdateServerSpawnSystem(serial);
    }
    
    public static void InitSpawnSystem()
    {
        m_spawnSystemList = MonsterDBPlugin._Root.AddComponent<SpawnSystemList>();

        string exampleFile = Paths.SpawnPath + Path.DirectorySeparatorChar + "Example.yml";
        if (File.Exists(exampleFile)) return;
        MonsterSpawnData data = new MonsterSpawnData();
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        File.WriteAllText(exampleFile, serial);
    }

    public static void ReadFile(string filePath)
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string serial = File.ReadAllText(filePath);
        MonsterSpawnData data = deserializer.Deserialize<MonsterSpawnData>(serial);
        m_spawnData[data.m_name] = data;
        UpdateSpawnList();
        MonsterDBPlugin.MonsterDBLogger.LogInfo("Updated spawn list");
        UpdateServerFiles();
        
    }

    private static SpawnSystem.SpawnData? FormatSpawnData(MonsterSpawnData input)
    {
        if (input.m_name.IsNullOrWhiteSpace()) return null;
        GameObject? prefab = MonsterDB.TryGetGameObject(input.m_prefab);
        if (prefab == null) return null;
        if (!Enum.TryParse(input.m_biome, out Heightmap.Biome biome)) return null;
        if (!Enum.TryParse(input.m_biomeArea, out Heightmap.BiomeArea biomeArea)) return null;
        return new SpawnSystem.SpawnData()
        {
            m_name = input.m_name,
            m_enabled = input.m_enabled,
            m_devDisabled = input.m_devDisabled,
            m_prefab = prefab,
            m_biome = biome,
            m_biomeArea = biomeArea,
            m_maxSpawned = input.m_maxSpawned,
            m_spawnInterval = input.m_spawnInterval,
            m_spawnChance = input.m_spawnChance,
            m_spawnDistance = input.m_spawnDistance,
            m_spawnRadiusMin = input.m_spawnRadiusMin,
            m_spawnRadiusMax = input.m_spawnRadiusMax,
            m_requiredGlobalKey = input.m_requiredGlobalKey,
            m_requiredEnvironments = input.m_requiredEnvironments,
            m_groupSizeMin = input.m_groupSizeMin,
            m_groupSizeMax = input.m_groupSizeMax,
            m_groupRadius = input.m_groupRadius,
            m_spawnAtNight = input.m_spawnAtNight,
            m_spawnAtDay = input.m_spawnAtDay,
            m_minAltitude = input.m_minAltitude,
            m_maxAltitude = input.m_maxAltitude,
            m_minTilt = input.m_minTilt,
            m_maxTilt = input.m_maxTilt,
            m_inForest = input.m_inForest,
            m_outsideForest = input.m_outsideForest,
            m_inLava = input.m_inLava,
            m_outsideLava = input.m_outsideLava,
            m_canSpawnCloseToPlayer = input.m_canSpawnCloseToPlayer,
            m_insidePlayerBase = input.m_insidePlayerBase,
            m_minOceanDepth = input.m_minOceanDepth,
            m_maxOceanDepth = input.m_maxOceanDepth,
            m_huntPlayer = input.m_huntPlayer,
            m_groundOffset = input.m_groundOffset,
            m_groundOffsetRandom = input.m_groundOffsetRandom,
            m_maxLevel = input.m_maxLevel,
            m_minLevel = input.m_minLevel,
            m_levelUpMinCenterDistance = input.m_levelUpMinCenterDistance,
            m_overrideLevelupChance = input.m_overrideLevelupChance,
            m_foldout = input.m_foldout
        };
    }

    public static void ClearSpawnData() => m_spawnData.Clear();
    public static void AddSpawnData(Dictionary<string, MonsterSpawnData> data) => m_spawnData = data;
    public static void UpdateSpawnList() => m_spawnSystemList.m_spawners = m_spawnData.Values.Select(FormatSpawnData).Where(info => info != null).ToList();
}

[Serializable][CanBeNull]
public class MonsterSpawnData
{
    public string m_name = "";
    public bool m_enabled = false;
    public bool m_devDisabled;
    public string m_prefab = null!;
    public string m_biome = null!;
    public string m_biomeArea = "Everything";
    public int m_maxSpawned = 1;
    public float m_spawnInterval = 4f;
    public float m_spawnChance = 100f;
    public float m_spawnDistance = 10f;
    public float m_spawnRadiusMin;
    public float m_spawnRadiusMax;
    public string m_requiredGlobalKey = "";
    public List<string> m_requiredEnvironments = new List<string>();
    public int m_groupSizeMin = 1;
    public int m_groupSizeMax = 1;
    public float m_groupRadius = 3f;
    public bool m_spawnAtNight = true;
    public bool m_spawnAtDay = true;
    public float m_minAltitude = -1000f;
    public float m_maxAltitude = 1000f;
    public float m_minTilt;
    public float m_maxTilt = 35f;
    public bool m_inForest = true;
    public bool m_outsideForest = true;
    public bool m_inLava;
    public bool m_outsideLava = true;
    public bool m_canSpawnCloseToPlayer;
    public bool m_insidePlayerBase;
    public float m_minOceanDepth;
    public float m_maxOceanDepth;
    public bool m_huntPlayer;
    public float m_groundOffset = 0.5f;
    public float m_groundOffsetRandom;
    public int m_maxLevel = 1;
    public int m_minLevel = 1;
    public float m_levelUpMinCenterDistance;
    public float m_overrideLevelupChance = -1f;
    public bool m_foldout;
}