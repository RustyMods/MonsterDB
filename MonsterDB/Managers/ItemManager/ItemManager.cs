using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class ItemManager
{
    public static void Setup()
    {
        Command save = new Command("write_item", $"[prefabName]: write item YML to {FileManager.ExportFolder} folder", args =>
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

            if (!prefab.GetComponent<ItemDrop>())
            {
                MonsterDBPlugin.LogWarning("Invalid, missing ItemDrop component");
                return true;
            }

            Write(prefab);
            return true;
        });
        
        Command read = new Command("mod_item", $"[fileName]: read item YML from {FileManager.ImportFolder} folder", args =>
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
                return true;
            }
            
            string filePath = Path.Combine(FileManager.ImportFolder, prefabName + ".yml");
            Read(filePath);
            return true;
        }, FileManager.GetModFileNames, adminOnly: true);

        Command revert = new Command("revert_item", "[prefabName]: revert item to factory settings", args =>
        {
            if (args.Length < 3)
            {
                MonsterDBPlugin.LogInfo("Invalid parameters");
                return true;
            }
            
            string prefabName = args[2];
            if (string.IsNullOrEmpty(prefabName))
            {
                MonsterDBPlugin.LogInfo("Invalid prefab");
                return true;
            }

            if (SyncManager.GetOriginal<BaseItem>(prefabName) is not {} item)
            {
                MonsterDBPlugin.LogInfo("Original data not found");
                return true;
            }
            
            item.Update();
            string text = ConfigManager.Serialize(item);
            SyncManager.rawFiles[item.Prefab] = text;
            SyncManager.UpdateSync();
            
            return true;
        }, optionsFetcher: SyncManager.GetOriginalKeys<BaseItem>, adminOnly: true);

        Command clone = new Command("clone_item", "[prefabName][newName]: must be an item", args =>
        {
            if (args.Length < 3)
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            
            string prefabName = args[2];
            string newName = args[3];
            if (string.IsNullOrEmpty(prefabName) || string.IsNullOrEmpty(newName))
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            GameObject? prefab = PrefabManager.GetPrefab(prefabName);
            if (prefab == null)
            {
                return true;
            }

            if (!prefab.GetComponent<ItemDrop>())
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing ItemDrop component");
                return true;
            }
            
            Clone(prefab, newName);
            return true;
        }, optionsFetcher: PrefabManager.GetAllPrefabNames<ItemDrop>, adminOnly: true);

        Command search = new Command("search_item", "search item by name", args =>
        {
            if (args.Length < 3) return true;

            string query = args[2];
            var names = PrefabManager.SearchCache<ItemDrop>(query);
            for (int i = 0; i < names.Count; ++i)
            {
                MonsterDBPlugin.LogInfo(names[i]);
            }
            
            return true;
        }, () => PrefabManager.SearchCache<ItemDrop>(""));
    }
    

    public static string? Save(GameObject prefab, bool isClone = false, string source = "")
    {
        if (!prefab.GetComponent<ItemDrop>()) return null;

        if (SyncManager.GetOriginal<BaseItem>(prefab.name) is { } item) return ConfigManager.Serialize(item);
        BaseItem reference = new BaseItem();
        reference.Setup(prefab, isClone, source);
        
        SyncManager.originals.Add(prefab.name, reference);
        return ConfigManager.Serialize(reference);
    }

    private static void Write(GameObject prefab, bool isClone = false, string clonedFrom = "")
    {
        string filePath = Path.Combine(FileManager.ExportFolder, prefab.name + ".yml");
        string? text = Save(prefab, isClone, clonedFrom);
        if (string.IsNullOrEmpty(text)) return;
        File.WriteAllText(filePath, text);
        MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
    }
    
    private static void Read(string filePath)
    {
        if (!File.Exists(filePath)) return;
        string text = File.ReadAllText(filePath);
        try
        {
            Header header = ConfigManager.Deserialize<Header>(text);
            if (header.Type != BaseType.Item) return;
            BaseItem reference = ConfigManager.Deserialize<BaseItem>(text);
            reference.Update();
            SyncManager.rawFiles[reference.Prefab] = text;
            SyncManager.UpdateSync();
        }
        catch
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
        }
    }

    public static void Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.clones.ContainsKey(cloneName)) return;
        if  (!source.GetComponent<ItemDrop>()) return;
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            if (p.TryGetComponent(out ItemDrop component))
            {
                if (component.m_itemData.m_shared.m_attack.m_attackProjectile != null)
                {
                    Clone pr = new Clone(component.m_itemData.m_shared.m_attack.m_attackProjectile,
                        $"MDB_{cloneName}_{component.m_itemData.m_shared.m_attack.m_attackProjectile.name}");
                    pr.OnCreated += projectile =>
                    {
                        component.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;
                    };
                    pr.Create();
                }
            }
            
            
            Renderer[]? renderers = p.GetComponentsInChildren<Renderer>(true);
            Dictionary<string, Material> newMaterials = new Dictionary<string, Material>();
            
            for (int i = 0; i < renderers.Length; ++i)
            {
                Renderer renderer = renderers[i];
                CloneMaterials(renderer, ref  newMaterials);
            }

            void CloneMaterials(Renderer r, ref Dictionary<string, Material> mats)
            {
                List<Material> newMats = new();
                for (int i = 0; i < r.sharedMaterials.Length; ++i)
                {
                    Material mat = r.sharedMaterials[i];
                    if (mat == null) continue;
                    string name = $"MDB_{cloneName}_{mat.name.Replace("(Instance)", string.Empty)}";
                    if (mats.TryGetValue(name, out Material? clonedMat))
                    {
                        newMats.Add(clonedMat);
                    }
                    else
                    {
                        clonedMat = new Material(mat);
                        clonedMat.name = name;
                        newMats.Add(clonedMat);
                        mats.Add(name, clonedMat);
                    }
                }
                r.sharedMaterials = newMats.ToArray();
                r.materials = newMats.ToArray();
            }
            
            
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(p, true, source.name);
            }
        };
        c.Create();
    }
}