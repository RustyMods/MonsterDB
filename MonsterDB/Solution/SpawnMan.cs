using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution;

public static class SpawnMan
{
    private static readonly CustomSyncedValue<string> ServerSpawnSystem = new(MonsterDBPlugin.ConfigSync, "MonsterDB_SpawnSystemData", "");
    private static readonly string m_spawnFolderPath = CreatureManager.m_folderPath + Path.DirectorySeparatorChar + "SpawnData";
    private static SpawnSystemList m_spawnSystemList = null!;
    private static Dictionary<string, CreatureSpawnData> m_spawnData = new();

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
    private static class SpawnSystem_Awake_Patch
    {
        private static void Postfix(SpawnSystem __instance)
        {
            if (!__instance) return;
            if (__instance.m_spawnLists.Contains(m_spawnSystemList)) return;
            __instance.m_spawnLists.Add(m_spawnSystemList);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static class ZNet_Awake_Patch
    {
        private static void Postfix() => UpdateServerFiles();
    }
    
    public static void Setup()
    {
        m_spawnSystemList = MonsterDBPlugin.m_root.AddComponent<SpawnSystemList>();
        ReadSpawnFiles();
        string exampleFile = m_spawnFolderPath + Path.DirectorySeparatorChar + "Example.yml";
        if (!File.Exists(exampleFile))
        {
            CreatureSpawnData data = new CreatureSpawnData();
            ISerializer serializer = new SerializerBuilder().Build();
            string serial = serializer.Serialize(data);
            File.WriteAllText(exampleFile, serial);
        }

        ServerSpawnSystem.ValueChanged += () =>
        {
            if (!ZNet.instance || ZNet.instance.IsServer()) return;
            UpdateSpawnData();
        };
        
        SetupFileWatch();
    }

    private static void SetupFileWatch()
    {
        FileSystemWatcher watcher = new FileSystemWatcher(m_spawnFolderPath, "*.yml")
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };
        watcher.Created += OnSpawnChange;
        watcher.Deleted += OnSpawnChange;
        watcher.Changed += OnSpawnChange;
    }
    
    private static void OnSpawnChange(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string fileName = Path.GetFileName(e.Name);
        if (fileName == "Example.yml") return;
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                MonsterDBPlugin.MonsterDBLogger.LogInfo("File changed: " + fileName);
                ReadFile(e.FullPath);
                break;
            case WatcherChangeTypes.Created:
                MonsterDBPlugin.MonsterDBLogger.LogInfo("File created: " + fileName);
                ReadFile(e.FullPath);
                break;
            case WatcherChangeTypes.Deleted:
                MonsterDBPlugin.MonsterDBLogger.LogInfo("File deleted: " + fileName);
                ClearSpawnData();
                ReadSpawnFiles();
                break;
        }
    }

    private static void ReadFile(string filePath)
    {
        try
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            string serial = File.ReadAllText(filePath);
            CreatureSpawnData data = deserializer.Deserialize<CreatureSpawnData>(serial);
            m_spawnData[data.m_name] = data;
            UpdateSpawnList();
            UpdateServerFiles();
        }
        catch
        {
            Methods.Helpers.LogParseFailure(filePath);
        }
    }

    private static void ReadSpawnFiles()
    {
        if (!Directory.Exists(m_spawnFolderPath)) Directory.CreateDirectory(m_spawnFolderPath);
        string[] files = Directory.GetFiles(m_spawnFolderPath, "*.yml");
        IDeserializer deserializer = new DeserializerBuilder().Build();
        int count = 0;
        foreach (string file in files)
        {
            if (file.EndsWith("Example.yml")) continue;
            string text = File.ReadAllText(file);
            try
            {
                CreatureSpawnData data = deserializer.Deserialize<CreatureSpawnData>(text);
                m_spawnData[data.m_name] = data;
                ++count;
            }
            catch
            {
                Methods.Helpers.LogParseFailure(file);
            }
        }
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Registered {count} spawn files");
    }
    
    public static void UpdateSpawnData()
    {
        if (ServerSpawnSystem.Value.IsNullOrWhiteSpace()) return;
        try
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            Dictionary<string, CreatureSpawnData> data = deserializer.Deserialize<Dictionary<string, CreatureSpawnData>>(ServerSpawnSystem.Value);
            ClearSpawnData();
            AddSpawnData(data);
            UpdateSpawnList();
            MonsterDBPlugin.MonsterDBLogger.LogDebug($"Loaded {data.Count} spawn files");
        }
        catch
        {
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Failed to parse server spawn files");
        }
    }

    private static void ClearSpawnData() => m_spawnData.Clear();
    private static void AddSpawnData(Dictionary<string, CreatureSpawnData> data) => m_spawnData = data;
    private static void UpdateSpawnList() => m_spawnSystemList.m_spawners = m_spawnData.Values.Select(FormatSpawnData).Where(info => info != null).ToList();
    
    private static SpawnSystem.SpawnData? FormatSpawnData(CreatureSpawnData input)
    {
        if (input.m_name.IsNullOrWhiteSpace()) return null;
        GameObject? prefab = DataBase.TryGetGameObject(input.m_prefab);
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
    
    private static void UpdateServerFiles()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(m_spawnData);
        ServerSpawnSystem.Value = serial;
    }

    private class CreatureSpawnData
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
}

