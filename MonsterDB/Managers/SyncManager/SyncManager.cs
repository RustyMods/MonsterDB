using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;
using UnityEngine;

namespace MonsterDB;

public static class SyncManager
{
    public static readonly Dictionary<string, Base> originals;
    public static readonly List<Base> loadList;
    public static Dictionary<string, string> rawFiles;
    private static readonly CustomSyncedValue<string> sync;
    
    static SyncManager()
    {
        originals = new Dictionary<string, Base>();
        loadList = new List<Base>();
        rawFiles = new Dictionary<string, string>();
        sync =  new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.Files", "");
        sync.ValueChanged += OnSyncChange;
    }
    
    public static T? GetOriginal<T>(string prefabName) where T : Base =>
        originals.TryGetValue(prefabName, out Base? baseValue) ? baseValue as T : null;

    private static void SetupFileWatcher()
    {
        FileSystemWatcher watcher = new(CreatureManager.ModifiedFolder, "*.yml");
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
        string filePath = e.FullPath;
        CreatureManager.Read(filePath);
    }

    public static void UpdateSync()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        sync.Value = ConfigManager.Serialize(rawFiles);
    }

    private static void Reset()
    {
        foreach (Base? og in originals.Values)
        {
            og.Update();
        }
    }
    
    private static void OnSyncChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(sync.Value)) return;
        try
        {
            rawFiles = ConfigManager.Deserialize<Dictionary<string, string>>(sync.Value);
            MonsterDBPlugin.LogInfo($"Received {rawFiles.Count} files from server, reloading");
        }
        catch
        {
            MonsterDBPlugin.LogError("Server Sync Data invalid");
            return;
        }
        
        loadList.Clear();
        Reset();
        foreach (KeyValuePair<string, string> file in rawFiles)
        {
            try
            {
                Header header = ConfigManager.Deserialize<Header>(file.Value);
                switch (header.Type)
                {
                    case CreatureType.Character:
                        CharacterCreature character = ConfigManager.Deserialize<CharacterCreature>(file.Value);
                        loadList.Add(character);
                        break;
                    case CreatureType.Humanoid:
                        HumanoidCreature humanoid = ConfigManager.Deserialize<HumanoidCreature>(file.Value);
                        loadList.Add(humanoid);
                        break;
                    case CreatureType.Human:
                        PlayerCreature player = ConfigManager.Deserialize<PlayerCreature>(file.Value);
                        loadList.Add(player);
                        break;
                    case CreatureType.Egg:
                        BaseEgg egg = ConfigManager.Deserialize<BaseEgg>(file.Value);
                        loadList.Add(egg);
                        break;
                }
            }
            catch (Exception ex)
            {
                MonsterDBPlugin.LogError($"Failed to deserialize server prefab data: {file.Key}");
                MonsterDBPlugin.LogDebug(ex.Message);
            }
        }
        LoadClones();
        Load();
    }
    
    private static void LoadClones()
    {
        int characters = 0;
        int humanoids = 0;
        int players = 0;
        int eggs = 0;
        
        for (int i = 0; i < loadList.Count; ++i)
        {
            Base? data = loadList[i];
            if (data.IsCloned)
            {
                GameObject? prefab = PrefabManager.GetPrefab(data.ClonedFrom);
                if (prefab == null) continue;
                switch (data.Type)
                {
                    case CreatureType.Human:
                        ++players;
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case CreatureType.Humanoid:
                        ++humanoids;
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case CreatureType.Character:
                        ++characters;
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case CreatureType.Egg:
                        ++eggs;
                        EggManager.Clone(prefab, data.Prefab, false);
                        break;
                }
            }
        }

        int count = players + humanoids + characters + eggs;
        MonsterDBPlugin.LogInfo($"Loading clones: {characters} characters, {humanoids} humanoids, {players} players, {eggs} eggs (total:{count})");
    }
    
    private static void Load()
    {
        int characters = 0;
        int humanoids = 0;
        int players = 0;
        int eggs = 0;
        for (int i = 0; i < loadList.Count; ++i)
        {
            Base data = loadList[i];
            if (data.Type == CreatureType.None) continue;
            data.Update();
            switch (data.Type)
            {
                case CreatureType.Character:
                    ++characters;
                    break;
                case CreatureType.Human:
                    ++players;
                    break;
                case CreatureType.Humanoid:
                    ++humanoids;
                    break;
                case CreatureType.Egg:
                    ++eggs;
                    break;
            }
        }
        int count = characters + humanoids + players + eggs;
        MonsterDBPlugin.LogInfo($"Modified {characters} characters, {humanoids} humanoids, {players} players, {eggs} eggs (total: {count})");
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            LoadClones();
            Load();
            UpdateSync();
            SetupFileWatcher();
        }
    }
}