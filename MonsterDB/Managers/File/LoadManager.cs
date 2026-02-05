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
        output = null;
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
        Header? target = loadList.Find(x => x.Prefab == prefabName);
        if (target == null) return false;
        if (!TryGetOriginal(prefabName, out T result)) return false;
        target.CopyFields(result);
        target.Update();
        return true;
    }

    public static void UpdateSync()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        sync.Value = ConfigManager.Serialize(files);
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
        int characters = 0;
        int humanoids = 0;
        int players = 0;
        int eggs = 0;
        int items = 0;
        int fish = 0;
        int projectiles = 0;
        int ragdoll = 0;
        int spawnAbilities = 0;
        int visual = 0;
        int spawners = 0;
        int spawnAreas = 0;
        
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
                        ++items;
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
                        ++fish;
                        FishManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.Projectile:
                        ++projectiles;
                        ProjectileManager.TryClone(prefab, data.Prefab, out _, false);
                        break;
                    case BaseType.Ragdoll:
                        ++ragdoll;
                        RagdollManager.TryClone(prefab, data.Prefab, out _, false);
                        break;
                    case BaseType.SpawnAbility:
                        ++spawnAbilities;
                        SpawnAbilityManager.TryClone(prefab, data.Prefab, out _, false);
                        break;
                    case BaseType.Visual:
                        ++visual;
                        VisualManager.TryClone(prefab, data.Prefab, out _, false);
                        break;
                    case BaseType.CreatureSpawner:
                        ++spawners;
                        CreatureSpawnerManager.Clone(prefab, data.Prefab, false);
                        break;
                    case BaseType.SpawnArea:
                        ++spawnAreas;
                        SpawnAreaManager.Clone(prefab, data.Prefab, false);
                        break;
                }
            }
        }

        int count = players + humanoids + characters + eggs + items + fish + projectiles + ragdoll + spawnAbilities + visual + spawners + spawnAreas;

        StringBuilder sb = new();
        sb.Append("Loading clones: ");
        if (characters > 0) sb.Append($"{characters} characters, ");
        if (humanoids > 0) sb.Append($"{humanoids} humanoids, ");
        if (players > 0) sb.Append($"{players} humans, ");
        if (eggs > 0) sb.Append($"{eggs} eggs, ");
        if (items > 0) sb.Append($"{items} items, ");
        if (fish > 0) sb.Append($"{fish} fishes, ");
        if (projectiles > 0) sb.Append($"{projectiles} projectiles, ");
        if (ragdoll > 0) sb.Append($"{ragdoll} ragdolls, ");
        if (spawnAbilities > 0) sb.Append($"{spawnAbilities} abilities, ");
        if (visual > 0) sb.Append($"{visual} prefabs, ");
        if (spawners > 0) sb.Append($"{spawners} creature spawners, ");
        if (spawnAreas > 0) sb.Append($"{spawnAreas} spawn areas, ");
        sb.Append($"(total: {count})");
        
        MonsterDBPlugin.LogInfo(sb.ToString());
    }
    
    private static void Load()
    {
        List<Header> ordered = loadList
            .OrderBy(x => x.Type is not BaseType.SpawnAbility)
            .ThenBy(x => x.Type is not BaseType.Projectile)
            .ThenBy(x => x.Type is not BaseType.Item)
            .ToList();
        
        int characters = 0;
        int humanoids = 0;
        int players = 0;
        int eggs = 0;
        int items = 0;
        int fish = 0;
        int projectiles = 0;
        int ragdoll = 0;
        int spawnAbilities = 0;
        int visual = 0;
        int spawners = 0;
        int spawnAreas = 0;
        
        for (int i = 0; i < ordered.Count; ++i)
        {
            Header data = ordered[i];
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
                case BaseType.Projectile:
                    ++projectiles;
                    break;
                case BaseType.Ragdoll:
                    ++ragdoll;
                    break;
                case BaseType.SpawnAbility:
                    ++spawnAbilities;
                    break;
                case BaseType.Visual:
                    ++visual;
                    break;
                case BaseType.CreatureSpawner:
                    ++spawners;
                    break;
                case BaseType.SpawnArea:
                    ++spawnAreas;
                    break;
            }
        }

        int count = characters + humanoids + players + eggs + items + fish + projectiles + ragdoll + spawnAbilities +
                    visual + spawners + spawnAreas;
        
        StringBuilder sb = new();
        sb.Append("Modified: ");
        if (characters > 0) sb.Append($"{characters} characters, ");
        if (humanoids > 0) sb.Append($"{humanoids} humanoids, ");
        if (players > 0) sb.Append($"{players} humans, ");
        if (eggs > 0) sb.Append($"{eggs} eggs, ");
        if (items > 0) sb.Append($"{items} items, ");
        if (fish > 0) sb.Append($"{fish} fishes, ");
        if (projectiles > 0) sb.Append($"{projectiles} projectiles, ");
        if (ragdoll > 0) sb.Append($"{ragdoll} ragdolls, ");
        if (spawnAbilities > 0) sb.Append($"{spawnAbilities} abilities, ");
        if (visual > 0) sb.Append($"{visual} prefabs, ");
        if (spawners > 0) sb.Append($"{spawners} creature spawners, ");
        if (spawnAreas > 0) sb.Append($"{spawnAreas} spawn areas, ");
        sb.Append($"(total: {count})");
        
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