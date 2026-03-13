using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class CreatureSpawnerManager
{
    [Obsolete]
    public static void WriteAllCreatureSpawners(Terminal.ConsoleEventArgs args)
    {
        foreach (GameObject? prefab in PrefabManager.GetAllPrefabs<CreatureSpawner>())
        {
            bool isClone = false;
            string source = "";
            if (CloneManager.clones.TryGetValue(prefab.name, out Clone clone))
            {
                isClone = true;
                source = clone.SourceName;
            }
            Write(prefab, isClone, source);
        }
    }
    
    [Obsolete]
    public static void WriteCreatureSpawner(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }
        GameObject? prefab = PrefabManager.GetPrefab(prefabName);
        if (prefab == null)
        {
            args.Context.LogWarning($"Failed to find prefab {prefabName}");
            return;
        }

        if (!prefab.GetComponent<CreatureSpawner>())
        {
            args.Context.LogWarning($"Prefab {prefabName} is not a CreatureSpawner");
            return;
        }
        
        bool isClone = CloneManager.IsClone(prefab.name, out string source);
        Write(prefab, isClone, source);
    }

    [Obsolete]
    public static List<string> GetCreatureSpawnersOptions(int i, string word) => i switch
    {
        2 => PrefabManager.GetAllPrefabNames<CreatureSpawner>(),
        _ => []
    };

    public static bool Write(GameObject prefab, bool isClone, string source, Terminal? context = null)
    {
        string text = Save(prefab, isClone, source);
        string filepath = Path.Combine(FileManager.ExportFolder, prefab.name + ".yml");
        File.WriteAllText(filepath, text);
        context?.LogInfo($"Exported Creature Spawner {prefab.name}");
        context?.LogInfo(filepath.RemoveRootPath());
        return true;
    }

    private static string Save(GameObject prefab, bool isClone, string clonedFrom)
    {
        if (LoadManager.GetOriginal<BaseCreatureSpawner>(prefab.name) is { } reference)
        {
            return ConfigManager.Serialize(reference);
        }

        reference = new BaseCreatureSpawner(prefab, isClone, clonedFrom);
        LoadManager.originals.Add(prefab.name, reference);
        return ConfigManager.Serialize(reference);
    }

    public static bool TryClone(
        GameObject source, 
        string cloneName, 
        bool write = true, 
        Terminal? context = null)
    {
        if (CloneManager.prefabs.ContainsKey(cloneName))
        {
            context?.LogWarning($"Clone {cloneName} already exists");
            return false;
        }

        if (!source.GetComponent<CreatureSpawner>())
        {
            context?.LogWarning($"{source.name} missing Creature Spawner component");
            return false;
        }
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(p, true, source.name, context: context);
            }
        };
        c.Create();
        context?.LogInfo($"Cloned {source.name} as {cloneName}");
        return true;
    }
}