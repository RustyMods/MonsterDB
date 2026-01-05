using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace MonsterDB;

public static class CreatureManager
{
    public static readonly string SaveFolder;
    public static readonly string ModifiedFolder;
    
    static CreatureManager()
    {
        SaveFolder = Path.Combine(ConfigManager.DirectoryPath, "Save");
        ModifiedFolder = Path.Combine(ConfigManager.DirectoryPath, "Modified");

        if (!Directory.Exists(SaveFolder)) Directory.CreateDirectory(SaveFolder);
        if (!Directory.Exists(ModifiedFolder)) Directory.CreateDirectory(ModifiedFolder);
        Start();
    }
    
    private static void Start()
    {
        string[] files =  Directory.GetFiles(ModifiedFolder, "*.yml", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            string text = File.ReadAllText(filePath);
            try
            {
                Header header = ConfigManager.Deserialize<Header>(text);
                switch (header.Type)
                {
                    case CreatureType.Character:
                        CharacterCreature character = ConfigManager.Deserialize<CharacterCreature>(text);
                        SyncManager.loadList.Add(character);
                        SyncManager.rawFiles[character.Prefab] = text;
                        break;
                    case CreatureType.Humanoid:
                        HumanoidCreature humanoid = ConfigManager.Deserialize<HumanoidCreature>(text);
                        SyncManager.loadList.Add(humanoid);
                        SyncManager.rawFiles[humanoid.Prefab] = text;
                        break;
                    case CreatureType.Human:
                        PlayerCreature player = ConfigManager.Deserialize<PlayerCreature>(text);
                        SyncManager.loadList.Add(player);
                        SyncManager.rawFiles[player.Prefab] = text;
                        break;
                    case CreatureType.Egg:
                        BaseEgg data = ConfigManager.Deserialize<BaseEgg>(text);
                        SyncManager.loadList.Add(data);
                        SyncManager.rawFiles[data.Prefab] = text;
                        break;
                }

            }
            catch (Exception ex)
            {
                MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
                MonsterDBPlugin.LogDebug(ex.Message);
            }
        }
        MonsterDBPlugin.LogInfo($"Loaded {files.Length} modified creature files.");
    }
    
    public static void Setup()
    {
        Command save = new Command("write", "[prefabName]: save creature reference to Save folder", args =>
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

        Command saveAll = new Command("write_all", "save all creatures to Save folder", _ =>
        {
            List<GameObject> prefabs = PrefabManager.GetAllPrefabs<Character>();
            for (int i = 0; i < prefabs.Count; ++i)
            {
                GameObject? prefab = prefabs[i];
                Write(prefab);
            }
            return true;
        });

        Command read = new Command("mod", "[prefabName]: read creature file from Modified folder and update", args =>
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
            
            string filePath = Path.Combine(ModifiedFolder, prefabName + ".yml");
            Read(filePath);
            return true;
        }, () => Directory.GetFiles(ModifiedFolder, "*.yml", SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension)
            .ToList(), 
            adminOnly: true);

        Command revert = new Command("revert", "[prefabName]: revert creature to factory settings", args =>
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

            if (SyncManager.GetOriginal<BaseCreature>(prefabName) is not { } creature)
            {
                MonsterDBPlugin.LogInfo("Original data not found");
                return true;
            }
            creature.Update();
            string text = ConfigManager.Serialize(creature);
            SyncManager.rawFiles[creature.Prefab] = text;
            SyncManager.UpdateSync();
            return true;
        }, () => SyncManager.originals.Keys.ToList(), adminOnly: true);

