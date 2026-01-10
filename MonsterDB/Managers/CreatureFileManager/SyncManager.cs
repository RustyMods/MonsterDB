using System;
using System.Collections.Generic;
using System.Linq;
using ServerSync;
using UnityEngine;

namespace MonsterDB;

public static class SyncManager
{
    public static readonly Dictionary<string, Header> originals;
    public static readonly List<Header> loadList;
    public static Dictionary<string, string> rawFiles;
    private static readonly CustomSyncedValue<string> sync;
    
    static SyncManager()
    {
        originals = new Dictionary<string, Header>();
        loadList = new List<Header>();
        rawFiles = new Dictionary<string, string>();
        sync =  new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.Files", "");
        sync.ValueChanged += OnSyncChange;
    }
    
    public static T? GetOriginal<T>(string prefabName) where T : Header =>
        originals.TryGetValue(prefabName, out Header? baseValue) ? baseValue as T : null;

    public static List<string> GetOriginalKeys<T>() where T : Header => originals
        .Where(x => x.Value is T)
        .Select(x => x.Key)
        .ToList();

    public static void UpdateSync()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        sync.Value = ConfigManager.Serialize(rawFiles);
    }

    private static void Reset()
    {
        foreach (Header? og in originals.Values)
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
                    case BaseType.Character:
                        BaseCharacter character = ConfigManager.Deserialize<BaseCharacter>(file.Value);
                        loadList.Add(character);
                        break;
                    case BaseType.Humanoid:
                        BaseHumanoid humanoid = ConfigManager.Deserialize<BaseHumanoid>(file.Value);
                        loadList.Add(humanoid);
                        break;
                    case BaseType.Human:
                        BaseHuman player = ConfigManager.Deserialize<BaseHuman>(file.Value);
                        loadList.Add(player);
                        break;
                    case BaseType.Egg:
                        BaseEgg egg = ConfigManager.Deserialize<BaseEgg>(file.Value);
                        loadList.Add(egg);
                        break;
                    case BaseType.Item:
                        BaseItem item = ConfigManager.Deserialize<BaseItem>(file.Value);
                        loadList.Add(item);
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
        int items = 0;
        int fish = 0;
        
        for (int i = 0; i < loadList.Count; ++i)
        {
            Header? data = loadList[i];
            if (data.IsCloned)
            {
                GameObject? prefab = PrefabManager.GetPrefab(data.ClonedFrom);
                if (prefab == null) continue;
                switch (data.Type)
                {
                    case BaseType.Human:
                        ++players;
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Humanoid:
                        ++humanoids;
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Character:
                        ++characters;
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Egg:
                        ++eggs;
                        EggManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Item:
                        ++items;
                        ItemManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Fish:
                        ++fish;
                        FishManager.Clone(prefab, data.Prefab, false);
                        break;
                }
            }
        }

        int count = players + humanoids + characters + eggs + items + fish;
        MonsterDBPlugin.LogInfo($"Loading clones: {characters} characters, {humanoids} humanoids, {players} humans, {eggs} eggs, items {items}, fishes {fish} (total:{count})");
    }
    
    private static void Load()
    {
        int characters = 0;
        int humanoids = 0;
        int players = 0;
        int eggs = 0;
        int items = 0;
        int fish = 0;
        for (int i = 0; i < loadList.Count; ++i)
        {
            Header data = loadList[i];
            if (data.Type == BaseType.None) continue;
            data.Update();
            switch (data.Type)
            {
                case BaseType.Character:
                    ++characters;
                    break;
                case BaseType.Human:
                    ++players;
                    break;
                case BaseType.Humanoid:
                    ++humanoids;
                    break;
                case BaseType.Egg:
                    ++eggs;
                    break;
                case BaseType.Item:
                    ++items;
                    break;
                case BaseType.Fish:
                    ++fish;
                    break;
            }
        }
        int count = characters + humanoids + players + eggs + items + fish;
        MonsterDBPlugin.LogInfo($"Modified {characters} characters, {humanoids} humanoids, {players} humans, {eggs} eggs, items {items}, fishes {fish} (total: {count})");
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            LoadClones();
            Load();
            UpdateSync();
            FileManager.SetupFileWatcher();
        }
    }
}