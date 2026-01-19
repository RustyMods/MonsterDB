using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class FishManager
{
    public static void Setup()
    {
        Command save = new Command("write_fish", $"[prefabName]: write fish YML to {FileManager.ExportFolder} folder", args =>
        {
            if (args.Length < 3)
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            
            string prefabName = args[2];
            if (string.IsNullOrEmpty(prefabName))
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return false;
            }
            
            GameObject? prefab = PrefabManager.GetPrefab(prefabName);

            if (prefab == null)
            {
                return true;
            }

            if (!prefab.GetComponent<ItemDrop>())
            {
                MonsterDBPlugin.LogWarning("Invalid, missing ItemDrop component");
                return true;
            }

            Write(prefab);
            return true;
        });

        Command saveAll = new Command("write_all_fish", $"write all fish YML to {FileManager.ExportFolder} folder", _ =>
        {
            List<GameObject> list = PrefabManager.GetAllPrefabs<Fish>();
            foreach (GameObject? fish in list)
            {
                Write(fish);
            }
            return true;
        });
        
        Command read = new Command("mod_fish", $"[fileName]: read fish reference from {FileManager.ImportFolder} folder", args =>
        {
            if (args.Length < 3)
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            
            string prefabName = args[2];
            if (string.IsNullOrEmpty(prefabName))
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            
            string filePath = Path.Combine(FileManager.ImportFolder, prefabName + ".yml");
            Read(filePath);
            return true;
        }, FileManager.GetModFileNames, adminOnly: true);

        Command revert = new Command("revert_fish", "[prefabName]: revert fish to factory settings", args =>
        {
            if (args.Length < 3)
            {
                MonsterDBPlugin.LogInfo("Invalid parameters");
                return true;
            }
            
            string prefabName = args[2];
            if (string.IsNullOrEmpty(prefabName))
            {
                MonsterDBPlugin.LogInfo("Invalid prefab");
                return true;
            }

            if (LoadManager.GetOriginal<BaseFish>(prefabName) is not {} fish)
            {
                MonsterDBPlugin.LogInfo("Original data not found");
                return true;
            }
            
            fish.Update();
            LoadManager.UpdateSync();
            
            return true;
        }, optionsFetcher: LoadManager.GetOriginalKeys<BaseFish>, adminOnly: true);

        Command clone = new Command("clone_fish", "[prefabName][newName]: must be a fish", args =>
        {
            if (args.Length < 3)
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            
            string prefabName = args[2];
            string newName = args[3];
            if (string.IsNullOrEmpty(prefabName) || string.IsNullOrEmpty(newName))
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            GameObject? prefab = PrefabManager.GetPrefab(prefabName);
            if (prefab == null)
            {
                return true;
            }

            if (!prefab.GetComponent<ItemDrop>())
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing ItemDrop component");
                return true;
            }
            
            Clone(prefab, newName);
            return true;
        }, optionsFetcher: PrefabManager.GetAllPrefabNames<Fish>, adminOnly: true);
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

    private static void Write(GameObject prefab, bool isClone = false, string clonedFrom = "")
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