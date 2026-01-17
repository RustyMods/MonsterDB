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

        Command export = new Command("export_all", "exports all files in import folder into a single file", _ =>
        {
            Export(ImportFolder);
            return true;
        });
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

            string? fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName.StartsWith("translations."))
            {
                LocalizationManager.Register(filePath);
                continue;
            }
            
            string text = File.ReadAllText(filePath);
            try
            {
                Header header = ConfigManager.Deserialize<Header>(text);
                switch (header.Type)
                {
                    case BaseType.Character:
                        BaseCharacter character = ConfigManager.Deserialize<BaseCharacter>(text);
                        SyncManager.loadList.Add(character);
                        RegisterSpawnList(character.SpawnData);
                        SyncManager.files.Add(character);
                        break;
                    case BaseType.Humanoid:
                        BaseHumanoid humanoid = ConfigManager.Deserialize<BaseHumanoid>(text);
                        SyncManager.loadList.Add(humanoid);
                        RegisterSpawnList(humanoid.SpawnData);
                        SyncManager.files.Add(humanoid);
                        break;
                    case BaseType.Human:
                        BaseHuman player = ConfigManager.Deserialize<BaseHuman>(text);
                        SyncManager.loadList.Add(player);
                        RegisterSpawnList(player.SpawnData);
                        SyncManager.files.Add(player);
                        break;
                    case BaseType.Egg:
                        BaseEgg data = ConfigManager.Deserialize<BaseEgg>(text);
                        SyncManager.loadList.Add(data);
                        SyncManager.files.Add(data);
                        break;
                    case BaseType.Item:
                        BaseItem item = ConfigManager.Deserialize<BaseItem>(text);
                        SyncManager.loadList.Add(item);
                        SyncManager.files.Add(item);
                        break;
                    case BaseType.Fish:
                        BaseFish fish = ConfigManager.Deserialize<BaseFish>(text);
                        SyncManager.loadList.Add(fish);
                        SyncManager.files.Add(fish);
                        break;
                    case BaseType.Projectile:
                        BaseProjectile projectile = ConfigManager.Deserialize<BaseProjectile>(text);
                        SyncManager.loadList.Add(projectile);
                        SyncManager.files.Add(projectile);
                        break;
                    case BaseType.Ragdoll:
                        BaseRagdoll ragdoll = ConfigManager.Deserialize<BaseRagdoll>(text);
                        SyncManager.loadList.Add(ragdoll);
                        SyncManager.files.Add(ragdoll);
                        break;
                    case BaseType.All:
                        BaseAggregate all = ConfigManager.Deserialize<BaseAggregate>(text);
                        all.Load();
                        SyncManager.files.Add(all);
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

    private static void RegisterSpawnList(SpawnDataRef[]? list)
    {
        if (list == null) return;
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

        string? fileName = Path.GetFileNameWithoutExtension(filePath);
        if (fileName.StartsWith("translations."))
        {
            LocalizationManager.UpdateParse(filePath);
            return;
        }
        
        string text = File.ReadAllText(filePath);
        try
        {
            Header header = ConfigManager.Deserialize<Header>(text);
            switch (header.Type)
            {
                case BaseType.Humanoid:
                    BaseHumanoid humanoid = ConfigManager.Deserialize<BaseHumanoid>(text);
                    humanoid.Update();
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Character:
                    BaseCharacter character = ConfigManager.Deserialize<BaseCharacter>(text);
                    character.Update();
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Human:
                    BaseHuman human = ConfigManager.Deserialize<BaseHuman>(text);
                    human.Update();
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Egg:
                    BaseEgg egg = ConfigManager.Deserialize<BaseEgg>(text);
                    egg.Update();
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Item:
                    BaseItem item = ConfigManager.Deserialize<BaseItem>(text);
                    item.Update();
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Fish:
                    BaseFish fish = ConfigManager.Deserialize<BaseFish>(text);
                    fish.Update();
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Projectile:
                    BaseProjectile projectile = ConfigManager.Deserialize<BaseProjectile>(text);
                    projectile.Update();
                    SyncManager.UpdateSync();
                    break;
                case BaseType.Ragdoll:
                    BaseRagdoll ragdoll = ConfigManager.Deserialize<BaseRagdoll>(text);
                    ragdoll.Update();
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

    public static void Export(string dirPath)
    {
        BaseAggregate export = new BaseAggregate();
        export.Init(dirPath);
        string data = ConfigManager.Serialize(export);
        string filePath = Path.Combine(ExportFolder, "All.yml");
        File.WriteAllText(filePath, data);
    }
}