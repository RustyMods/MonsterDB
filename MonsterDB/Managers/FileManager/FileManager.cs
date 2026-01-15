using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace MonsterDB;

public static class FileManager
{
    private const string ExportFolderName = "Export";
    private const string ImportFolderName = "Import";
    public static readonly string ExportFolder;
    public static readonly string ImportFolder;
    private static readonly ConfigEntry<Toggle> _fileWatcherEnabled;
    
    static FileManager()
    {
        ExportFolder = Path.Combine(ConfigManager.DirectoryPath, ExportFolderName);
        ImportFolder = Path.Combine(ConfigManager.DirectoryPath, ImportFolderName);
        _fileWatcherEnabled = ConfigManager.config("File Watcher", ImportFolderName, Toggle.On,
            $"If on, YML files under {ImportFolderName} folder will trigger to update when changed, created or renamed", false);

        if (!Directory.Exists(ExportFolder)) Directory.CreateDirectory(ExportFolder);
        if (!Directory.Exists(ImportFolder)) Directory.CreateDirectory(ImportFolder);
    }

    private static bool IsFileWatcherEnabled() => _fileWatcherEnabled.Value is Toggle.On;
    
    public static List<string> GetModFileNames() => Directory
            .GetFiles(ImportFolder, "*.yml", SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();
    
    public static void Start()
    {
        string[] files =  Directory.GetFiles(ImportFolder, "*.yml", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            string text = File.ReadAllText(filePath);
            try
            {
                Header header = ConfigManager.Deserialize<Header>(text);
                switch (header.Type)
                {
                    case BaseType.Character:
                        BaseCharacter character = ConfigManager.Deserialize<BaseCharacter>(text);
                        SyncManager.loadList.Add(character);
                        SyncManager.rawFiles[character.Prefab] = text;
                        if (character.SpawnData != null)
                        {
                            RegisterSpawnList(character.SpawnData);
                        }
                        break;
                    case BaseType.Humanoid:
                        BaseHumanoid humanoid = ConfigManager.Deserialize<BaseHumanoid>(text);
                        SyncManager.loadList.Add(humanoid);
                        SyncManager.rawFiles[humanoid.Prefab] = text;
                        if (humanoid.SpawnData != null)
                        {
                            RegisterSpawnList(humanoid.SpawnData);
                        }
                        break;
                    case BaseType.Human:
                        BaseHuman player = ConfigManager.Deserialize<BaseHuman>(text);
                        SyncManager.loadList.Add(player);
                        SyncManager.rawFiles[player.Prefab] = text;
                        if (player.SpawnData != null)
                        {
                            RegisterSpawnList(player.SpawnData);
                        }
                        break;
                    case BaseType.Egg:
                        BaseEgg data = ConfigManager.Deserialize<BaseEgg>(text);
                        SyncManager.loadList.Add(data);
                        SyncManager.rawFiles[data.Prefab] = text;
                        break;
                    case BaseType.Item:
                        BaseItem item = ConfigManager.Deserialize<BaseItem>(text);
                        SyncManager.loadList.Add(item);
                        SyncManager.rawFiles[item.Prefab] = text;
                        break;
                    case BaseType.Fish:
                        BaseFish fish = ConfigManager.Deserialize<BaseFish>(text);
                        SyncManager.loadList.Add(fish);
                        SyncManager.rawFiles[fish.Prefab] = text;
                        break;
                    case BaseType.Projectile:
                        BaseProjectile projectile = ConfigManager.Deserialize<BaseProjectile>(text);
                        SyncManager.loadList.Add(projectile);
                        SyncManager.rawFiles[projectile.Prefab] = text;
                        break;
                }

            }
            catch (Exception ex)
            {
                MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
                MonsterDBPlugin.LogDebug(ex.Message);
            }
        }
        MonsterDBPlugin.LogInfo($"Loaded {files.Length} files.");
    }

    private static void RegisterSpawnList(SpawnDataRef[] list)
    {
        for (int i = 0; i < list.Length; ++i)
        {
            SpawnDataRef data = list[i];
            SpawnManager.Add(data);
        }
    }
    
    public static void SetupFileWatcher()
    {
        FileSystemWatcher watcher = new(ImportFolder, "*.yml");
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
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        string filePath = e.FullPath;
        Read(filePath);
    }
    
    public static void Read(string filePath)
    {
        if (!File.Exists(filePath)) return;
        
        string text = File.ReadAllText(filePath);
        try
        {
            Header header = ConfigManager.Deserialize<Header>(text);
            switch (header.Type)
            {
                case BaseType.Humanoid:
                {
                    BaseHumanoid data = ConfigManager.Deserialize<BaseHumanoid>(text);
                    data.Update();
                    SyncManager.rawFiles[data.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                }
                case BaseType.Character:
                {
                    BaseCharacter data = ConfigManager.Deserialize<BaseCharacter>(text);
                    data.Update();
                    SyncManager.rawFiles[data.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                }
                case BaseType.Human:
                {
                    BaseHuman data = ConfigManager.Deserialize<BaseHuman>(text);
                    data.Update();
                    SyncManager.rawFiles[data.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                }
                case BaseType.Egg:
                    BaseEgg reference = ConfigManager.Deserialize<BaseEgg>(text);
                    reference.Update();
                    SyncManager.rawFiles[reference.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Item:
                    BaseItem item = ConfigManager.Deserialize<BaseItem>(text);
                    item.Update();
                    SyncManager.rawFiles[item.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Fish:
                    BaseFish fish = ConfigManager.Deserialize<BaseFish>(text);
                    fish.Update();
                    SyncManager.rawFiles[fish.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Projectile:
                    BaseProjectile projectile = ConfigManager.Deserialize<BaseProjectile>(text);
                    projectile.Update();
                    SyncManager.rawFiles[projectile.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
            }
        }
        catch (Exception ex)
        {
            MonsterDBPlugin.LogWarning($"Failure updating file: {Path.GetFileName(filePath)}");
            MonsterDBPlugin.LogWarning(ex.Message);
            MonsterDBPlugin.LogDebug(ex.StackTrace);
        }
    }
}