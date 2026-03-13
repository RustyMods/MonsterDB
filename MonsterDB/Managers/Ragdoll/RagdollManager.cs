using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class RagdollManager
{
    private static bool TrySave(
        GameObject prefab, 
        out BaseRagdoll ragdoll, 
        bool isClone = false, 
        string source = "")
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

    public static void Write(
        GameObject prefab, 
        bool isClone = false, 
        string clonedFrom = "", 
        string dirPath = "", 
        Terminal? context = null)
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filepath = Path.Combine(dirPath, prefab.name + ".yml");
        if (TrySave(prefab, out BaseRagdoll ragdoll, isClone, clonedFrom))
        {
            string text = ConfigManager.Serialize(ragdoll);
            File.WriteAllText(filepath, text);
            MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filepath}");
            context?.LogInfo($"Exported {prefab.name}");
            context?.LogInfo(filepath.RemoveRootPath());
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

    public static bool TryClone(
        GameObject source, 
        string cloneName, 
        out GameObject clone, 
        bool write = true, 
        string dirPath = "",
        Terminal? context = null)
    {
        if (CloneManager.prefabs.TryGetValue(cloneName, out clone))
        {
            context?.LogWarning($"{cloneName} already exists");
            return true;
        }
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

    public static bool TryGetRagdoll(GameObject prefab, out GameObject output)
    {
        output = prefab;
        if (!prefab.TryGetComponent(out Character character)) return false;
        for (int i = 0; i < character.m_deathEffects.m_effectPrefabs.Length; ++i)
        {
            var effect = character.m_deathEffects.m_effectPrefabs[i];
            if (effect.m_prefab.GetComponent<Ragdoll>())
            {
                output = effect.m_prefab;
                return true;
            }
        }
        return false;
    }
}