using System.IO;
using BepInEx;
using MonsterDB.DataBase;
using Paths = MonsterDB.DataBase.Paths;

namespace MonsterDB.FileWatcher;

public static class FileWatch
{
    public static void InitFileWatch()
    {
        FileSystemWatcher MonsterWatcher = new FileSystemWatcher(Paths.MonsterPath, "*.yml")
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };

        MonsterWatcher.Changed += OnCreatureChange;
        MonsterWatcher.Deleted += OnCreatureDelete;

        FileSystemWatcher SpawnWatcher = new FileSystemWatcher(Paths.SpawnPath, "*.yml")
        {
            Filter = "*.yml",
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            SynchronizingObject = ThreadingHelper.SynchronizingObject,
            NotifyFilter = NotifyFilters.LastWrite
        };

        SpawnWatcher.Changed += OnSpawnChange;
        SpawnWatcher.Created += OnSpawnChange;
        SpawnWatcher.Deleted += OnSpawnChange;
    }

    private static void OnSpawnChange(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance.IsServer()) return;
        string fileName = Path.GetFileName(e.Name);
        if (fileName == "Example.yml") return;
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Changed:
                MonsterDBPlugin.MonsterDBLogger.LogInfo("File changed: " + fileName);
                SpawnData.ReadFile(e.FullPath);
                break;
            case WatcherChangeTypes.Created:
                MonsterDBPlugin.MonsterDBLogger.LogInfo("File created: " + fileName);
                SpawnData.ReadFile(e.FullPath);
                break;
            case WatcherChangeTypes.Deleted:
                MonsterDBPlugin.MonsterDBLogger.LogInfo("File deleted: " + fileName);
                SpawnData.ClearSpawnData();
                SpawnData.ReadSpawnFiles();
                break;
        }
    }

    private static void OnCreatureDelete(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance.IsServer()) return;
        string fileName = Path.GetFileName(e.Name);
        MonsterDBPlugin.MonsterDBLogger.LogInfo("File deleted: " + fileName);

        if (MonsterManager.ResetMonster(fileName.Replace(".yml", string.Empty)))
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Server: Successfully reset creature after deleting file");
            DataBase.ServerSync.UpdateServerMonsterDB();
        }
        else
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Server: Failed to reset creature after deleting file");
        }
    }

    private static void OnCreatureChange(object sender, FileSystemEventArgs e)
    {
        if (!ZNet.instance.IsServer()) return;
        string fileName = Path.GetFileName(e.Name);
        MonsterDBPlugin.MonsterDBLogger.LogInfo("File changed: " + fileName);

        if (MonsterManager.FileWatch_UpdateMonster(e.FullPath, fileName))
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Server: Successfully updated " + fileName);
            DataBase.ServerSync.UpdateServerMonsterDB();
        }
        else
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Server: Failed to update " + fileName);
        }
    }
}