using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class VisualManager
{
    [Obsolete]
    public static void WriteVisualYML(Terminal.ConsoleEventArgs args)
    {
        string? prefabName = args.GetString(2);
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

        bool isClone = CloneManager.IsClone(prefab.name, out string source);
        Write(prefab, isClone, source);
    }

    [Obsolete]
    public static void CloneVisual(Terminal.ConsoleEventArgs args)
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

        TryClone(prefab, newName, out _);
    }
    public static bool Write(
        GameObject prefab, 
        bool isClone = false, 
        string source = "", 
        string dirPath = "", 
        Terminal? context = null)
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filepath = Path.Combine(dirPath, $"{prefab.name}.yml");
        if (!TrySave(prefab, out BaseVisual visual, isClone, source))
        {
            return false;
        }
        string text = ConfigManager.Serialize(visual);
        File.WriteAllText(filepath, text);
        MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filepath}");
        context?.LogInfo($"Exported Visuals {prefab.name}");
        context?.LogInfo(filepath.RemoveRootPath());

        return true;
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
    
    public static bool TryClone(
        GameObject source, 
        string cloneName, 
        out GameObject clone, 
        bool write = true, 
        string dirPath = "",
        Terminal? context = null)
    {
        if (CloneManager.prefabs.TryGetValue(cloneName, out clone)) return true;
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(p, true, source.name, dirPath, context);
            }
        };
#pragma warning disable CS8601 // Possible null reference assignment.
        clone = c.Create();
#pragma warning restore CS8601 // Possible null reference assignment.
        context?.LogInfo($"Cloned {source.name} as {cloneName}");
        return clone != null;
    }
}