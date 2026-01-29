using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            string fileName = args.GetString(2);
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
            string prefabName = args.GetString(2);
            if (string.IsNullOrEmpty(prefabName))
            {
                MonsterDBPlugin.LogInfo("Invalid prefab");
                return true;
            }

            if (LoadManager.GetOriginal<Base>(prefabName) is not { } creature)
            {
                MonsterDBPlugin.LogInfo("Original data not found");
                return true;
            }
            creature.Update();
            LoadManager.UpdateSync();
            return true;
        }, LoadManager.GetOriginalKeys<Base>, adminOnly: true);

        Command clone = new Command("clone", 
            "[prefabName][newName]: must be a character", 
            args =>
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
            
            if (!prefab.GetComponent<Character>())
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing character component");
                return true;
            }
            Clone(prefab, newName);
            return true;
        }, optionsFetcher: PrefabManager.GetAllPrefabNames<Character>, adminOnly: true);

        Command hierarchy = new Command("export_bones", "[prefabName]: export creature hierarchy", args =>
        {
            string prefabName = args.GetString(2);
            if (string.IsNullOrEmpty(prefabName))
            {
                MonsterDBPlugin.LogInfo("Invalid parameters");
                return true;
            }

            GameObject? prefab = PrefabManager.GetPrefab(prefabName);
            if (prefab == null)
            {
                MonsterDBPlugin.LogWarning($"Failed to find prefab {prefabName}");
                return true;
            }

            string folderPath = Path.Combine(FileManager.ExportFolder, prefabName);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, $"{prefabName}.bones.yml");
            ExportHierarchy(prefab, filePath);
            return true;
        }, optionsFetcher: PrefabManager.GetAllPrefabNames<Character>);
    }

    public static bool TrySave(GameObject prefab, out Base? data, bool isClone = false, string source = "")
    {
        data = LoadManager.GetOriginal<Base>(prefab.name);
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
            BaseHuman human = new BaseHuman();
            human.Setup(prefab, isClone, source);
            data = human;

        }
        else if (character is Humanoid && ai is MonsterAI)
        {
            BaseHumanoid humanoid = new BaseHumanoid();
            humanoid.Setup(prefab, isClone, source);
            data = humanoid;
        }
        else if (ai is AnimalAI)
        {
            var chara = new BaseCharacter();
            chara.Setup(prefab,  isClone, source);
            data = chara;
        }

        if (data == null) return false;
        
        LoadManager.originals.Add(prefab.name, data);
        return true;
    }

    public static void Write(GameObject prefab, bool isClone = false, string source = "")
    {
        string folder = Path.Combine(FileManager.ExportFolder, prefab.name);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string filePath = Path.Combine(folder, prefab.name + ".yml");

        if (!TrySave(prefab, out Base? data, isClone, source)) return;
        
        string text = ConfigManager.Serialize(data);
        File.WriteAllText(filePath, text);
        MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
        
        Character? character = prefab.GetComponent<Character>();
        if (character != null)
        {
            if (character.m_deathEffects != null && character.m_deathEffects.m_effectPrefabs != null)
            {
                for (int i = 0; i < character.m_deathEffects.m_effectPrefabs.Length; ++i)
                {
                    EffectList.EffectData? effect = character.m_deathEffects.m_effectPrefabs[i];
                    if (effect.m_prefab != null && effect.m_prefab.GetComponent<Ragdoll>())
                    {
                        bool isRagdollClone = false;
                        string ragdollSource = "";
                        if (CloneManager.clones.TryGetValue(effect.m_prefab.name, out Clone ragdollClone))
                        {
                            isRagdollClone = true;
                            ragdollSource = ragdollClone.PrefabName;
                        }
                        RagdollManager.Write(effect.m_prefab, isRagdollClone, ragdollSource, folder);
                    }
                }
            }
            
            if (character is Humanoid humanoid and not Human)
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
                        
                        if (CloneManager.clones.TryGetValue(item.name, out Clone d))
                        {
                            isItemClone = true;
                            itemSource = d.PrefabName;
                        }

                        ItemManager.Write(item, isItemClone, itemSource, itemFolder);
                    }
                }
            }
        }
    }

    private static void CloneRagdoll(Character character, string cloneName)
    {
        EffectList.EffectData[]? deathEffects = character.m_deathEffects.m_effectPrefabs;
        for (int index = 0; index < deathEffects.Length; index++)
        {
            EffectList.EffectData effect = deathEffects[index];
            if (effect.m_prefab != null && effect.m_prefab.GetComponent<Ragdoll>())
            {
                string newRagdollName = $"{cloneName}_ragdoll";
                if (RagdollManager.TryClone(effect.m_prefab, newRagdollName, out GameObject newRagdoll, false))
                {
                    effect.m_prefab = newRagdoll;
                }
                break;
            }
        }
    }

    private static GameObject? ClonePlayer(GameObject source, Player player, string cloneName, bool write = true)
    {
        if (CloneManager.prefabs.TryGetValue(cloneName, out var clone)) return clone;
        Clone c = new Clone(source, cloneName);
        c.OnCreated += prefab =>
        {
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

            if (write && human.m_deathEffects != null)
            {
                CloneRagdoll(human, cloneName);
            }
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(prefab, true, source.name);
            }
        };

        return c.Create();
    }

    private static void CloneItems(Humanoid humanoid, string cloneName)
    {
        if (humanoid.m_defaultItems != null)
        {
            humanoid.m_defaultItems = CreateItems(humanoid.m_defaultItems, cloneName);
        }

        if (humanoid.m_randomWeapon != null)
        {
            humanoid.m_randomWeapon = CreateItems(humanoid.m_randomWeapon, cloneName);
        }

        if (humanoid.m_randomArmor != null)
        {
            humanoid.m_randomArmor = CreateItems(humanoid.m_randomArmor, cloneName);
        }

        if (humanoid.m_randomShield != null)
        {
            humanoid.m_randomShield = CreateItems(humanoid.m_randomShield, cloneName);
        }

        if (humanoid.m_randomItems != null)
        {
            List<Humanoid.RandomItem> newRandomItems = new();
            foreach (Humanoid.RandomItem? item in humanoid.m_randomItems)
            {
                string newItemName = $"MDB_{cloneName}_{item.m_prefab.name}";
                if (ItemManager.TryClone(item.m_prefab, newItemName, out GameObject newItem, false))
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
                newSet.m_items = CreateItems(set.m_items, cloneName);
                newRandomSets.Add(newSet);
            }
            humanoid.m_randomSets = newRandomSets.ToArray();
        }
    }

    public static GameObject? Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.prefabs.TryGetValue(cloneName, out var clone)) return clone;
        if (source.TryGetComponent(out Player player))
        {
            return ClonePlayer(source, player, cloneName, write);
        }
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += prefab =>
        {
            Character? character = prefab.GetComponent<Character>();

            if (write && character is Humanoid humanoid)
            {
                CloneItems(humanoid, cloneName);
            }

            if (write && character.m_deathEffects != null)
            {
                CloneRagdoll(character, cloneName);
            }
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(prefab, true, source.name);
            }
        };

        return c.Create();
    }
    
    private static GameObject[] CreateItems(GameObject[] list, string cloneName)
    {
        List<GameObject> newItems = new();
        foreach (GameObject item in list)
        {
            if (item == null) continue;
                
            string newItemName = $"MDB_{cloneName}_{item.name}";
            if (ItemManager.TryClone(item, newItemName, out GameObject newItem, false))
            {
                newItems.Add(newItem);
            }
        }

        return newItems.ToArray();
    }

    private static void ExportHierarchy(GameObject root, string filePath)
    {
        if (root == null) return;

        if (string.IsNullOrEmpty(filePath))
            filePath = Path.Combine(FileManager.ExportFolder, $"{root.name}.bones.yml");

        StringBuilder sb = new();
        sb.AppendLine(root.name);
        for (int i = 0; i < root.transform.childCount; ++i)
        {
            WriteTransform(sb, root.transform.GetChild(i), 1);
        }

        File.WriteAllText(filePath, sb.ToString());
        MonsterDBPlugin.LogInfo($"Saved {root.name} hierarchy to {filePath}");

        return;
        
        void WriteTransform(StringBuilder builder, Transform t, int depth)
        {
            builder.Append(new string(' ', depth * 2)); // 2 spaces per depth
            builder.Append("- ");
            builder.AppendLine(t.name);

            for (int i = 0; i < t.childCount; i++)
            {
                WriteTransform(builder, t.GetChild(i), depth + 1);
            }
        }
    }
}