using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class SpawnAreaManager
{
    public static string? Save(GameObject prefab, bool isClone = false, string source = "")
    {
        if (LoadManager.GetOriginal<BaseSpawnArea>(prefab.name) is { } area) return ConfigManager.Serialize(area);
        BaseSpawnArea reference = new BaseSpawnArea();
        reference.Setup(prefab, isClone, source);
        LoadManager.originals.Add(prefab.name, reference);
        return ConfigManager.Serialize(reference);
    }

    public static void Read(string filePath)
    {
        if (!File.Exists(filePath)) return;
        string text = File.ReadAllText(filePath);
        try
        {
            Header header = ConfigManager.Deserialize<Header>(text);
            if (header.Type != BaseType.SpawnArea) return;
            BaseSpawnArea reference = ConfigManager.Deserialize<BaseSpawnArea>(text);
            reference.Update();
            LoadManager.UpdateSync();
        }
        catch
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
        }
    }

    public static void Write(GameObject prefab, bool isClone = false, string source = "", string dirPath = "")
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filePath = Path.Combine(dirPath, prefab.name + ".yml");
        BaseSpawnArea spawnArea = new();
        spawnArea.Setup(prefab, isClone, source);
        string text = ConfigManager.Serialize(spawnArea);
        File.WriteAllText(filePath, text);
    }

    public static void Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.prefabs.ContainsKey(cloneName)) return;
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (!p.GetComponent<SpawnArea>())
            {
                p.AddComponent<SpawnArea>();
            }
            
            if (write)
            {
                Write(p, true, source.name);
            }
        };
        c.Create();
    }
}