using System.Collections.Generic;
using System.IO;
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
    
    static SpawnManager()
    {
        spawnRefs = new Dictionary<string, SpawnDataRef>();
        sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.SpawnList");
        SpawnList = MonsterDBPlugin.instance.gameObject.AddComponent<SpawnSystemList>();
        sync.ValueChanged += OnSyncChange;
    }
    
    public static void Add(SpawnDataRef data)
    {
        spawnRefs[data.m_name] = data;
    }

    private static void Start()
    {
        foreach (SpawnDataRef? data in spawnRefs.Values)
        {
            SpawnSystem.SpawnData spawn = data;
            SpawnList.m_spawners.Add(spawn);
        }
        MonsterDBPlugin.LogInfo($"Loaded {spawnRefs.Count} spawn files");
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            Start();
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
            SpawnSystem.SpawnData spawnData = spawnRef.Value;
            SpawnList.m_spawners.Add(spawnData);
        }
    }

    public static void Update(SpawnDataRef info)
    {
        SpawnList.m_spawners.RemoveAll(x => x.m_name == info.m_name);
        spawnRefs[info.m_name] = info;
        SpawnSystem.SpawnData spawnData = info;
        SpawnList.m_spawners.Add(spawnData);
        
        UpdateSync();
            
        MonsterDBPlugin.LogInfo($"Updated Spawn Data: {info.m_name}");
    }

    public static void Setup()
    {
        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(SpawnSystem), nameof(SpawnSystem.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(SpawnManager), nameof(Patch_SpawnSystem_Awake))));
    }

    public static void Patch_SpawnSystem_Awake(SpawnSystem __instance)
    {
        ValidateSpawns();
        __instance.m_spawnLists.Add(SpawnList);
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