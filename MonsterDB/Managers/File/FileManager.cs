using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;

namespace MonsterDB;

public static class FileManager
{
    public const string ExportFolderName = "Export";
    public const string ImportFolderName = "Import";
    public static readonly string ExportFolder;
    public static readonly string ImportFolder;
    private static readonly ConfigEntry<Toggle> _fileWatcherEnabled;
    public static bool started;
    
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
        if (started) return;
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
                        LoadManager.loadList.Add(character);
                        RegisterSpawnList(character.SpawnData);
                        LoadManager.files.Add(character);
                        break;
                    case BaseType.Humanoid:
                        BaseHumanoid humanoid = ConfigManager.Deserialize<BaseHumanoid>(text);
                        LoadManager.loadList.Add(humanoid);
                        RegisterSpawnList(humanoid.SpawnData);
                        LoadManager.files.Add(humanoid);
                        break;
                    case BaseType.Human:
                        BaseHuman player = ConfigManager.Deserialize<BaseHuman>(text);
                        LoadManager.loadList.Add(player);
                        RegisterSpawnList(player.SpawnData);
                        LoadManager.files.Add(player);
                        break;
                    case BaseType.Egg:
                        BaseEgg data = ConfigManager.Deserialize<BaseEgg>(text);
                        LoadManager.loadList.Add(data);
                        LoadManager.files.Add(data);
                        break;
                    case BaseType.Item:
                        BaseItem item = ConfigManager.Deserialize<BaseItem>(text);
                        LoadManager.loadList.Add(item);
                        LoadManager.files.Add(item);
                        break;
                    case BaseType.Fish:
                        BaseFish fish = ConfigManager.Deserialize<BaseFish>(text);
                        LoadManager.loadList.Add(fish);
                        LoadManager.files.Add(fish);
                        break;
                    case BaseType.Projectile:
                        BaseProjectile projectile = ConfigManager.Deserialize<BaseProjectile>(text);
                        LoadManager.loadList.Add(projectile);
                        LoadManager.files.Add(projectile);
                        break;
                    case BaseType.Ragdoll:
                        BaseRagdoll ragdoll = ConfigManager.Deserialize<BaseRagdoll>(text);
                        LoadManager.loadList.Add(ragdoll);
                        LoadManager.files.Add(ragdoll);
                        break;
                    case BaseType.SpawnAbility:
                        BaseSpawnAbility spawnAbility = ConfigManager.Deserialize<BaseSpawnAbility>(text);
                        LoadManager.loadList.Add(spawnAbility);
                        LoadManager.files.Add(spawnAbility);
                        break;
                    case BaseType.Visual:
                        BaseVisual visual = ConfigManager.Deserialize<BaseVisual>(text);
                        LoadManager.loadList.Add(visual);
                        LoadManager.files.Add(visual);
                        break;
                    case BaseType.All:
                        BaseAggregate all = ConfigManager.Deserialize<BaseAggregate>(text);
                        LoadManager.loadList.AddRange(all.Load());
                        LoadManager.files.Add(all);
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
        started = true;
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
            LocalizationManager.UpdateWords(filePath);
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
                    LoadManager.UpdateSync();
                    break;
                case BaseType.Character:
                    BaseCharacter character = ConfigManager.Deserialize<BaseCharacter>(text);
                    character.Update();
                    LoadManager.UpdateSync();
                    break;
                case BaseType.Human:
                    BaseHuman human = ConfigManager.Deserialize<BaseHuman>(text);
                    human.Update();
                    LoadManager.UpdateSync();
                    break;
                case BaseType.Egg:
                    BaseEgg egg = ConfigManager.Deserialize<BaseEgg>(text);
                    egg.Update();
                    LoadManager.UpdateSync();
                    break;
                case BaseType.Item:
                    BaseItem item = ConfigManager.Deserialize<BaseItem>(text);
                    item.Update();
                    LoadManager.UpdateSync();
                    break;
                case BaseType.Fish:
                    BaseFish fish = ConfigManager.Deserialize<BaseFish>(text);
                    fish.Update();
                    LoadManager.UpdateSync();
                    break;
                case BaseType.Projectile:
                    BaseProjectile projectile = ConfigManager.Deserialize<BaseProjectile>(text);
                    projectile.Update();
                    LoadManager.UpdateSync();
                    break;
                case BaseType.Ragdoll:
                    BaseRagdoll ragdoll = ConfigManager.Deserialize<BaseRagdoll>(text);
                    ragdoll.Update();
                    LoadManager.UpdateSync();
                    break;
                case BaseType.SpawnAbility:
                    BaseSpawnAbility spawnAbility = ConfigManager.Deserialize<BaseSpawnAbility>(text);
                    spawnAbility.Update();
                    LoadManager.UpdateSync();
                    break;
                case BaseType.Visual:
                    BaseVisual visual = ConfigManager.Deserialize<BaseVisual>(text);
                    visual.Update();
                    LoadManager.UpdateSync();
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

    private static void Export(string dirPath)
    {
        try
        {
            BaseAggregate export = new BaseAggregate();
            export.Read(dirPath);
            string data = ConfigManager.Serialize(export);
            string filePath = Path.Combine(ExportFolder, "All.yml");
            File.WriteAllText(filePath, data);
        }
        catch
        {
            MonsterDBPlugin.LogWarning("Failed to aggregate files");
        }
    }
}