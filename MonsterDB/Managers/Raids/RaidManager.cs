using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace MonsterDB;

public static class RaidManager
{
    public const string FolderName = "Raids";
    private static readonly string FolderPath;
    private static Dictionary<string, RandomEventRef> raids;
    private static readonly Dictionary<string, bool> defaultEnable;
    private static readonly ConfigEntry<Toggle> disableAll;
    private static readonly CustomSyncedValue<string> sync;

    static RaidManager()
    {
        raids = new Dictionary<string, RandomEventRef>();
        defaultEnable = new Dictionary<string, bool>();
        FolderPath = Path.Combine(ConfigManager.DirectoryPath, FolderName);
        sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.Raids", "");
        sync.ValueChanged += OnSyncChange;
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

        Read();

        disableAll = ConfigManager.config("Raids", "Disable All", Toggle.Off, "If on, all raids are disabled");
        disableAll.SettingChanged += OnDisableAllChanged;
    }

    [Obsolete]
    public static void WriteAllRaidYML(Terminal.ConsoleEventArgs args)
    {
        if (!RandEventSystem.m_instance)
        {
            args.Context.LogWarning("Random Event System not active");
            return;
        }

        List<RandomEvent> list = RandEventSystem.instance.m_events;
        for (int i = 0; i < list.Count; ++i)
        {
            RandomEvent? raid = list[i];
            Write(raid);
        }
    }

    public static void WriteRaidYML(Terminal context, string input)
    {
        if (!RandEventSystem.m_instance)
        {
            context.LogWarning("Random Event System not active");
            return;
        }
        
        if (string.IsNullOrEmpty(input))
        {
            context.LogWarning("Invalid parameters");
            return;
        }
        List<RandomEvent> list = RandEventSystem.instance.m_events;
            
        RandomEvent? raid = list.FirstOrDefault(x => string.Equals(x.m_name, input, StringComparison.CurrentCultureIgnoreCase));
        if (raid == null)
        {
            context.LogWarning($"Failed to find raid: {input}");
            return;
        }

        Write(raid);
    }

    [Obsolete]
    public static void WriteRaidYML(Terminal.ConsoleEventArgs args)
    {
        if (!RandEventSystem.m_instance)
        {
            args.Context.LogWarning("Random Event System not active");
            return;
        }
        
        string name = args.GetStringFrom(2);
        if (string.IsNullOrEmpty(name))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }
        List<RandomEvent> list = RandEventSystem.instance.m_events;
            
        RandomEvent? raid = list.FirstOrDefault(x => string.Equals(x.m_name, name, StringComparison.CurrentCultureIgnoreCase));
        if (raid == null)
        {
            args.Context.LogWarning($"Failed to find raid: {name}");
            return;
        }

