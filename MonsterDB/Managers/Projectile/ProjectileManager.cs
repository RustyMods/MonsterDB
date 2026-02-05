using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class ProjectileManager
{
    [Obsolete]
    public static void WriteProjectileYML(Terminal.ConsoleEventArgs args)
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
            args.Context.LogWarning($"Failed to find prefab: {prefabName}");
            return;
        }

        if (!prefab.GetComponent<Projectile>())
        {
            args.Context.LogWarning("Invalid, missing projectile component");
            return;
        }

        Write(prefab);
    }

    [Obsolete]
    public static List<string> GetProjectileOptions(int i, string word) => i switch
    {
        2 => PrefabManager.GetAllPrefabNames<Projectile>(),
        _ => new List<string>()
    };

    [Obsolete]
    public static void CloneProjectile(Terminal.ConsoleEventArgs args)
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
    public static bool TrySave(GameObject prefab, out BaseProjectile data, bool isClone = false, string source = "")
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        data = LoadManager.GetOriginal<BaseProjectile>(prefab.name);
#pragma warning restore CS8601 // Possible null reference assignment.
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
            if (CloneManager.clones.TryGetValue(component.m_spawnOnHit.name, out Clone c))
            {
                isSpawnClone = true;
                spawnSource = c.SourceName;
            }
            
            SpawnAbilityManager.Write(component.m_spawnOnHit, isSpawnClone, spawnSource, dirPath);
        }
    }

    public static bool TryClone(GameObject source, string cloneName, out GameObject clone, bool write = true, string dirPath = "")
    {
        if (CloneManager.prefabs.TryGetValue(cloneName, out clone)) return true;
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
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
#pragma warning disable CS8601 // Possible null reference assignment.
        clone = c.Create();
#pragma warning restore CS8601 // Possible null reference assignment.
        return clone != null;
    }
}