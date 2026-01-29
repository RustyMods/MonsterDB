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
            string prefabName = args.GetString(2);
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
            string prefabName = args.GetString(2);
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
            string prefabName = args.GetString(2);
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

            string prefabName = args.GetString(2);
            string newName = args.GetString(3);
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
            string query = args.GetString(2);
            List<string> names = PrefabManager.SearchCache<ItemDrop>(query);
            for (int i = 0; i < names.Count; ++i)
            {
                MonsterDBPlugin.LogInfo(names[i]);
            }
            
            return true;
        }, () => PrefabManager.SearchCache<ItemDrop>(""));

        Command create = new Command("create_item", "[prefabName]: create item from non-item prefabs", args =>
        {
            string prefabName = args.GetString(2);
            string newName = args.GetString(3);
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

            if (prefab.GetComponent<Character>())
            {
                MonsterDBPlugin.LogWarning("Cannot convert creature into an item");
                return true;
            }

            if (prefab.GetComponent<ItemDrop>())
            {
                TryClone(prefab, newName, out _);
                return true;
            }

            if (!prefab.GetComponent<ZNetView>())
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing ZNetView");
                return true;
            }

            var colliders = prefab.GetComponentInChildren<Collider>();
            if (colliders == null)
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing colliders");
                return true;
            }

            TryCreateItem(prefab, newName, out _);
            return true;
        }, PrefabManager.GetAllPrefabNames<ZNetView>, adminOnly: true);
    }

    public static bool TrySave(GameObject prefab, out BaseItem item, bool isClone = false, string source = "")
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        item = LoadManager.GetOriginal<BaseItem>(prefab.name);
#pragma warning restore CS8601 // Possible null reference assignment.
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
            ItemDrop.ItemData.SharedData? sharedData = itemDrop.m_itemData.m_shared;
            if (sharedData.m_attack.m_attackProjectile != null)
            {
                bool isProjectileClone = false;
                string projectileSource = "";
                if (CloneManager.clones.TryGetValue(sharedData.m_attack.m_attackProjectile.name, out Clone c))
                {
                    isProjectileClone = true;
                    projectileSource = c.SourceName;
                }

                ProjectileManager.Write(sharedData.m_attack.m_attackProjectile, isProjectileClone, projectileSource, dirPath);
            }

            if (sharedData.m_secondaryAttack.m_attackProjectile != null)
            {
                bool isProjectileClone = false;
                string projectileSource = "";
                if (CloneManager.clones.TryGetValue(sharedData.m_secondaryAttack.m_attackProjectile.name, out Clone c))
                {
                    isProjectileClone = true;
                    projectileSource = c.SourceName;
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
        if (CloneManager.prefabs.TryGetValue(cloneName, out clone)) return true;
        if (!source.GetComponent<ItemDrop>()) return false;
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
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
#pragma warning disable CS8601 // Possible null reference assignment.
        clone = c.Create();
#pragma warning restore CS8601 // Possible null reference assignment.
        return clone != null;
    }

    public static bool TryCreateItem(GameObject source, string cloneName, out GameObject clone, bool write = true,
        string dirPath = "")
    {
        if (CloneManager.prefabs.TryGetValue(cloneName, out clone)) return true;
        if (source.GetComponent<ItemDrop>() || !source.GetComponent<ZNetView>()) return false;

        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            MonoBehaviour[]? components = p.GetComponents<MonoBehaviour>();
            for (int i = 0; i < components.Length; ++i)
            {
                MonoBehaviour component = components[i];
                if (component is ZNetView) continue;
                Object.Destroy(component);
            }
            
            ItemDrop? item = p.AddComponent<ItemDrop>();
            GameObject? eggPrefab = PrefabManager.GetPrefab("AsksvinEgg");
            if (eggPrefab == null) return;
            if (!eggPrefab.TryGetComponent(out ItemDrop egg)) return;
            Sprite[]? icons = egg.m_itemData.m_shared.m_icons;
            Transform? notGrowing = Utils.FindChild(eggPrefab.transform, "Not Growing");
            if (notGrowing != null)
            {
                var itemParticles = Object.Instantiate(notGrowing, p.transform);
                itemParticles.name = "Not Growing";
            }
            item.m_itemData = new ItemDrop.ItemData
            {
                m_shared = new ItemDrop.ItemData.SharedData
                {
                    m_name = cloneName,
                    m_icons = icons,
                    m_attack = new Attack(),
                    m_secondaryAttack = new Attack()
                },
            };

            if (!p.TryGetComponent(out ZSyncTransform zSyncTransform))
            {
                zSyncTransform = p.AddComponent<ZSyncTransform>();
                zSyncTransform.m_syncPosition = true;
                zSyncTransform.m_syncRotation = true;
            }

            if (!p.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody = p.AddComponent<Rigidbody>();
                rigidbody.mass = 1;
                rigidbody.linearDamping = 0;
                rigidbody.angularDamping = 0.05f;
                rigidbody.useGravity = true;
                rigidbody.isKinematic = false;
                rigidbody.interpolation = RigidbodyInterpolation.None;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            
            var colliders = p.GetComponentsInChildren<Collider>();
            var itemLayer = LayerMask.NameToLayer("item");
            for (int i = 0; i < colliders.Length; ++i)
            {
                var collider = colliders[i];
                collider.gameObject.layer = itemLayer;
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