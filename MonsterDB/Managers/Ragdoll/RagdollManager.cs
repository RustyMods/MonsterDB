using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class RagdollManager
{
    public static bool TrySave(GameObject prefab, out BaseRagdoll ragdoll, bool isClone = false, string source = "")
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        ragdoll = LoadManager.GetOriginal<BaseRagdoll>(prefab.name);
#pragma warning restore CS8601 // Possible null reference assignment.
        if (ragdoll != null) return true;

        if (!prefab.GetComponent<Ragdoll>()) return false;

        ragdoll = new BaseRagdoll();
        ragdoll.Setup(prefab, isClone, source);
        
        LoadManager.originals.Add(prefab.name, ragdoll);
        return true;
    }

    public static void Write(GameObject prefab, bool isClone = false, string clonedFrom = "", string dirPath = "")
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filePath = Path.Combine(dirPath, prefab.name + ".yml");
        if (TrySave(prefab, out BaseRagdoll ragdoll, isClone, clonedFrom))
        {
            string text = ConfigManager.Serialize(ragdoll);
            File.WriteAllText(filePath, text);
            MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
        }
    }

    private static void Read(string filePath)
    {
        if (!File.Exists(filePath)) return;
        string text = File.ReadAllText(filePath);
        try
        {
            Header header = ConfigManager.Deserialize<Header>(text);
            if (header.Type != BaseType.Ragdoll) return;
            BaseRagdoll reference = ConfigManager.Deserialize<BaseRagdoll>(text);
            reference.Update();
            LoadManager.UpdateSync();
        }
        catch
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
        }
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