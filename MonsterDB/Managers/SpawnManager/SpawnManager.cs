using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace MonsterDB;

public static class SpawnManager
{
    private static readonly SpawnSystemList SpawnList;
    private const string FolderName = "Spawns";
    private static readonly string FolderPath;
    private static Dictionary<string, SpawnDataRef> spawnRefs;
    private static readonly Dictionary<SpawnDataRef, SpawnSystem.SpawnData> map;
    private static readonly CustomSyncedValue<string> sync;
    
    static SpawnManager()
    {
        FolderPath = Path.Combine(ConfigManager.DirectoryPath, FolderName);
        spawnRefs = new Dictionary<string, SpawnDataRef>();
        map = new Dictionary<SpawnDataRef, SpawnSystem.SpawnData>();
        sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.SpawnList");
        SpawnList = MonsterDBPlugin.instance.gameObject.AddComponent<SpawnSystemList>();
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
        
        sync.ValueChanged += OnSyncChange;

        Command create = new Command("setup_spawn",
            "[prefabName]: write new spawn data file and load into spawn system",
            args =>
            {
                if (args.Length < 3)
                {
                    MonsterDBPlugin.LogWarning("Invalid parameters");
                    return true;
                }
                string? prefabName = args[2];
                GameObject? prefab = PrefabManager.GetPrefab(prefabName);
                if (prefab == null || !prefab.GetComponent<Character>())
                {
                    MonsterDBPlugin.LogWarning("Invalid prefab");
                    return true;
                }
                Create(prefab);
                return true;
            }, optionsFetcher: PrefabManager.GetAllPrefabNames<Character>, adminOnly: true);
    }

    private static void Start()
    {
        string[] files =  Directory.GetFiles(FolderPath);
        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            string text = File.ReadAllText(filePath);
            try
            {
                SpawnDataRef data = ConfigManager.Deserialize<SpawnDataRef>(text);
                spawnRefs[filePath] = data;
                SpawnSystem.SpawnData spawnData = data;
                SpawnList.m_spawners.Add(spawnData);
            }
            catch
            {
                MonsterDBPlugin.LogWarning($"Failed to deserialize {Path.GetFileName(filePath)}");
            }
        }
        MonsterDBPlugin.LogInfo($"Loaded {files.Length} spawn files");
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            Start();
            UpdateSync();
            SetupFileWatcher();
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
        map.Clear();
        foreach (KeyValuePair<string, SpawnDataRef> spawnRef in spawnRefs)
        {
            SpawnSystem.SpawnData spawnData = spawnRef.Value;
            SpawnList.m_spawners.Add(spawnData);
            map[spawnRef.Value] = spawnData;
        }
    }

    private static void SetupFileWatcher()
    {
        FileSystemWatcher watcher = new(FolderPath, "*.yml");
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        
        string? filePath = e.FullPath;
        string fileName = Path.GetFileName(filePath);
        try
        {
            SpawnDataRef info = ConfigManager.Deserialize<SpawnDataRef>(File.ReadAllText(filePath));
            if (spawnRefs.TryGetValue(filePath, out SpawnDataRef data))
            {
                if (map.TryGetValue(data, out SpawnSystem.SpawnData? spawnDat))
                {
                    spawnDat.SetFieldsFrom(info);
                }

                data.SetFieldsFrom(info);
            }
            else
            {
                SpawnSystem.SpawnData spawnDat = info;
                map[info] = spawnDat;
                SpawnList.m_spawners.Add(spawnDat);
                spawnRefs[filePath] = info;
            }

            UpdateSync();
            
            MonsterDBPlugin.LogInfo($"Updated Spawn Data: {fileName}");
        }
        catch
        {
            MonsterDBPlugin.LogError($"Failed to deserialize spawn data file: {fileName}");
        }
    }

    public static void Create(GameObject prefab)
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
        string filePath = Path.Combine(FolderPath, $"{prefab.name}.yml");
        if (File.Exists(filePath)) return;
        if (!prefab.GetComponent<Character>()) return;
        SpawnDataRef spawnRef = new SpawnDataRef();
        spawnRef.m_name = $"MDB {prefab.name} Spawn Data";
        spawnRef.m_prefab = prefab.name;
        string text = ConfigManager.Serialize(spawnRef);
        File.WriteAllText(filePath, text);
        MonsterDBPlugin.LogInfo($"Saved spawn data file: {filePath}");
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