using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class ProjectileManager
{
    public static void Setup()
    {
        var save = new Command("write_projectile", "[prefabName]", args =>
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

            if (!prefab.GetComponent<Projectile>())
            {
                MonsterDBPlugin.LogWarning("Invalid, missing projectile component");
                return true;
            }

            Write(prefab);
            
            return true;
        }, PrefabManager.GetAllPrefabNames<Projectile>);
    }
    
    public static bool TrySave(GameObject prefab, out BaseProjectile data, bool isClone = false, string source = "")
    {
        data = SyncManager.GetOriginal<BaseProjectile>(prefab.name);
        if (data != null) return true;
        if (!prefab.GetComponent<Projectile>()) return false;

        data = new BaseProjectile();
        data.Setup(prefab, isClone, source);
        return true;
    }

    public static void Write(GameObject prefab, bool isClone = false, string source = "", string dirPath = "")
    {
        if (!TrySave(prefab, out BaseProjectile data, isClone, source)) return;
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filePath = Path.Combine(dirPath, prefab.name + ".yml");
        string text = ConfigManager.Serialize(data);
        File.WriteAllText(filePath, text);
        MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
    }

    public static bool TryClone(GameObject source, string cloneName, out GameObject clone, bool write = true, string dirPath = "")
    {
        if (CloneManager.clones.TryGetValue(cloneName, out clone)) return true;
        if (!source.GetComponent<Projectile>()) return false;
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            Renderer[]? renderers = p.GetComponentsInChildren<Renderer>(true);
            Dictionary<string, Material> newMaterials = new Dictionary<string, Material>();

            for (int i = 0; i < renderers.Length; ++i)
            {
                Renderer renderer = renderers[i];
                VisualUtils.CloneMaterials(renderer, ref newMaterials);
            }
            
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(p, true, source.name, dirPath);
            }
        };
        clone = c.Create();
        return clone != null;
    }
}