        Command clone = new Command("clone", "[prefabName][newName]: must be a character", args =>
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

    public static string? Save(GameObject prefab, bool isClone = false, string source = "")
    {
        Character? character = prefab.GetComponent<Character>();
        BaseAI ai = prefab.GetComponent<BaseAI>();
        if (character == null || ai == null) return null;

        string? text = null;

        if (character is Human && ai is MonsterAI)
        {
            if (SyncManager.GetOriginal<PlayerCreature>(prefab.name) is { } original)
            {
                return ConfigManager.serializer.Serialize(original);
            } 
            PlayerCreature creature = new PlayerCreature();
            creature.Setup(prefab,isClone, source);
            if (!SyncManager.originals.ContainsKey(prefab.name)) SyncManager.originals.Add(prefab.name, creature);
            text = ConfigManager.Serialize(creature);
            if (isClone && (ZNet.instance?.IsServer() ?? false))
            {
                SyncManager.rawFiles[creature.Prefab] = text;
                SyncManager.UpdateSync();
            }
        }
        else if (character is Humanoid && ai is MonsterAI)
        {
            if (SyncManager.GetOriginal<HumanoidCreature>(prefab.name) is { } original)
            {
                return ConfigManager.Serialize(original);
            }
            HumanoidCreature creature = new HumanoidCreature();
            creature.Setup(prefab, isClone, source);
            if (!SyncManager.originals.ContainsKey(prefab.name)) SyncManager.originals.Add(prefab.name, creature);
            text = ConfigManager.Serialize(creature);
            if (isClone && (ZNet.instance?.IsServer() ?? false))
            {
                SyncManager.rawFiles[creature.Prefab] = text;
                SyncManager.UpdateSync();
            }
        }
        else if (ai is AnimalAI)
        {
            if (SyncManager.GetOriginal<CharacterCreature>(prefab.name) is { } original)
            {
                return ConfigManager.serializer.Serialize(original);
            }
            CharacterCreature creature = new CharacterCreature();
            creature.Setup(prefab,  isClone, source);
            if (!SyncManager.originals.ContainsKey(prefab.name)) SyncManager.originals.Add(prefab.name, creature);
            text = ConfigManager.Serialize(creature);
            if (isClone && (ZNet.instance?.IsServer() ?? false))
            {
                SyncManager.rawFiles[creature.Prefab] = text;
                SyncManager.UpdateSync();
            }
        }
        return text;
    }

    private static void Write(GameObject prefab, bool isClone = false, string source = "")
    {
        string filePath = Path.Combine(SaveFolder, prefab.name + ".yml");
        
        string? text = Save(prefab, isClone, source);

        if (string.IsNullOrEmpty(text)) return;
        
        File.WriteAllText(filePath, text);
        MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filePath}");
    }

    public static void Read(string filePath)
    {
        if (!File.Exists(filePath)) return;
        
        string text = File.ReadAllText(filePath);
        try
        {
            Header header = ConfigManager.Deserialize<Header>(text);
            switch (header.Type)
            {
                case CreatureType.Humanoid:
                {
                    HumanoidCreature data = ConfigManager.Deserialize<HumanoidCreature>(text);
                    data.Update();
                    SyncManager.rawFiles[data.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                }
                case CreatureType.Character:
                {
                    CharacterCreature data = ConfigManager.Deserialize<CharacterCreature>(text);
                    data.Update();
                    SyncManager.rawFiles[data.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                }
                case CreatureType.Human:
                {
                    PlayerCreature data = ConfigManager.Deserialize<PlayerCreature>(text);
                    data.Update();
                    SyncManager.rawFiles[data.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
                }
                case CreatureType.Egg:
                    BaseEgg reference = ConfigManager.Deserialize<BaseEgg>(text);
                    reference.Update();
                    SyncManager.rawFiles[reference.Prefab] = text;
                    SyncManager.UpdateSync();
                    break;
            }
        }
        catch (Exception ex)
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
            MonsterDBPlugin.LogDebug(ex.Message);
        }
    }

    public static void Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.clones.ContainsKey(cloneName)) return;
        
        Clone clone = new Clone(source, cloneName);
        clone.OnCreated += prefab =>
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
            
            Renderer? renderer = null;
            LevelEffects? lvlEffects = prefab.GetComponentInChildren<LevelEffects>();
            if (lvlEffects != null) renderer = lvlEffects.m_mainRender;
            if (renderer == null && prefab.TryGetComponent(out VisEquipment visEq)) renderer = visEq.m_bodyModel;
            if (renderer == null) renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            Material[]? newMaterials = null;
            if (renderer != null)
            {
                List<Material> mats = new();
                foreach (Material? mat in renderer.sharedMaterials)
                {
                    Material material = new Material(mat);
                    material.name = $"MDB_{cloneName}_{mat.name}";
                    mats.Add(material);
                }

                newMaterials = mats.ToArray();
                renderer.sharedMaterials = newMaterials;
                renderer.materials = newMaterials;
            }
            
            if (!isPlayer && character is Humanoid humanoid)
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
                        Humanoid.RandomItem newRandomItem = new Humanoid.RandomItem();
                        newRandomItem.m_chance = item.m_chance;
                        string newItemName = $"MDB_{cloneName}_{item.m_prefab.name}";
                        if (newItems.TryGetValue(newItemName, out var go))
                        {
                            newRandomItem.m_prefab = go;
                        }
                        else
                        {
                            Clone mock = new Clone(item.m_prefab, newItemName);
                            mock.OnCreated += m =>
                            {
                                go = m;
                            };
                            mock.Create();
                            newItems[newItemName] = go;
                            newRandomItems.Add(newRandomItem);
                        }
                    }

