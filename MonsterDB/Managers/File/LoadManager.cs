using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace MonsterDB;

public static class LoadManager
{
    public static readonly Dictionary<string, Header> originals;
    public static readonly List<Header> loadList;
    private static readonly CustomSyncedValue<string> sync;
    public static BaseAggregate files;
    public static bool resetting;

    static LoadManager()
    {
        originals = new Dictionary<string, Header>();
        loadList = new List<Header>();
        files = new BaseAggregate();
        sync =  new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.Files", "");
        sync.ValueChanged += OnSyncChange;

        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(Game), nameof(Game.Logout)),
            new HarmonyMethod(AccessTools.Method(typeof(LoadManager), nameof(Patch_Game_Logout))));
        harmony.Patch(AccessTools.Method(typeof(FejdStartup), nameof(FejdStartup.OnStartGame)),
            new HarmonyMethod(AccessTools.Method(typeof(FileManager), nameof(FileManager.Start))));
    }

    public static void ReloadAll(Terminal.ConsoleEventArgs args)
    {
        Reload();
        args.Context.AddString("Reloaded all files");
    }

    private static void Reload()
    {
        ResetAll<Header>();
        loadList.Clear();
        SpawnManager.Clear();
        CloneManager.Clear();
        FileManager.started = false;
        FileManager.Start();
        LoadClones();
        Load();
    }

    private static void Patch_Game_Logout()
    {
        files = new BaseAggregate();
        ResetAll<Header>();
        loadList.Clear();
        SpawnManager.Clear();
        CloneManager.Clear();
        RaidManager.Reset();
        FileManager.started = false;
    }

    public static T? GetOriginal<T>(string prefabName) where T : Header =>
        originals.TryGetValue(prefabName, out Header? baseValue) ? baseValue as T : null;

    public static List<string> GetOriginalKeys<T>() where T : Header => originals
        .Where(x => x.Value is T)
        .Select(x => x.Key)
        .ToList();

    public static List<T> GetOriginals<T>() where T : Header => originals
            .Where(x => x.Value is T)
            .Select(x => x.Value as T)
            .ToList()!;

    public static bool TryGetOriginal<T>(string prefabName, out T output) where T : Header
    {
        output = null!;
        if (!originals.TryGetValue(prefabName, out Header header)) return false;
        if (header is not T result) return false;
        output = result;
        return true;
    }

    public static void ResetAll<T>() where T : Header
    {
        if (loadList.Count <= 0 || originals.Count <= 0) return;
        resetting = true;
        foreach (Header? data in loadList)
        {
            if (data is not T) continue;
            if (!originals.TryGetValue(data.Prefab, out Header? original)) continue;
            data.CopyFields(original);
            data.Update();
        }
        resetting = false;
    }

    public static bool Reset<T>(string prefabName) where T : Header
    {
        if (!TryGetOriginal(prefabName, out T result)) return false;
        Header? target = loadList.Find(x => x.Prefab == prefabName);
        if (target == null) return false;
        target.CopyFields(result);
        target.Update();
        return true;
    }

    public static void UpdateSync()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        sync.Value = ConfigManager.Serialize(files, false);
    }
    
    private static void OnSyncChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(sync.Value)) return;
        try
        {
            BaseAggregate data = ConfigManager.Deserialize<BaseAggregate>(sync.Value);
            if (string.IsNullOrEmpty(data.PrefabToUpdate))
            {
                ResetAll<Header>();
                loadList.Clear();
                loadList.AddRange(data.Load());
                LoadClones();
                Load();
            }
            else
            {
                if (data.GetPrefabToUpdate(out Header header))
                {
                    header.Update();
                }
            }

            if (data.translations != null)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> kvp in data.translations)
                {
                    LocalizationManager.AddWords(kvp.Key, kvp.Value);
                }
            }
        }
        catch
        {
            MonsterDBPlugin.LogWarning("Failed to deserialize server files");
        }
    }
    
    private static void LoadClones()
    {
        Dictionary<BaseType, int> count = new  Dictionary<BaseType, int>();
        
        for (int i = 0; i < loadList.Count; ++i)
        {
            Header? data = loadList[i];
            if (data.IsCloned)
            {
                GameObject? prefab = PrefabManager.GetPrefab(data.ClonedFrom);
                if (prefab == null) continue;
                if (count.ContainsKey(data.Type)) ++count[data.Type];
                else count[data.Type] = 1;
                switch (data.Type)
                {
                    case BaseType.Human:
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Humanoid:
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Character:
                        CreatureManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Egg:
                        if (!prefab.GetComponent<ItemDrop>())
                        {
                            ItemManager.TryCreateItem(prefab, data.Prefab, out _, false);
                        }
                        else
                        {
                            EggManager.Clone(prefab, data.Prefab, false);
                        }
                        break;
                    case BaseType.Item:
                        if (!prefab.GetComponent<ItemDrop>())
                        {
                            ItemManager.TryCreateItem(prefab, data.Prefab, out _, false);
                        }
                        else
                        {
                            ItemManager.TryClone(prefab, data.Prefab, out _, false);
                        }
                        break;
                    case BaseType.Fish:
                        FishManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Projectile:
                        ProjectileManager.TryClone(prefab, data.Prefab, out _, false);
                        break;
                    case BaseType.Ragdoll:
                        RagdollManager.TryClone(prefab, data.Prefab, out _, false);
                        break;
                    case BaseType.SpawnAbility:
                        SpawnAbilityManager.TryClone(prefab, data.Prefab, out _, false);
                        break;
                    case BaseType.Visual:
                        VisualManager.TryClone(prefab, data.Prefab, out _, false);
                        break;
                    case BaseType.CreatureSpawner:
                        CreatureSpawnerManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.SpawnArea:
                        SpawnAreaManager.Clone(prefab, data.Prefab, false);
                        break;
                }
            }
        }
        
        StringBuilder sb = new();
        sb.Append("Loading clones: ");
        foreach (KeyValuePair<BaseType, int> kvp in count)
        {
            sb.Append($"{kvp.Value} {kvp.Key}, ");
        }
        sb.Append($"(total: {count.Sum(x => x.Value)})");
        
        MonsterDBPlugin.LogInfo(sb.ToString());
    }
    
    private static void Load()
    {
        List<Header> ordered = loadList
            .OrderBy(x => x.Type is not BaseType.SpawnAbility)
            .ThenBy(x => x.Type is not BaseType.Projectile)
            .ThenBy(x => x.Type is not BaseType.Item)
            .ToList();

        Dictionary<BaseType, int> count = new Dictionary<BaseType, int>();
        
        for (int i = 0; i < ordered.Count; ++i)
        {
            Header data = ordered[i];
            if (data.Type == BaseType.None) continue;
            data.Update();
            if (count.ContainsKey(data.Type)) ++count[data.Type];
            else count[data.Type] = 1;
        }
        
        StringBuilder sb = new();
        sb.Append("Modified: ");
        foreach (KeyValuePair<BaseType, int> kvp in count)
        {
            sb.Append($"{kvp.Value} {kvp.Key}, ");
        }
        sb.Append($"(total: {count.Sum(x => x.Value)})");
        
        MonsterDBPlugin.LogInfo(sb.ToString());
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            UpdateSync();
            FileManager.SetupFileWatcher();
        }
    }

    [HarmonyPriority(Priority.Last)]
    public static void Start()
    {
        LoadClones();
        Load();
        SpawnManager.Start();
    }
}