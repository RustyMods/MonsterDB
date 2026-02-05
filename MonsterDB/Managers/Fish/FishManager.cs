using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class FishManager
{
    [Obsolete]
    public static void WriteFishYML(Terminal.ConsoleEventArgs args)
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
            return;
        }

        if (!prefab.GetComponent<ItemDrop>())
        {
            args.Context.LogWarning("Invalid, missing ItemDrop component");
            return;
        }
        Write(prefab);
    }

    [Obsolete]
    public static List<string> GetFishOptions(int i, string word) => i switch
    {
        2 => PrefabManager.GetAllPrefabNames<Fish>(),
        _ => new List<string>()
    };

    [Obsolete]
    public static void WriteAllFishYML(Terminal.ConsoleEventArgs args)
    {
        List<GameObject> list = PrefabManager.GetAllPrefabs<Fish>();
        foreach (GameObject? fish in list)
        {
            Write(fish);
        }
    }

    [Obsolete]
    public static void UpdateFishYML(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }
            
        string filePath = Path.Combine(FileManager.ImportFolder, prefabName + ".yml");
        Read(filePath);
    }

    [Obsolete]
    public static void ResetFish(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid prefab");
            return;
        }

        if (LoadManager.GetOriginal<BaseFish>(prefabName) is not {} fish)
        {
            args.Context.LogWarning("Original data not found");
            return;
        }
            
        fish.Update();
        LoadManager.UpdateSync();
    }

    public static void ResetFish(Terminal context, string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            context.LogWarning("Invalid prefab");
            return;
        }

        if (LoadManager.GetOriginal<BaseFish>(prefabName) is not {} fish)
        {
            context.LogWarning("Original data not found");
            return;
        }
            
        fish.Update();
        LoadManager.UpdateSync();
    }

    [Obsolete]
    public static List<string> GetResetFishOptions(int i, string word) => i switch
    {
        2 => LoadManager.GetOriginalKeys<BaseFish>(),
        _ => new List<string>()
    };

    [Obsolete]
    public static void CloneFish(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        string newName = args.GetString(3);
        if (string.IsNullOrEmpty(prefabName) || string.IsNullOrEmpty(newName))
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

        if (!prefab.GetComponent<ItemDrop>())
        {
            args.Context.LogWarning("Invalid prefab, missing ItemDrop component");
            return;
        }
            
        Clone(prefab, newName);
    }
    
    public static string? Save(GameObject prefab, bool isClone = false, string source = "")
    {
        if (!prefab.GetComponent<ItemDrop>()) return null;

        if (LoadManager.GetOriginal<BaseFish>(prefab.name) is { } item) return ConfigManager.Serialize(item);
        BaseFish reference = new BaseFish();
        reference.Setup(prefab, isClone, source);
        
        LoadManager.originals.Add(prefab.name, reference);
        return ConfigManager.Serialize(reference);
    }

    public static void Write(GameObject prefab, bool isClone = false, string clonedFrom = "")
    {
        string filePath = Path.Combine(FileManager.ExportFolder, prefab.name + ".yml");
        string? text = Save(prefab, isClone, clonedFrom);
        if (string.IsNullOrEmpty(text)) return;
        File.WriteAllText(filePath, text);
        MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
    }
    
    private static void Read(string filePath)
    {
        if (!File.Exists(filePath)) return;
        string text = File.ReadAllText(filePath);
        try
        {
            Header header = ConfigManager.Deserialize<Header>(text);
            if (header.Type != BaseType.Fish) return;
            BaseFish reference = ConfigManager.Deserialize<BaseFish>(text);
            reference.Update();
            LoadManager.UpdateSync();
        }
        catch
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
        }
    }

    public static void Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.prefabs.ContainsKey(cloneName)) return;
        if  (!source.GetComponent<ItemDrop>()) return;
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (!p.GetComponent<Fish>())
            {
                p.AddComponent<Fish>();
            }
            
            if (write)
            {
                Write(p, true, source.name);
            }
        };
        c.Create();
    }
}