                    humanoid.m_randomItems = newRandomItems.ToArray();
                }

                if (humanoid.m_randomSets != null)
                {
                    List<Humanoid.ItemSet> newRandomSets = new List<Humanoid.ItemSet>();
                    foreach (var set in humanoid.m_randomSets)
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
                        Clone newRagdoll = new Clone(effect.m_prefab, $"MDB_{cloneName}_ragdoll");
                        newRagdoll.OnCreated += p =>
                        {
                            if (newMaterials != null && p.TryGetComponent(out Ragdoll rag))
                            {
                                if (rag.m_mainModel != null)
                                {
                                    rag.m_mainModel.sharedMaterials = newMaterials;
                                    rag.m_mainModel.materials = newMaterials;
                                }
                                else if (isPlayer && p.TryGetComponent(out VisEquipment visEquip) && visEquip.m_bodyModel != null)
                                {
                                    visEquip.m_bodyModel.sharedMaterials = newMaterials;
                                    visEquip.m_bodyModel.materials = newMaterials;
                                }
                            }
                            effect.m_prefab = p;
                        };
                        newRagdoll.Create();
                        break;
                    }
                }
            }
            MonsterDBPlugin.LogDebug($"Cloned {source.name} as {cloneName}");
            if (write)
            {
                Write(prefab, true, source.name);
            }

            SpawnManager.Create(prefab);
        };

        clone.Create();

        return;
        
        GameObject[] CreateItems(GameObject[] list, ref Dictionary<string, GameObject> clones)
        {
            List<GameObject> newItems = new();
            foreach (GameObject item in list)
            {
                if (item == null) continue;
                string newItemName = $"MDB_{cloneName}_{item.name}";
                if (clones.TryGetValue(newItemName, out GameObject newItem))
                {
                    newItems.Add(newItem);
                    continue;
                }

                GameObject? projectile = null;
                
                if (item.TryGetComponent(out ItemDrop id))
                {
                    if (id.m_itemData.m_shared.m_attack.m_attackProjectile != null)
                    {
                        string projName = $"MDB_{cloneName}_{id.m_itemData.m_shared.m_attack.m_attackProjectile.name}";
                        if (clones.TryGetValue(projName, out GameObject proj))
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
                
                mock.OnCreated += prefab =>
                {
                    newItems.Add(prefab);
                    newItem = prefab;
                    if (prefab.TryGetComponent(out ItemDrop component))
                    {
                        if (projectile != null)
                        {
                            component.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;
                        }
                    }
                };
                
                mock.Create();
                
                if (newItem != null)
                {
                    clones[newItemName] = newItem;
                }
            }

            return newItems.ToArray();
        }
    }
}