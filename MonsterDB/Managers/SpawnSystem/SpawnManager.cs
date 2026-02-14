using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace MonsterDB;

public static class SpawnManager
{
    private static readonly SpawnSystemList SpawnList;
    private static Dictionary<string, SpawnDataRef> spawnRefs;
    private static readonly CustomSyncedValue<string> sync;
    private static readonly Dictionary<string, SpawnSystem.SpawnData> CachedSpawnData;
    private static readonly Dictionary<string, BaseSpawnData> updateList;
    
    static SpawnManager()
    {
        spawnRefs = new Dictionary<string, SpawnDataRef>();
        CachedSpawnData = new Dictionary<string, SpawnSystem.SpawnData>();
        updateList = new Dictionary<string, BaseSpawnData>();
        sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.SpawnList");
        SpawnList = MonsterDBPlugin.instance.gameObject.AddComponent<SpawnSystemList>();
        sync.ValueChanged += OnSyncChange;
    }
    
    public static void Add(SpawnDataRef data)
    {
        spawnRefs[data.m_name] = data;
    }

    public static void QueueUpdate(BaseSpawnData data)
    {
        updateList[data.Prefab] = data;
    }

    public static List<string> GetCachedSpawnDataIds() => CachedSpawnData.Keys.ToList();
    
    public static bool TryGetSpawnData(string id, out SpawnSystem.SpawnData data) => CachedSpawnData.TryGetValue(id, out data);

    public static void Export(string spawnerId)
    {
        if (!CachedSpawnData.TryGetValue(spawnerId, out SpawnSystem.SpawnData data)) return;
        BaseSpawnData info = new BaseSpawnData();
        info.SetupData(data);
        string text = ConfigManager.Serialize(info);
        string filePath = Path.Combine(FileManager.ExportFolder, info.Prefab + ".yml");
        File.WriteAllText(filePath, text);
    }

    public static void Start()
    {
        foreach (SpawnDataRef? data in spawnRefs.Values)
        {
            SpawnSystem.SpawnData spawn = new SpawnSystem.SpawnData();
            data.UpdateFields(spawn, data.m_name, false);
            SpawnList.m_spawners.Add(spawn);
        }
        MonsterDBPlugin.LogInfo($"Loaded {spawnRefs.Count} spawn files");
    }

    public static void Clear()
    {
        SpawnList.m_spawners.Clear();
        spawnRefs.Clear();
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            UpdateSync();
        }
    }

    private static void UpdateSync()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        
        string text = ConfigManager.Serialize(spawnRefs);
        sync.Value = text;
    }

    private static void OnSyncChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;

        if (string.IsNullOrEmpty(sync.Value)) return;
        
        Dictionary<string, SpawnDataRef> data = ConfigManager.Deserialize<Dictionary<string, SpawnDataRef>>(sync.Value);
        spawnRefs = data;
        SpawnList.m_spawners.Clear();
        foreach (KeyValuePair<string, SpawnDataRef> spawnRef in spawnRefs)
        {
            SpawnSystem.SpawnData spawnData = new SpawnSystem.SpawnData();
            spawnRef.Value.UpdateFields(spawnData, spawnRef.Key, false);
            SpawnList.m_spawners.Add(spawnData);
        }
    }
    
    public static void Update(SpawnDataRef info)
    {
        SpawnList.m_spawners.RemoveAll(x => x.m_name == info.m_name);
        spawnRefs[info.m_name] = info;
        SpawnSystem.SpawnData spawnData = new SpawnSystem.SpawnData();
        info.UpdateFields(spawnData, info.m_name, false);
        SpawnList.m_spawners.Add(spawnData);
        
        UpdateSync();
            
        MonsterDBPlugin.LogInfo($"Updated Spawn Data: {info.m_name}");
    }

    public static void Setup()
    {
        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(SpawnSystem), nameof(SpawnSystem.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(SpawnManager), nameof(Patch_SpawnSystem_Awake))));
    }

    public static void Patch_SpawnSystem_Awake(SpawnSystem __instance)
    {
        ValidateSpawns();
        __instance.m_spawnLists.Add(SpawnList);
        CacheAndUpdateSpawners(__instance);
    }

    private static void CacheAndUpdateSpawners(SpawnSystem __instance)
    {
        List<SpawnSystemList>? lists = __instance.m_spawnLists;
        for (int i = 0; i < lists.Count; ++i)
        {
            SpawnSystemList? list =  lists[i];
            List<SpawnSystem.SpawnData>? spawners = list.m_spawners;
            for (int j = 0; j < spawners.Count; ++j)
            {
                SpawnSystem.SpawnData? spawner = spawners[j];
                CachedSpawnData[spawner.m_name] = spawner;
                if (updateList.TryGetValue(spawner.m_name, out BaseSpawnData? data))
                {
                    data.UpdateData(spawner);
                }
            }
        }
    }

    private static void ValidateSpawns()
    {
        for (var i = 0; i < SpawnList.m_spawners.Count; ++i)
        {
            SpawnSystem.SpawnData? spawn = SpawnList.m_spawners[i];
            if (!spawn.m_enabled) continue;
            
            if (spawn.m_prefab == null)
            {
                MonsterDBPlugin.LogWarning($"{spawn.m_name} prefab is null, disabling");
                spawn.m_enabled = false;
            }
        }
    }
}