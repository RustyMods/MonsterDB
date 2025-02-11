﻿using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using MonsterDB.Solution;
using ServerSync;
using UnityEngine;

namespace MonsterDB
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class MonsterDBPlugin : BaseUnityPlugin
    {
        internal const string ModName = "MonsterDB";
        internal const string ModVersion = "0.1.4";
        internal const string Author = "RustyMods";
        private const string ModGUID = Author + "." + ModName;
        private static readonly string ConfigFileName = ModGUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource MonsterDBLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private enum Toggle { On = 1, Off = 0 }

        public static GameObject m_root = null!;
        public static MonsterDBPlugin m_plugin = null!;

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        private static ConfigEntry<Toggle> _shareTextures = null!;
        private static ConfigEntry<Toggle> _useRandomNames = null!;
        private static ConfigEntry<Toggle> _autoUpdate = null!;
        public static bool ShareTextures() => _shareTextures.Value is Toggle.On;
        public static bool UseNames() => _useRandomNames.Value is Toggle.On;
        public static bool AutoUpdate() => _autoUpdate.Value is Toggle.On;
        private void InitConfigs()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            _shareTextures = config("2 - Settings", "Server Sync Textures", Toggle.Off, "If on, clients can download textures from server, experimental");
            _useRandomNames = config("2 - Settings", "Use Viking Names", Toggle.Off, "If on, any creatures based on Human, will use random names");
            _autoUpdate = config("2 - Settings", "Auto Update", Toggle.On, "If on, any changes made to files will automatically update creature");
        }
        public void Awake()
        {
            m_plugin = this;
            m_root = new GameObject("root");
            DontDestroyOnLoad(m_root);
            m_root.SetActive(false);
            InitConfigs();
            Solution.Tutorial.Write();
            SpawnMan.Setup();
            TextureManager.ReadLocalTextures();
            CreatureManager.Setup();
            AudioManager.Setup();
            Initialization.ReadLocalFiles();
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy() => Config.Save();
        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                MonsterDBLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                MonsterDBLogger.LogError($"There was an issue loading your {ConfigFileName}");
                MonsterDBLogger.LogError("Please check your config entries for spelling and format!");
            }
        }
        
        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order;
            [UsedImplicitly] public bool? Browsable;
            [UsedImplicitly] public string? Category;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer;
        }
    }
}