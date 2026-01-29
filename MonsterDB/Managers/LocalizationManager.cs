using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;

namespace MonsterDB;

public static class LocalizationManager
{
    private const string FolderName = "Localizations";
    private static readonly string FolderPath;
    private static Dictionary<string, string[]> localizations;
    private static readonly CustomSyncedValue<string> sync;
    private static readonly ConfigEntry<Toggle> _fileWatcherEnabled;
    
    static LocalizationManager()
    {
        FolderPath = Path.Combine(ConfigManager.DirectoryPath, FolderName);
        localizations = new Dictionary<string, string[]>();
        sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "MDB.ServerSync.Localization", "");
        sync.ValueChanged += OnSyncChange;
        _fileWatcherEnabled = ConfigManager.config("File Watcher", FolderName, Toggle.On,
            $"If on, YML files under {FolderName} folder will trigger update on changed, created, or renamed", false);
        
        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(Localization), nameof(Localization.LoadCSV)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(LocalizationManager),
                nameof(Patch_Localization_LoadCSV))));
    }
    
    private static bool IsFileWatcherEnabled() => _fileWatcherEnabled.Value is Toggle.On;

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
                "enemy_human: Human",
                "hud_ride: Ride",
                "hud_procreate_pregnant: Expecting",
                "hud_procreate_bonding: Bonding",
                "hud_growup_maturing: Maturing"
            };
            File.WriteAllLines(filePath, defaultLines);
            localizations["English"] = defaultLines.ToArray();
        }
        else
        {
            for (int i = 0; i < files.Length; ++i)
            {
                string filePath = files[i];
                string? fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] extraLines = File.ReadAllLines(filePath);
                List<string> lines = new();
                if (localizations.TryGetValue(fileName, out string[] translations))
                {
                    lines.AddRange(translations);
                    lines.AddRange(extraLines);
                }
                else
                {
                    lines.AddRange(extraLines);
                }
                localizations[fileName] = lines.ToArray();
                MonsterDBPlugin.LogInfo($"Loaded {fileName} localizations");
            }
        }
    }

    public static void Register(string filePath)
    {
        string? fileName = Path.GetFileNameWithoutExtension(filePath);
        string[] parts = fileName.Split('.');
        if (parts.Length != 2) return;
        string language = parts[1];
        string[] extraLines = File.ReadAllLines(filePath);
        List<string> lines = new();
        if (localizations.TryGetValue(language, out string[] translations))
        {
            lines.AddRange(translations);
            lines.AddRange(extraLines);
        }
        else
        {
            lines.AddRange(extraLines);
        }
        localizations[language] = lines.ToArray();
    }

    public static void Init(ZNet net)
    {
        if (net.IsServer())
        {
            SetupFileWatcher();
            UpdateSync();
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
        ParseLines(__instance, lines);
    }

    private static void ParseLines(Localization instance, string[] lines)
    {
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (line.StartsWith("#") || string.IsNullOrEmpty(line)) continue;
            string[] parts = line.Split(':');
            if (parts.Length < 2) continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();
            value = instance.StripCitations(value);

            instance.AddWord(key, value);
        }
    }
    
    public static void UpdateWords(string filePath)
    {
        string? fileName = Path.GetFileNameWithoutExtension(filePath);
        string[] parts = fileName.Split('.');
        if (parts.Length != 2) return;
        string language = parts[1];
        string[] extraLines = File.ReadAllLines(filePath);
        Dictionary<string, string> lines = new();

        if (localizations.TryGetValue(language, out string[] translations))
        {
            for (int i = 0; i < translations.Length; ++i)
            {
                string line = translations[i];
                if (string.IsNullOrEmpty(line) || line.StartsWith("$")) continue;
                var lineParts = line.Split(':');
                if (lineParts.Length < 2) continue;
                var key =  lineParts[0].Trim();
                var value =  lineParts[1].Trim();
                lines[key] = value;
            }
        }

        for (int i = 0; i < extraLines.Length; ++i)
        {
            string line = extraLines[i];
            if (string.IsNullOrEmpty(line) || line.StartsWith("$")) continue;
            var lineParts = line.Split(':');
            if (lineParts.Length < 2) continue;
            var key =  lineParts[0].Trim();
            var value =  lineParts[1].Trim();
            lines[key] = value;
        }
        
        string[] newLines = lines.Select(f => string.Join(":", f.Key, f.Value)).ToArray();
        
        localizations[language] = newLines;
        
        Update();
    }

    public static void AddWords(string language, Dictionary<string, string> translations)
    {
        Dictionary<string, string> lines = new();
        if (localizations.TryGetValue(language, out string[] localization))
        {
            for (int i = 0; i < localization.Length; ++i)
            {
                string line = localization[i];
                if (string.IsNullOrEmpty(line) || line.StartsWith("$")) continue;
                string[] lineParts = line.Split(':');
                if (lineParts.Length < 2) continue;
                string key =  lineParts[0].Trim();
                string value =  lineParts[1].Trim();
                lines[key] = value;
            }
        }

        foreach (var kvp in translations)
        {
            lines[kvp.Key] = kvp.Value;
        }
        
        string[] newLines = lines.Select(f => string.Join(":", f.Key, f.Value)).ToArray();
        localizations[language] = newLines;
    }

    public static void AddWord(string language, string key, string value, bool replace = true)
    {
        if (!localizations.TryGetValue(language, out string[] translations))
        {
            localizations[language] = new[] { string.Join(":", key, value) };
        }
        else
        {
            Dictionary<string, string> words = new();
            for (int i = 0; i < translations.Length; ++i)
            {
                string line = translations[i];
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                string[] lineParts = line.Split(':');
                if (lineParts.Length < 2) continue;
                string lKey = lineParts[0].Trim();
                string lValue = lineParts[1].Trim();
                words[lKey] = lValue;
            }

            if (replace)
            {
                words[key] = value;
            }
            else
            {
                if (!words.ContainsKey(key))
                {
                    words.Add(key, value);
                }
            }
            string[] newLines =  words.Select(f => string.Join(":", f.Key, f.Value)).ToArray();
            localizations[language] = newLines;
        }
        Update();
    }

    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!IsFileWatcherEnabled()) return;
        if (!ZNet.instance || !ZNet.instance.IsServer() || Localization.instance == null) return;
        
        string? language = Path.GetFileNameWithoutExtension(e.FullPath);
        string[] lines = File.ReadAllLines(e.FullPath);
        
        Dictionary<string, string> dict = new();

        if (localizations.TryGetValue(language, out string[] translations))
        {
            for (int i = 0; i < translations.Length; ++i)
            {
                string line = translations[i];
                if (string.IsNullOrEmpty(line) || line.StartsWith("$")) continue;
                string[] lineParts = line.Split(':');
                if (lineParts.Length < 2) continue;
                string key =  lineParts[0].Trim();
                string value =  lineParts[1].Trim();
                dict[key] = value;
            }
        }
        
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line) || line.StartsWith("$")) continue;
            string[] lineParts = line.Split(':');
            if (lineParts.Length < 2) continue;
            string key =  lineParts[0].Trim();
            string value =  lineParts[1].Trim();
            dict[key] = value;
        }
        
        string[] newLines = dict.Select(f => string.Join(":", f.Key, f.Value)).ToArray();
        
        localizations[language] = newLines;
        Update();
    }

    private static void Update()
    {
        UpdateSync();
        if (Localization.instance == null) return;
        string lang = Localization.instance.GetSelectedLanguage();
        if (!localizations.TryGetValue(lang, out string[] translations)) return;
        ParseLines(Localization.instance, translations);
        Localization.instance.m_cache.EvictAll();
        MonsterDBPlugin.LogDebug($"Updated {lang} localizations");
    }

    private static void OnSyncChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(sync.Value)) return;

        try
        {
            Dictionary<string, string[]> translations = ConfigManager.Deserialize<Dictionary<string, string[]>>(sync.Value);
            localizations = translations;
            Update();
        }
        catch
        {
            MonsterDBPlugin.LogWarning("Failed to deserialize server localization files");
        }
    }

    private static void UpdateSync()
    {
        if (ZNet.instance && ZNet.instance.IsServer())
        {
            sync.Value = ConfigManager.Serialize(localizations);
        }
    }
}