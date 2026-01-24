using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class SpawnAbilityManager
{
    public static void Setup()
    {
        Command write = new Command("write_spawnability", "[prefabName]: write spawn ability YML file", args =>
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

            if (!prefab.GetComponent<SpawnAbility>())
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing SpawnAbility component");
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
        }, PrefabManager.GetAllPrefabNames<SpawnAbility>);

        Command clone = new Command("clone_spawnability", "[prefabName]: clone spawn ability YML file", args =>
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
                MonsterDBPlugin.LogWarning($"Failed to find prefab: {prefabName}");
                return true;
            }



            if (!prefab.GetComponent<SpawnAbility>())
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing SpawnAbility component");
                return true;
            }

            TryClone(prefab, newName, out _, true);
            
            return true;
        }, PrefabManager.GetAllPrefabNames<SpawnAbility>, adminOnly: true);
    }

    public static void Write(GameObject prefab, bool isClone = false, string source = "", string dirPath = "")
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filePath = Path.Combine(dirPath, prefab.name + ".yml");
        BaseSpawnAbility spawnAbility = new();
        spawnAbility.Setup(prefab, isClone, source);
        string text = ConfigManager.Serialize(spawnAbility);
        File.WriteAllText(filePath, text);
    }

    public static bool TryClone(GameObject prefab, string cloneName, out GameObject clone, bool write = true,
        string dirPath = "")
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        if (CloneManager.prefabs.TryGetValue(cloneName, out clone)) return true;
        
        Clone c = new Clone(prefab, cloneName);
        c.OnCreated += p =>
        {
            if (write)
            {
                Write(p, true, prefab.name, dirPath);
            }
            
            MonsterDBPlugin.LogDebug($"Cloned {prefab.name} as {cloneName}");
        };
#pragma warning disable CS8601 // Possible null reference assignment.
        clone = c.Create();
#pragma warning restore CS8601 // Possible null reference assignment.
        return clone != null;
    }
}