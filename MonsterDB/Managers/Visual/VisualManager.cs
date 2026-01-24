using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class VisualManager
{
    public static void Setup()
    {
        Command export = new Command("write_visual", "[prefabName]: write prefab visual data YML file", args =>
        {
            string? prefabName = args.GetString(2);
            if (string.IsNullOrEmpty(prefabName))
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return false;
            }

            GameObject? prefab = PrefabManager.GetPrefab(prefabName);

            if (prefab == null)
            {
                MonsterDBPlugin.LogWarning($"Failed to find prefab: {prefabName}");
                return true;
            }

            bool isClone = false;
            string source = "";

            if (CloneManager.clones.TryGetValue(prefabName, out Clone c))
            {
                isClone = true;
                source = c.PrefabName;
            }
            
            Write(prefab, isClone, source);
            return true;
        });

        Command clone = new Command("clone_visual", "[prefabName]: clone prefab and write visual YML file",
            args =>
            {
                string prefabName = args.GetString(2);
                string newName = args.GetString(3);
                if (string.IsNullOrEmpty(prefabName) || string.IsNullOrEmpty(newName))
                {
                    MonsterDBPlugin.LogWarning("Invalid parameters");
                    return true;
                }

                GameObject? prefab = PrefabManager.GetPrefab(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.LogWarning($"Failed to find prefab: {prefabName}");
                    return true;
                }

                TryClone(prefab, newName, out _);
                
                return true;
            }, adminOnly: true);
    }

    public static void Write(GameObject prefab, bool isClone = false, string source = "", string dirPath = "")
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filePath = Path.Combine(dirPath, $"{prefab.name}.yml");
        if (TrySave(prefab, out BaseVisual visual, isClone, source))
        {
            string text = ConfigManager.Serialize(visual);
            File.WriteAllText(filePath, text);
            MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
        }
    }

    public static bool TrySave(GameObject prefab, out BaseVisual visual, bool isClone = false, string source = "")
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        visual = LoadManager.GetOriginal<BaseVisual>(prefab.name);
#pragma warning restore CS8601 // Possible null reference assignment.
        if (visual != null) return true;
        
        visual = new BaseVisual();
        visual.Setup(prefab, isClone, source);
        LoadManager.originals.Add(prefab.name, visual);
        return true;
    }
    
    public static bool TryClone(GameObject source, string cloneName, out GameObject clone, bool write = true, string dirPath = "")
    {
        if (CloneManager.prefabs.TryGetValue(cloneName, out clone)) return true;
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(p, true, source.name, dirPath);
            }
        };
#pragma warning disable CS8601 // Possible null reference assignment.
        clone = c.Create();
#pragma warning restore CS8601 // Possible null reference assignment.
        return clone != null;
    }
}