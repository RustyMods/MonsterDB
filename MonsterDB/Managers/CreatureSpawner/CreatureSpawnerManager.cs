using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class CreatureSpawnerManager
{
    public static void Setup()
    {
        Command write_all = new Command("write_all_creature_spawners", "", _ =>
        {
            foreach (var prefab in PrefabManager.GetAllPrefabs<CreatureSpawner>())
            {
                bool isClone = false;
                string source = "";
                if (CloneManager.clones.TryGetValue(prefab.name, out Clone clone))
                {
                    isClone = true;
                    source = clone.PrefabName;
                }
                Write(prefab, isClone, source);
            }
            return true;
        });

        Command write = new Command("write_creature_spawner", "", args =>
        {
            var prefabName = args.GetString(2);
            if (string.IsNullOrEmpty(prefabName)) return true;
            var prefab = PrefabManager.GetPrefab(prefabName);
            if (prefab == null || !prefab.GetComponent<CreatureSpawner>()) return true;
            bool isClone = false;
            string source = "";
            if (CloneManager.clones.TryGetValue(prefab.name, out Clone clone))
            {
                isClone = true;
                source = clone.PrefabName;
            }
            Write(prefab, isClone, source);
            return true;
        }, PrefabManager.GetAllPrefabNames<CreatureSpawner>);
    }

    public static void Write(GameObject prefab, bool isClone, string source)
    {
        var text = Save(prefab, isClone, source);
        var filePath = Path.Combine(FileManager.ExportFolder, prefab.name + ".yml");
        File.WriteAllText(filePath, text);
    }

    public static string Save(GameObject prefab, bool isClone, string clonedFrom)
    {
        if (LoadManager.GetOriginal<BaseCreatureSpawner>(prefab.name) is { } reference)
        {
            return ConfigManager.Serialize(reference);
        }

        reference = new BaseCreatureSpawner();
        reference.Setup(prefab, isClone, clonedFrom);
        LoadManager.originals.Add(prefab.name, reference);
        return ConfigManager.Serialize(reference);
    }

    public static void Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.prefabs.ContainsKey(cloneName)) return;
        if (!source.GetComponent<CreatureSpawner>()) return;
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