using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class CreatureManager
{
    public static void Setup()
    {
        Command save = new Command("write", 
            $"[prefabName]: save creature YML to {FileManager.ExportFolder} folder", 
            args =>
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
                
            Write(prefab);

            return true;
        }, optionsFetcher: PrefabManager.GetAllPrefabNames<Character>);
        
        Command saveAll = new Command("write_all", 
            $"save all creatures YML to {FileManager.ExportFolder} folder", 
            _ =>
        {
            List<GameObject> prefabs = PrefabManager.GetAllPrefabs<Character>();
            for (int i = 0; i < prefabs.Count; ++i)
            {
                GameObject? prefab = prefabs[i];
                Write(prefab);
            }
            return true;
        });

        Command read = new Command("mod", 
            $"[fileName]: read YML file from {FileManager.ImportFolder} and update", 
            args =>
        {
            if (args.Length < 3)
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            
            string fileName = args[2];
            if (string.IsNullOrEmpty(fileName))
            {
                MonsterDBPlugin.LogWarning("Invalid parameters");
                return true;
            }
            
            string filePath = Path.Combine(FileManager.ImportFolder, fileName + ".yml");
            FileManager.Read(filePath);
            return true;
        }, FileManager.GetModFileNames, adminOnly: true);

        Command revert = new Command("revert", 
            "[prefabName]: revert creature to factory settings", 
            args =>
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

            if (SyncManager.GetOriginal<Base>(prefabName) is not { } creature)
            {
                MonsterDBPlugin.LogInfo("Original data not found");
                return true;
            }
            creature.Update();
            string text = ConfigManager.Serialize(creature);
            SyncManager.rawFiles[creature.Prefab] = text;
            SyncManager.UpdateSync();
            return true;
        }, SyncManager.GetOriginalKeys<Base>, adminOnly: true);

        Command clone = new Command("clone", 
            "[prefabName][newName]: must be a character", 
            args =>
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

            if (!prefab.GetComponent<Character>())
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing character component");
                return true;
            }
            Clone(prefab, newName);
            return true;
        }, optionsFetcher: PrefabManager.GetAllPrefabNames<Character>, adminOnly: true);
    }

    public static bool TrySave(GameObject prefab, out Base? data, bool isClone = false, string source = "")
    {
        data = SyncManager.GetOriginal<Base>(prefab.name);
        if (data != null) return true;
        
        Character? character = prefab.GetComponent<Character>();
        BaseAI ai = prefab.GetComponent<BaseAI>();
        if (character == null || ai == null)
        {
            MonsterDBPlugin.LogWarning("Invalid prefab, missing Character or AI component");
            return false;
        }
        
        if (character is Human && ai is MonsterAI)
        {
            data = new BaseHuman();
            data.Setup(prefab, isClone, source);

        }
        else if (character is Humanoid && ai is MonsterAI)
        {
            data = new BaseHumanoid();
            data.Setup(prefab, isClone, source);
        }
        else if (ai is AnimalAI)
        {
            data = new BaseCharacter();
            data.Setup(prefab,  isClone, source);
        }

        if (data == null) return false;
        
        SyncManager.originals.Add(prefab.name, data);

        if (isClone && ZNet.instance && ZNet.instance.IsServer())
        {
            SyncManager.rawFiles[data.Prefab] = ConfigManager.Serialize(data);
            SyncManager.UpdateSync();
        }

        return true;
    }

    public static void Write(GameObject prefab, bool isClone = false, string source = "")
    {
        string folder = Path.Combine(FileManager.ExportFolder, prefab.name);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string filePath = Path.Combine(folder, prefab.name + ".yml");

        if (!TrySave(prefab, out Base? data, isClone, source)) return;
        
        string text = ConfigManager.Serialize(data);
        
        if (prefab.TryGetComponent(out Humanoid humanoid) && humanoid is not Human)
        {
            HashSet<GameObject> items = humanoid.GetItems();
            if (items.Count > 0)
            {
                string itemFolder = Path.Combine(folder, "items");
                if (!Directory.Exists(itemFolder)) Directory.CreateDirectory(itemFolder);
                foreach (GameObject? item in items)
                {
                    bool isItemClone = false;
                    string itemSource = "";
                    
                    if (PrefabManager.Clones.TryGetValue(item.name, out Clone d))
                    {
                        isItemClone = true;
                        itemSource = d.PrefabName;
                    }

                    if (ItemManager.TrySave(item, out BaseItem baseItem, isItemClone, itemSource))
                    {
                        string info = ConfigManager.Serialize(baseItem);
                        string itemPath = Path.Combine(itemFolder, item.name + ".yml");
                        File.WriteAllText(itemPath, info);
                    }

                    if (item.TryGetComponent(out ItemDrop component))
                    {
                        if (component.m_itemData.m_shared.m_attack.m_attackProjectile != null)
                        {
                            string pName = component.m_itemData.m_shared.m_attack.m_attackProjectile.name;
                            bool isProjectileClone = false;
                            string projectileSource = "";

                            if (PrefabManager.Clones.TryGetValue(pName, out Clone pD))
                            {
                                isProjectileClone = true;
                                projectileSource = pD.PrefabName;
                            }
                            
                            if (ProjectileManager.TrySave(component.m_itemData.m_shared.m_attack.m_attackProjectile, out BaseProjectile bp, isProjectileClone, projectileSource))
                            {
                                string pInfo = ConfigManager.Serialize(bp);
                                string pPath = Path.Combine(itemFolder, pName + ".yml");
                                File.WriteAllText(pPath, pInfo);
                            }
                        }

                        if (component.m_itemData.m_shared.m_secondaryAttack.m_attackProjectile != null)
                        {
                            string pName = component.m_itemData.m_shared.m_secondaryAttack.m_attackProjectile.name;
                            bool isProjectileClone = false;
                            string projectileSource = "";

                            if (PrefabManager.Clones.TryGetValue(pName, out Clone pD))
                            {
                                isProjectileClone = true;
                                projectileSource = pD.PrefabName;
                            }
                            
                            if (ProjectileManager.TrySave(component.m_itemData.m_shared.m_secondaryAttack.m_attackProjectile, out BaseProjectile bp, isProjectileClone, projectileSource))
                            {
                                string pInfo = ConfigManager.Serialize(bp);
                                string pPath = Path.Combine(itemFolder, pName + ".yml");
                                File.WriteAllText(pPath, pInfo);
                            }
                        }
                    }
                }
            }
        }
        File.WriteAllText(filePath, text);
        MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
    }

    public static void Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.clones.ContainsKey(cloneName)) return;
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += prefab =>
        {
            Character? character = prefab.GetComponent<Character>();
            bool isPlayer = false;
            if (character is Player)
            {
                Player? player = source.GetComponent<Player>();
                prefab.Remove<PlayerController>();
                prefab.Remove<Talker>();
                prefab.Remove<Skills>();
                prefab.Remove<Player>();
                Human? human = prefab.AddComponent<Human>();
                human.CopyFrom(player);
                MonsterAI? ai = prefab.AddComponent<MonsterAI>();
                MonsterAI? dverger = PrefabManager.GetPrefab("Dverger")!.GetComponent<MonsterAI>();
                ai.CopyFrom(dverger);
                prefab.AddComponent<CharacterDrop>();
                prefab.GetComponent<ZNetView>().m_persistent = true;
                human.m_eye = Utils.FindChild(prefab.transform, "EyePos");
                character = human;
                isPlayer = true;
            }

            Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>(true);
            Dictionary<string, Material> newMaterials = new Dictionary<string, Material>();
            
            for (int i = 0; i < renderers.Length; ++i)
            {
                Renderer renderer = renderers[i];
                CloneMaterials(renderer, ref  newMaterials);
            }

            if (write && !isPlayer && character is Humanoid humanoid)
            {
                Dictionary<string, GameObject> newItems = new();
                if (humanoid.m_defaultItems != null)
                {
                    humanoid.m_defaultItems = CreateItems(humanoid.m_defaultItems, ref newItems);
                }

                if (humanoid.m_randomWeapon != null)
                {
                    humanoid.m_randomWeapon = CreateItems(humanoid.m_randomWeapon, ref newItems);
                }

                if (humanoid.m_randomArmor != null)
                {
                    humanoid.m_randomArmor = CreateItems(humanoid.m_randomArmor, ref newItems);
                }

                if (humanoid.m_randomShield != null)
                {
                    humanoid.m_randomShield = CreateItems(humanoid.m_randomShield, ref newItems);
                }

                if (humanoid.m_randomItems != null)
                {
                    List<Humanoid.RandomItem> newRandomItems = new();
                    foreach (Humanoid.RandomItem? item in humanoid.m_randomItems)
                    {
                        if (TryCreateItem(item.m_prefab, out GameObject? newItem, ref newItems) && newItem != null)
                        {
                            Humanoid.RandomItem newRandomItem = new Humanoid.RandomItem();
                            newRandomItem.m_chance = item.m_chance;
                            newRandomItem.m_prefab = newItem;
                            newRandomItems.Add(newRandomItem);
                        }
                    }

                    humanoid.m_randomItems = newRandomItems.ToArray();
                }

                if (humanoid.m_randomSets != null)
                {
                    List<Humanoid.ItemSet> newRandomSets = new List<Humanoid.ItemSet>();
                    foreach (Humanoid.ItemSet? set in humanoid.m_randomSets)
                    {
                        Humanoid.ItemSet newSet = new Humanoid.ItemSet();
                        newSet.m_name = set.m_name;
                        newSet.m_items = CreateItems(set.m_items, ref newItems);
                        newRandomSets.Add(newSet);
                    }
                    humanoid.m_randomSets = newRandomSets.ToArray();
                }
            }

            if (character.m_deathEffects != null)
            {
                EffectList.EffectData[]? deathEffects = character.m_deathEffects.m_effectPrefabs;
                for (int index = 0; index < deathEffects.Length; index++)
                {
                    EffectList.EffectData effect = deathEffects[index];
                    if (effect.m_prefab != null && effect.m_prefab.GetComponent<Ragdoll>())
                    {
                        string newRagdollName = $"MDB_{cloneName}_ragdoll";
                        if (CloneManager.clones.TryGetValue(newRagdollName, out GameObject cl))
                        {
                            effect.m_prefab = cl;
                        }
                        else
                        {
                            Clone newRagdoll = new Clone(effect.m_prefab, newRagdollName);
                            newRagdoll.OnCreated += p =>
                            {
                                if (p.TryGetComponent(out Ragdoll rag))
                                {
                                    List<Material> mats = new();
                                    if (rag.m_mainModel != null)
                                    {
                                        for (int i = 0; i < rag.m_mainModel.sharedMaterials.Length; ++i)
                                        {
                                            Material? mat = rag.m_mainModel.sharedMaterials[i];
                                            if (mat == null) continue;
                                            string name = $"MDB_{cloneName}_{mat.name.Replace("(Instance)", string.Empty)}";
                                            if (!newMaterials.TryGetValue(name, out var material))
                                            {
                                                mats.Add(mat);
                                            }
                                            else
                                            {
                                                mats.Add(material);
                                            }
                                        }
                                        rag.m_mainModel.sharedMaterials = mats.ToArray();
                                        rag.m_mainModel.materials = mats.ToArray();
                                    }
                                }
                                effect.m_prefab = p;
                            };
                            newRagdoll.Create();
                        }
                        break;
                    }
                }
            }
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(prefab, true, source.name);
            }

            return;

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
        };

        c.Create();

        return;

        bool TryCreateItem(GameObject item, out GameObject? newItem, ref Dictionary<string, GameObject> clones)
        {
            string newItemName = $"MDB_{cloneName}_{item.name}";
            if (clones.TryGetValue(newItemName, out newItem)) return newItem;
            if (CloneManager.clones.TryGetValue(newItemName, out GameObject cl))
            {
                newItem = cl;
                clones[newItemName] = cl;
                return true;
            }
            
            GameObject? projectile = null;
                
            if (item.TryGetComponent(out ItemDrop id))
            {
                if (id.m_itemData.m_shared.m_attack.m_attackProjectile != null)
                {
                    string projName = $"MDB_{cloneName}_{id.m_itemData.m_shared.m_attack.m_attackProjectile.name}";
                    if (clones.TryGetValue(projName, out GameObject proj) || CloneManager.clones.TryGetValue(projName, out proj))
                    {
                        projectile = proj;
                    }
                    else
                    {
                        Clone mockProjectile = new Clone(id.m_itemData.m_shared.m_attack.m_attackProjectile, projName);
                        mockProjectile.OnCreated += p =>
                        {
                            projectile = p;
                        };
                        mockProjectile.Create();
                        if (projectile != null)
                        {
                            clones[projName] = projectile;
                        }
                    }
                }
            }

            Clone mock = new Clone(item, newItemName);
            GameObject? clone = null;
                
            mock.OnCreated += prefab =>
            {
                clone = prefab;
                if (prefab.TryGetComponent(out ItemDrop component))
                {
                    if (projectile != null)
                    {
                        component.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;
                    }
                }
            };
                
            mock.Create();

            newItem = clone;
            if (newItem != null)
            {
                clones[newItemName] = newItem;
            }

            return newItem != null;
        }
        
        GameObject[] CreateItems(GameObject[] list, ref Dictionary<string, GameObject> clones)
        {
            List<GameObject> newItems = new();
            foreach (GameObject item in list)
            {
                if (item == null) continue;
                if (TryCreateItem(item, out GameObject? newItem, ref clones) && newItem != null)
                {
                    newItems.Add(newItem);
                    clones[newItem.name] = newItem;
                }
            }

            return newItems.ToArray();
        }
    }
}