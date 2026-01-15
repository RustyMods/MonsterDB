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

    public static void Write(GameObject prefab, bool isClone = false, string source = "")
    {
        if (!TrySave(prefab, out BaseProjectile data, isClone, source)) return;
        
        string filePath = Path.Combine(FileManager.ExportFolder, prefab.name + ".yml");
        var text = ConfigManager.Serialize(data);
        File.WriteAllText(filePath, text);
    }

    public static void Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.clones.ContainsKey(cloneName)) return;
        if (!source.GetComponent<Projectile>()) return;

        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(p, true, source.name);
            }
        };
        c.Create();

    }
}