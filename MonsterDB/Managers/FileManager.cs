using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;

namespace MonsterDB;

public static class FileManager
{
    public static readonly string SaveFolder;
    public static readonly string ModifiedFolder;
    
    static FileManager()
    {
        SaveFolder = Path.Combine(ConfigManager.DirectoryPath, "Save");
        ModifiedFolder = Path.Combine(ConfigManager.DirectoryPath, "Modified");

        if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
        if (!Directory.Exists(ModifiedFolder)) Directory.CreateDirectory(ModifiedFolder);
    }
    
    public static List<string> GetModFileNames() => Directory
            .GetFiles(ModifiedFolder, "*.yml", SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();
    
    public static void Start()
    {
        string[] files =  Directory.GetFiles(ModifiedFolder, "*.yml", SearchOption.AllDirectories);
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
                        break;
                    case BaseType.Humanoid:
                        BaseHumanoid humanoid = ConfigManager.Deserialize<BaseHumanoid>(text);
                        SyncManager.loadList.Add(humanoid);
                        SyncManager.rawFiles[humanoid.Prefab] = text;
                        break;
                    case BaseType.Human:
                        BaseHuman player = ConfigManager.Deserialize<BaseHuman>(text);
                        SyncManager.loadList.Add(player);
                        SyncManager.rawFiles[player.Prefab] = text;
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
                }

            }
            catch (Exception ex)
            {
                MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
                MonsterDBPlugin.LogDebug(ex.Message);
            }
        }
        MonsterDBPlugin.LogInfo($"Loaded {files.Length} modified creature files.");
    }
    
    public static void SetupFileWatcher()
    {
        FileSystemWatcher watcher = new(ModifiedFolder, "*.yml");
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private static void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
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
            }
        }
        catch (Exception ex)
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
            MonsterDBPlugin.LogDebug(ex.Message);
        }
    }
}