        Write(raid);
    }

    [Obsolete]
    public static List<string> GetRaidOptions(int i, string word) => i switch
    {
        2 => GetRaidNames(),
        _ => new List<string>()
    };

    public static List<string> GetRaidNames() => RandEventSystem.instance
        ? RandEventSystem.instance.m_events.Select(x => x.m_name).ToList()
        : new List<string>();

    [Obsolete]
    public static void UpdateRaid(Terminal.ConsoleEventArgs args)
    {
        Read();
        Update();
        args.Context.AddString("Updated all raids");
    }
    
    public static void Start()
    {
        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations)),
            postfix: new HarmonyMethod(typeof(RaidManager), nameof(Patch_ZoneSystem_SetupLocations)));
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            UpdateSync();
        }
    }

    public static void Reset()
    {
        raids.Clear();
        Read();
    }

    private static void Write(RandomEvent raid)
    {
        RandomEventRef reference = new RandomEventRef(raid);
        string text = ConfigManager.Serialize(reference);
        string? name = raid.m_name;
        if (string.IsNullOrEmpty(name)) name = "Untitled";
        string fileName = name + ".yml";
        string filePath = Path.Combine(FolderPath, fileName);
        File.WriteAllText(filePath, text);
    }

    public static void Read()
    {
        string[] files = Directory.GetFiles(FolderPath, "*.yml");
        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            string text = File.ReadAllText(filePath);
            try
            {
                RandomEventRef data = ConfigManager.Deserialize<RandomEventRef>(text);
                if (string.IsNullOrEmpty(data.m_name)) continue;
                raids[data.m_name] = data;
            }
            catch
            {
                MonsterDBPlugin.LogWarning($"Failed to deserialize raid: {Path.GetFileName(filePath)}");
            }
        }
    }

    public static void Update()
    {
        if (!RandEventSystem.instance) return;
        
        int enabledRaids = 0;
        int disabledRaids = 0;
        List<string> updatedRaids = new List<string>();
        for (int i = 0; i < RandEventSystem.instance.m_events.Count; ++i)
        {
            RandomEvent? raid = RandEventSystem.instance.m_events[i];
            if (!defaultEnable.ContainsKey(raid.m_name))
            {
                defaultEnable.Add(raid.m_name, raid.m_enabled);
            }
            raid.m_enabled &= disableAll.Value is Toggle.Off;
            if (raids.TryGetValue(raid.m_name, out RandomEventRef reference))
            {
                reference.UpdateFields(raid, raid.m_name, true);
                updatedRaids.Add(raid.m_name);
            }

            if (raid.m_enabled) ++enabledRaids;
            else ++disabledRaids;
        }

        foreach (KeyValuePair<string, RandomEventRef> kvp in raids)
        {
            if (updatedRaids.Contains(kvp.Key)) continue;
            RandomEvent newRaid = new RandomEvent();
            kvp.Value.UpdateFields(newRaid, kvp.Key, true);
            newRaid.m_enabled &= disableAll.Value is Toggle.Off;
            RandEventSystem.instance.m_events.Add(newRaid);
            if (newRaid.m_enabled) ++enabledRaids;
            else ++disabledRaids;
        }

        StringBuilder sb = new();
        sb.Append($"RaidManager: updated {raids.Count}, ");
        if (enabledRaids > 0) sb.Append($"enabled {enabledRaids}, ");
        if (disabledRaids > 0) sb.Append($"disabled {disabledRaids}, ");
        sb.Append($"(total: {RandEventSystem.instance.m_events.Count})");
        MonsterDBPlugin.LogInfo(sb.ToString());
    }

    private static void UpdateSync()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        sync.Value = ConfigManager.Serialize(raids);
    }

    private static void OnSyncChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(sync.Value)) return;
        try
        {
            raids = ConfigManager.Deserialize<Dictionary<string, RandomEventRef>>(sync.Value);
            Update();
        }
        catch
        {
            MonsterDBPlugin.LogWarning("Failed to deserialize server raids");
        }
    }

    private static void OnDisableAllChanged(object sender, EventArgs args)
    {
        int enabledRaids = 0;
        int disabledRaids = 0;
        for (int i = 0; i < RandEventSystem.instance.m_events.Count; ++i)
        {
            RandomEvent? raid = RandEventSystem.instance.m_events[i];
            if (!defaultEnable.TryGetValue(raid.m_name, out bool enabled))
            {
                defaultEnable[raid.m_name] = raid.m_enabled;
                enabled = raid.m_enabled;
            }
            raid.m_enabled = enabled && disableAll.Value is Toggle.Off;
            if (raid.m_enabled) ++enabledRaids;
            else ++disabledRaids;
        }
        
        if (enabledRaids > 0) MonsterDBPlugin.LogInfo($"Enabled {enabledRaids} raids");
        if (disabledRaids > 0) MonsterDBPlugin.LogInfo($"Disabled {disabledRaids} raids");
    }
    
    private static void Patch_ZoneSystem_SetupLocations()
    {
        Update();
    }
}