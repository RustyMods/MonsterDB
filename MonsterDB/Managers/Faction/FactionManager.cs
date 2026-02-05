using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace MonsterDB;

public static class FactionManager
{
    private static readonly Dictionary<Character.Faction, Faction> customFactions;
    private static readonly Dictionary<string, Character.Faction> factions;
    private const string FileName = "Factions.yml";
    private static readonly string FilePath;
    private static string rawFile = "";
    private static readonly CustomSyncedValue<string> sync;
    private static readonly ConfigEntry<Toggle> _fileWatcherEnabled;

    static FactionManager()
    {
        customFactions = new Dictionary<Character.Faction, Faction>();
        factions = new  Dictionary<string, Character.Faction>();
        FilePath = Path.Combine(ConfigManager.DirectoryPath, FileName);
        sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, $"{MonsterDBPlugin.ModName}.ServerSync.Factions", "");
        sync.ValueChanged += OnSyncChange;
        _fileWatcherEnabled = ConfigManager.config("File Watcher", "Faction File", Toggle.On,
            "If on, Faction.yml changed, renamed or created will trigger update");
        
        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetValues)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_Enum_GetValues))));
        harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetNames)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_Enum_GetNames))));
        harmony.Patch(AccessTools.Method(typeof(BaseAI), nameof(BaseAI.IsEnemy), new Type[]{typeof(Character), typeof(Character)}),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_BaseAI_IsEnemy))));
        harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetName)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_Enum_GetName))));
    }

    private static bool IsFileWatcherEnabled() => _fileWatcherEnabled.Value is Toggle.On;

    public static void Start()
    {
        if (File.Exists(FilePath))
        {
            Read(FilePath);
        }
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            UpdateSync();
            SetupFileWatcher();
        }
    }

    private static void UpdateSync()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        sync.Value = rawFile;
    }

    private static void OnSyncChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(sync.Value)) return;
        try
        {
            List<Faction> data = ConfigManager.Deserialize<List<Faction>>(sync.Value);
            customFactions.Clear();
            factions.Clear();
            foreach (Faction? faction in data)
            {
                faction.Setup();
            }
        }
        catch
        {
            MonsterDBPlugin.LogWarning("Failed to deserialize server factions");
        }
    }

    private static void SetupFileWatcher()
    {
        FileSystemWatcher watcher = new(ConfigManager.DirectoryPath, FileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!IsFileWatcherEnabled()) return;
        Read(FilePath);
    }

    private static void Read(string filePath)
    {
        try
        {
            string text = File.ReadAllText(filePath);
            List<Faction> data = ConfigManager.Deserialize<List<Faction>>(text);
            foreach (Faction? faction in data)
            {
                faction.Setup();
            }
            rawFile = text;
            UpdateSync();
        }
        catch
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize factions: {Path.GetFileName(filePath)}");
        }
    }

    private static bool IsCustom(Character.Faction faction) => customFactions.ContainsKey(faction);

    private static bool Patch_BaseAI_IsEnemy(Character a, Character b, ref bool __result)
    {
        if (!IsCustom(a.m_faction) && !IsCustom(b.m_faction)) return true;
        __result = IsEnemy(a, b);
        return false;
    }

    private static bool IsEnemy(Character a, Character b)
    {
        if (a.m_faction == b.m_faction) return false;

        string aGroup = a.GetGroup();
        string bGroup = b.GetGroup();
        if (aGroup.Length > 0 && aGroup == bGroup) return false;
        
        if (IsCustom(a.m_faction))
        {
            return IsEnemyToCreatures(a, b);
        }

        if (IsCustom(b.m_faction))
        {
            return IsEnemyToCreatures(b, a);
        }

        return true;
    }

    private static bool IsEnemyToCreatures(Character custom, Character other)
    {
        if (!customFactions.TryGetValue(custom.m_faction, out Faction faction)) return true;
        return faction.IsEnemy(custom, other);
    }
    
    private static void Patch_Enum_GetValues(Type enumType, ref Array __result)
    {
        if (enumType != typeof(Character.Faction)) return;
        if (factions.Count == 0) return;
        Character.Faction[] f = new Character.Faction[__result.Length + factions.Count];
        __result.CopyTo(f, 0);
        factions.Values.CopyTo(f, __result.Length);
        __result = f;
    }

    private static void Patch_Enum_GetNames(Type enumType, ref string[] __result)
    {
        if (enumType != typeof(Character.Faction)) return;
        if (factions.Count == 0) return;
        __result = __result.AddRangeToArray(factions.Keys.ToArray());
    }
    private static bool Patch_Enum_GetName(Type enumType, object value, ref string __result)
    {
        if (enumType != typeof(Character.Faction)) return true;
        if (customFactions.TryGetValue((Character.Faction)value, out Faction data))
        {
            __result = data.name;
            return false;
        }
        return true;
    }
    
    public static Character.Faction GetFaction(string name, Faction? data = null)
    {
        if (Enum.TryParse(name, true, out Character.Faction faction))
        {
            return faction;
        }
        // If already registered, return faction
        if (factions.TryGetValue(name, out faction))
        {
            return faction;
        }
        // If external factions already uses name, return external faction
        Dictionary<Character.Faction, string> map = GetFactionMap();
        foreach (KeyValuePair<Character.Faction, string> kvp in map)
        {
            if (kvp.Value == name)
            {
                faction = kvp.Key;
                factions[name] = faction;
                return faction;
            }
        }
        
        // Create new faction
        int hash = name.GetStableHashCode();
        faction = (Character.Faction)hash;
        factions[name] = faction;

        if (data == null && !customFactions.TryGetValue(faction, out data))
        {
            data = new Faction()
            {
                name = name,
            };
        }
        // Register data
        customFactions[faction] = data;
        
        return faction;
    }

    private static Dictionary<Character.Faction, string> GetFactionMap()
    {
        Array values = Enum.GetValues(typeof(Character.Faction));
        string[] names = Enum.GetNames(typeof(Character.Faction));
        Dictionary<Character.Faction, string> map = new();
        for (int i = 0; i < values.Length; ++i)
        {
            map[(Character.Faction)values.GetValue(i)] = names[i];
        }
        return map;
    }
}