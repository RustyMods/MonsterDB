using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using YamlDotNet.Serialization;

namespace MonsterDB;

public enum Toggle { On = 1, Off = 0 }

public static class ConfigManager
{
    private static readonly ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .DisableAliases()
        .WithNamingConvention(StrippedCase.Instance)
        .Build();

    private static readonly IDeserializer deserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties()
        .WithNamingConvention(StrippedCase.Instance)
        .Build();
    
    public static T Deserialize<T>(string data) => deserializer.Deserialize<T>(data);
    public static string Serialize<T>(T obj) => serializer.Serialize(obj);
    
    private static readonly ConfigFile Config;
    public static readonly ConfigSync ConfigSync;
    private static readonly string ConfigFileName;
    private static readonly string ConfigFileFullPath;

    public static readonly string DirectoryPath;
    
    [Flags]
    public enum LogLevel
    {
        None = 0,
        Debug = 1, 
        Warning = 2, 
        Error = 4,
        Info = 8,
        All = Debug | Warning | Error | Info
    }

    private static readonly ConfigEntry<LogLevel> logLevels;

    public static bool ShouldLog(LogLevel type)
    {
        return logLevels.Value.HasFlag(type);
    }
    
    static ConfigManager()
    {
        Config = MonsterDBPlugin.instance.Config;
        ConfigFileName = MonsterDBPlugin.ModGUID + ".cfg";
        ConfigFileFullPath = Path.Combine(Paths.ConfigPath, ConfigFileName);
        ConfigSync = new ConfigSync(MonsterDBPlugin.ModGUID)
        {
            DisplayName = MonsterDBPlugin.ModName,
            CurrentVersion = MonsterDBPlugin.ModVersion,
            MinimumRequiredVersion = MonsterDBPlugin.ModVersion
        };

        DirectoryPath = Path.Combine(Paths.ConfigPath, MonsterDBPlugin.ModName);
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }
        
        ConfigEntry<Toggle> serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
            "If on, the configuration is locked and can be changed by server admins only.");
        _ = ConfigSync.AddLockingConfigEntry(serverConfigLocked);

        logLevels = config("1 - General", "Log Levels", LogLevel.All, "Set log levels", false);
        
        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(ZNet), nameof(ZNet.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(ConfigManager), 
                nameof(Patch_ZNet_Awake))));
    }

    public static void Patch_ZNet_Awake(ZNet __instance)
    {
        SyncManager.Init(__instance);
        SpawnManager.Init(__instance);
        LocalizationManager.Init(__instance);
    }
    
    public static void SetupWatcher()
    {
        FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(ConfigFileFullPath)) return;
        try
        {
            MonsterDBPlugin.LogDebug("ReadConfigValues called");
            Config.Reload();
        }
        catch
        {
            MonsterDBPlugin.LogError($"There was an issue loading your {ConfigFileName}");
            MonsterDBPlugin.LogError("Please check your config entries for spelling and format!");
        }
    }
    
    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        ConfigDescription extendedDescription =
            new(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);
        ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);

        SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    public static ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }
}