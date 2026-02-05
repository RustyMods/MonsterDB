using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class SpawnAbilityManager
{
    [Obsolete]
    public static void WriteSpawnAbilityYML(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args[2];
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }
            
        GameObject? prefab = PrefabManager.GetPrefab(prefabName);

        if (prefab == null)
        {
            args.Context.LogWarning($"Failed to find prefab: {prefabName}");
            return;
        }

        if (!prefab.GetComponent<SpawnAbility>())
        {
            args.Context.LogWarning("Invalid prefab, missing SpawnAbility component");
            return;
        }

        bool isClone = CloneManager.IsClone(prefab.name, out string source);
        Write(prefab, isClone, source);
    }

    [Obsolete]
    public static List<string> GetSpawnAbilityOptions(int i, string word) => i switch
    {
        2 => PrefabManager.GetAllPrefabNames<SpawnAbility>(),
        _ => new List<string>()
    };

    [Obsolete]
    public static void CloneSpawnAbility(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        string newName = args.GetString(3);
        if (string.IsNullOrEmpty(newName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }
            
        GameObject? prefab = PrefabManager.GetPrefab(prefabName);

        if (prefab == null)
        {
            args.Context.LogWarning($"Failed to find prefab: {prefabName}");
            return;
        }
            
        if (!prefab.GetComponent<SpawnAbility>())
        {
            args.Context.LogWarning("Invalid prefab, missing SpawnAbility component");
            return;
        }

        TryClone(prefab, newName, out _, true);
    }
    public static void Write(GameObject prefab, bool isClone = false, string source = "", string dirPath = "")
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filePath = Path.Combine(dirPath, prefab.name + ".yml");
        BaseSpawnAbility spawnAbility = new();
        spawnAbility.Setup(prefab, isClone, source);
        string text = ConfigManager.Serialize(spawnAbility);
        File.WriteAllText(filePath, text);
    }

    public static bool TryClone(GameObject prefab, string cloneName, out GameObject clone, bool write = true,
        string dirPath = "")
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        if (CloneManager.prefabs.TryGetValue(cloneName, out clone)) return true;
        
        Clone c = new Clone(prefab, cloneName);
        c.OnCreated += p =>
        {
            if (write)
            {
                Write(p, true, prefab.name, dirPath);
            }
            
            MonsterDBPlugin.LogDebug($"Cloned {prefab.name} as {cloneName}");
        };
#pragma warning disable CS8601 // Possible null reference assignment.
        clone = c.Create();
#pragma warning restore CS8601 // Possible null reference assignment.
        return clone != null;
    }
}