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
        
        Command reload = new Command("reload", "reloads all files in import folder", _ =>
        {
            ResetAll();
            loadList.Clear();
            SpawnManager.Clear();
            CloneManager.Clear();
            FileManager.Start();
            LoadClones();
            Load();
            return true;
        }, adminOnly: true);

        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(Game), nameof(Game.Logout)),
            new HarmonyMethod(AccessTools.Method(typeof(LoadManager), nameof(Patch_Game_Logout))));
        harmony.Patch(AccessTools.Method(typeof(FejdStartup), nameof(FejdStartup.OnStartGame)),
            new HarmonyMethod(AccessTools.Method(typeof(FileManager), nameof(FileManager.Start))));

    }

    private static void Patch_Game_Logout()
    {
        files = new BaseAggregate();
        ResetAll();
        loadList.Clear();
        SpawnManager.Clear();
        CloneManager.Clear();
        FileManager.started = false;
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
        sync.Value = ConfigManager.Serialize(files);
    }

    private static void ResetAll()
    {
        if (loadList.Count <= 0 || originals.Count <= 0) return;
        
        MonsterDBPlugin.LogInfo("Starting reset");
        resetting = true;

        foreach (Header? data in loadList)
        {
            if (!originals.TryGetValue(data.Prefab, out Header? original)) continue;
            
            data.CopyFields(original);
            data.Update();
        }
        resetting = false;
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
                ResetAll();
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
                        ItemManager.TryClone(prefab, data.Prefab, out _, false);
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
                }
            }
        }

        int count = players + humanoids + characters + eggs + items + fish + projectiles + ragdoll + spawnAbilities + visual;

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
                    
            }
        }

        int count = characters + humanoids + players + eggs + items + fish + projectiles + ragdoll + spawnAbilities +
                    visual;
        
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