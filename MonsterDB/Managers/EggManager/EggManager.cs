using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public static class EggManager
{
    private static readonly List<string> newEggComponentPrefabs;
    private static readonly ConfigEntry<Toggle> _addPercentage;

    public static void RegisterHoverOverride(string prefabName)
    {
        if (newEggComponentPrefabs.Contains(prefabName)) return;
        newEggComponentPrefabs.Add(prefabName);
    }

    static EggManager()
    {
        _addPercentage = ConfigManager.config("Eggs", "Growth Percentage", Toggle.On,
            "If on, will add growth percentage to hover text");
        newEggComponentPrefabs = new List<string>();
    }

    public static void Setup()
    {
        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(ItemDrop), nameof(ItemDrop.GetHoverText)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(EggManager), nameof(Patch_ItemDrop_GetHoverText))));
        harmony.Patch(AccessTools.Method(typeof(EggGrow), nameof(EggGrow.GetHoverText)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(EggManager), nameof(Patch_EggGrow_GetHoverText))));
    }
    
    private static Command save = new Command("write_egg", $"[prefabName]: save egg reference to {FileManager.ExportFolder} folder", args =>
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
    }, optionsFetcher: PrefabManager.GetAllPrefabNames<ItemDrop>);

    private static Command saveAll = new Command("write_all_egg", $"save all egg references to {FileManager.ExportFolder} folder", _ =>
    {
        List<GameObject> prefabs = PrefabManager.GetAllPrefabs<EggGrow>();
        for (int i = 0; i < prefabs.Count; ++i)
        {
            GameObject? prefab = prefabs[i];
            Write(prefab);
        }
        return true;
    });

    private static Command read = new Command("mod_egg", $"[fileName]: read egg reference from {FileManager.ImportFolder} folder", args =>
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

    private static Command revert = new Command("revert_egg", "[prefabName]: revert egg to factory settings", args =>
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

        if (SyncManager.GetOriginal<BaseEgg>(prefabName) is not {} egg)
        {
            MonsterDBPlugin.LogInfo("Original data not found");
            return true;
        }
        
        egg.Update();
        string text = ConfigManager.Serialize(egg);
        SyncManager.rawFiles[egg.Prefab] = text;
        SyncManager.UpdateSync();
        
        return true;
    }, optionsFetcher: SyncManager.GetOriginalKeys<BaseEgg>, adminOnly: true);

    private static Command clone = new Command("clone_egg", "[prefabName][newName]: must be an item", args =>
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

    public static string? Save(GameObject prefab, bool isClone = false, string clonedFrom = "")
    {
        if (!prefab.GetComponent<ItemDrop>()) return null;

        if (SyncManager.GetOriginal<BaseEgg>(prefab.name) is {} reference)
        {
            return ConfigManager.Serialize(reference);
        }

        reference = new BaseEgg();
        reference.Setup(prefab, isClone, clonedFrom);

        SyncManager.originals.Add(prefab.name, reference);
        
        return ConfigManager.Serialize(reference);
    }

    private static void Write(GameObject prefab,  bool isClone = false, string clonedFrom = "")
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
            if (header.Type != BaseType.Egg) return;
            BaseEgg reference = ConfigManager.Deserialize<BaseEgg>(text);
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
        if (!source.GetComponent<ItemDrop>()) return;
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            if (!p.GetComponent<EggGrow>())
            {
                p.AddComponent<EggGrow>();
                RegisterHoverOverride(p.name);
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

    private static bool IsNewEgg(ItemDrop item)
    {
        string? prefabName = Utils.GetPrefabName(item.name);
        return newEggComponentPrefabs.Contains(prefabName);
    }

    private static void Patch_ItemDrop_GetHoverText(ItemDrop __instance, ref string __result)
    {
        if (!__instance.TryGetComponent(out EggGrow eggGrow)) return;

        if (!IsNewEgg(__instance)) return;
            
        if (!eggGrow.m_nview || !eggGrow.m_nview.IsValid()) return;

        float growStart = eggGrow.m_nview.GetZDO().GetFloat(ZDOVars.s_growStart);
        bool isGrowing = growStart > 0.0;
        double growPercentage = 0.0;
        if (isGrowing)
        {
            double timeElapsed = ZNet.instance.GetTimeSeconds() - growStart;
            growPercentage = Mathf.Clamp01((float)timeElapsed / eggGrow.m_growTime) * 100f;
        }

        string text = __instance.m_itemData.m_stack > 1
            ? "$item_chicken_egg_stacked"
            : isGrowing ? _addPercentage.Value is Toggle.On ?
                    $"$item_chicken_egg_warm ({growPercentage:0.0}%)" : "$item_chicken_egg_warm" :
                "$item_chicken_egg_cold";
        string hover = __result;
        int num = hover.IndexOf('\n');
        __result = num > 0
            ? $"{hover.Substring(0, num)} {Localization.instance.Localize(text)} {hover.Substring(num)}"
            : hover;
    }

    private static bool Patch_EggGrow_GetHoverText(EggGrow __instance, ref string __result)
    {
        if (_addPercentage.Value is Toggle.Off) return true;
            
        if (!__instance.m_item) return true;
        if (!__instance.m_nview || !__instance.m_nview.IsValid())
        {
            __result = __instance.m_item.GetHoverText();
            return false;
        }
        
        float growStart = __instance.m_nview.GetZDO().GetFloat(ZDOVars.s_growStart);
        bool isGrowing = growStart > 0.0;
        
        double growPercentage = 0.0;
        if (isGrowing)
        {
            double timeElapsed = ZNet.instance.GetTimeSeconds() - growStart;
            growPercentage = Mathf.Clamp01((float)timeElapsed / __instance.m_growTime) * 100f;
        }
        string text = __instance.m_item.m_itemData.m_stack > 1
            ? "$item_chicken_egg_stacked"
            : isGrowing ? 
                $"$item_chicken_egg_warm ({growPercentage:0.0}%)" : 
                "$item_chicken_egg_cold";
        string hover = __instance.m_item.GetHoverText();
        int num = hover.IndexOf('\n');
        __result = num > 0
            ? $"{hover.Substring(0, num)} {Localization.instance.Localize(text)} {hover.Substring(num)}"
            : hover;

        return false;
    }

    [HarmonyPatch(typeof(EggGrow), nameof(EggGrow.UpdateEffects))]
    private static class EggGrow_UpdateEffects
    {
        private static void Postfix(EggGrow __instance, float grow)
        {
            if (!IsNewEgg(__instance.m_item)) return;
            var ps = __instance.GetComponent<ParticleSystem>();
            if (ps == null) return;
            
            bool enablePS = grow > 0.0;
            if (enablePS)
            {
                ps.Play();
            }
            else
            {
                ps.Pause();
            }
        }
    }
}