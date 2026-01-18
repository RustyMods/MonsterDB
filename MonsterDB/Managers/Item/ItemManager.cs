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
        }, PrefabManager.GetAllPrefabNames<ItemDrop>);
        
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

            if (LoadManager.GetOriginal<BaseItem>(prefabName) is not {} item)
            {
                MonsterDBPlugin.LogInfo("Original data not found");
                return true;
            }
            
            item.Update();
            LoadManager.files.Add(item);
            LoadManager.UpdateSync();
            
            return true;
        }, optionsFetcher: LoadManager.GetOriginalKeys<BaseItem>, adminOnly: true);

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
            
            TryClone(prefab, newName, out _);
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

    public static bool TrySave(GameObject prefab, out BaseItem item, bool isClone = false, string source = "")
    {
        item = LoadManager.GetOriginal<BaseItem>(prefab.name);
        if (item != null) return true;

        if (!prefab.GetComponent<ItemDrop>()) return false;
        
        item = new BaseItem();
        item.Setup(prefab, isClone, source);
        
        LoadManager.originals.Add(prefab.name, item);

        return true;
    }

    public static void Write(GameObject prefab, bool isClone = false, string clonedFrom = "", string dirPath = "")
    {
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filePath = Path.Combine(dirPath, prefab.name + ".yml");

        if (TrySave(prefab, out BaseItem item, isClone, clonedFrom))
        {
            string text = ConfigManager.Serialize(item);
            File.WriteAllText(filePath, text);
            MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
        }

        if (prefab.TryGetComponent(out ItemDrop itemDrop))
        {
            var sharedData = itemDrop.m_itemData.m_shared;
            if (sharedData.m_attack.m_attackProjectile != null)
            {
                bool isProjectileClone = false;
                string projectileSource = "";
                if (PrefabManager.Clones.TryGetValue(sharedData.m_attack.m_attackProjectile.name, out Clone c))
                {
                    isProjectileClone = true;
                    projectileSource = c.PrefabName;
                }

                ProjectileManager.Write(sharedData.m_attack.m_attackProjectile, isProjectileClone, projectileSource, dirPath);
            }

            if (sharedData.m_secondaryAttack.m_attackProjectile != null)
            {
                bool isProjectileClone = false;
                string projectileSource = "";
                if (PrefabManager.Clones.TryGetValue(sharedData.m_secondaryAttack.m_attackProjectile.name, out Clone c))
                {
                    isProjectileClone = true;
                    projectileSource = c.PrefabName;
                }

                ProjectileManager.Write(sharedData.m_secondaryAttack.m_attackProjectile, isProjectileClone, projectileSource, dirPath);
            }
        }
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
            LoadManager.files.Add(reference);
            LoadManager.UpdateSync();
        }
        catch
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
        }
    }

    public static bool TryClone(GameObject source, string cloneName, out GameObject clone, bool write = true, string dirPath = "")
    {
        if (CloneManager.clones.TryGetValue(cloneName, out clone)) return true;
        if (!source.GetComponent<ItemDrop>()) return false;
        
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

            ItemDrop? itemDrop = p.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                ItemDrop.ItemData.SharedData? sharedData = itemDrop.m_itemData.m_shared;
                GameObject? projectile = sharedData.m_attack.m_attackProjectile;
                if (projectile != null)
                {
                    string newProjName = $"MDB_{cloneName}_{projectile.name}";
                    if (ProjectileManager.TryClone(projectile, newProjName, out GameObject newProjectile, false, dirPath))
                    {
                        sharedData.m_attack.m_attackProjectile = newProjectile;
                    }
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