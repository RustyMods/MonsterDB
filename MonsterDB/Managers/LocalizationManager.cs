using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using ServerSync;

namespace MonsterDB;

public static class LocalizationManager
{
    private const string FolderName = "Localizations";
    private static readonly string FolderPath;
    private static readonly Dictionary<string, string[]> localizations;
    private static readonly CustomSyncedValue<string> sync;
    
    static LocalizationManager()
    {
        FolderPath = Path.Combine(ConfigManager.DirectoryPath, FolderName);
        localizations = new Dictionary<string, string[]>();
        sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.Localization", "");
        sync.ValueChanged += OnSyncChange;
        
        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(Localization), nameof(Localization.LoadCSV)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(LocalizationManager),
                nameof(Patch_Localization_LoadCSV))));
    }

    public static void Start()
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

        string[] files = Directory.GetFiles(FolderPath, "*.yml", SearchOption.AllDirectories);
        if (files.Length <= 0)
        {
            string filePath = Path.Combine(FolderPath, "English.yml");
            List<string> defaultLines = new()
            {
                "# Insert key: value translation pairs",
                "# Name of file without extension dictates language (e.g. English.yml)",
                "# Any lines that start with # will be ignored",
                "enemy_human: Human"
            };
            File.WriteAllLines(filePath, defaultLines);
        }
        else
        {
            for (int i = 0; i < files.Length; ++i)
            {
                string filePath = files[i];
                string? fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] lines = File.ReadAllLines(filePath);
                if (localizations.ContainsKey(fileName)) continue;
                localizations.Add(fileName, lines);
            }
        }
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            SetupFileWatcher();
            sync.Value = ConfigManager.Serialize(localizations);
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

    private static void Patch_Localization_LoadCSV(Localization __instance, string language)
    {
        if (!localizations.TryGetValue(language, out string[] lines)) return;
        Parse(__instance, lines);
    }

    private static void Parse(Localization instance, string[] lines)
    {
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;
            string[] parts = line.Split(':');
            if (parts.Length != 2) continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();
            value = instance.StripCitations(value);

            instance.AddWord(key, value);
        }
    }

    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer() || Localization.m_instance == null) return;
        
        string? language = Path.GetFileNameWithoutExtension(e.FullPath);
        string[] lines = File.ReadAllLines(e.FullPath);
        localizations[language] = lines;
        sync.Value = ConfigManager.Serialize(localizations);

        Update();
    }

    private static void Update()
    {
        string lang = Localization.m_instance.GetSelectedLanguage();
        if (!localizations.TryGetValue(lang, out string[] translations)) return;
        Parse(Localization.m_instance, translations);
        
        Localization.m_instance.m_cache.EvictAll();
    }

    private static void OnSyncChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer() || Localization.m_instance == null) return;
        if (string.IsNullOrEmpty(sync.Value)) return;

        try
        {
            Dictionary<string, string[]> translations = ConfigManager.Deserialize<Dictionary<string, string[]>>(sync.Value);
            foreach (KeyValuePair<string, string[]> kvp in translations)
            {
                localizations[kvp.Key] = kvp.Value;
            }
            Update();
        }
        catch
        {
            MonsterDBPlugin.LogWarning("Failed to deserialize server localization files");
        }
    }
}