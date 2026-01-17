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

        var clone = new Command("clone_projectile", "[prefabName][newName]: clones projectile and writes YML file",
            args =>
            {
                if (args.Length < 4)
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
                
                string newName = args[3];
                if (string.IsNullOrEmpty(newName))
                {
                    MonsterDBPlugin.LogWarning("Invalid parameters");
                    return false;
                }
            
                GameObject? prefab = PrefabManager.GetPrefab(prefabName);

                if (prefab == null)
                {
                    return true;
                }

                TryClone(prefab, newName, out _);
                
                return true;
            }, PrefabManager.GetAllPrefabNames<Projectile>, adminOnly: true);
    }
    
    public static bool TrySave(GameObject prefab, out BaseProjectile data, bool isClone = false, string source = "")
    {
        data = LoadManager.GetOriginal<BaseProjectile>(prefab.name);
        if (data != null) return true;
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

        if (prefab.TryGetComponent(out Projectile component) && component.m_spawnOnHit != null &&
            component.m_spawnOnHit.GetComponent<SpawnAbility>())
        {
            bool isSpawnClone = false;
            string spawnSource = "";
            if (PrefabManager.Clones.TryGetValue(component.m_spawnOnHit.name, out Clone c))
            {
                isSpawnClone = true;
                spawnSource = c.PrefabName;
            }
            
            SpawnAbilityManager.Write(component.m_spawnOnHit, isSpawnClone, spawnSource, dirPath);
        }
    }

    public static bool TryClone(GameObject source, string cloneName, out GameObject clone, bool write = true, string dirPath = "")
    {
        if (CloneManager.clones.TryGetValue(cloneName, out clone)) return true;
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

            if (p.TryGetComponent(out Projectile projectile) && projectile.m_spawnOnHit != null && projectile.m_spawnOnHit.GetComponent<SpawnAbility>())
            {
                string newName = $"MDB_{cloneName}_{projectile.m_spawnOnHit.name}";
                if (SpawnAbilityManager.TryClone(projectile.m_spawnOnHit, newName, 
                        out GameObject newSpawnOnHit, false))
                {
                    projectile.m_spawnOnHit = newSpawnOnHit;
                }
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