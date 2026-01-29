using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public static class EggManager
{
    private static readonly List<string> newEggComponentPrefabs;
    private static readonly ConfigEntry<Toggle> _addPercentage;
    private static bool addPercentage => _addPercentage.Value is Toggle.On;

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
        
        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(ItemDrop), nameof(ItemDrop.GetHoverText)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(EggManager), nameof(Patch_ItemDrop_GetHoverText))));
        harmony.Patch(AccessTools.Method(typeof(EggGrow), nameof(EggGrow.GetHoverText)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(EggManager), nameof(Patch_EggGrow_GetHoverText))));
        harmony.Patch(AccessTools.Method(typeof(EggGrow), nameof(EggGrow.UpdateEffects)),
            new HarmonyMethod(AccessTools.Method(typeof(EggManager), nameof(Patch_EggGrow_UpdateEffects))));
    }

    public static void Setup()
    {
        Command save = new Command("write_egg", $"[prefabName]: save egg reference to {FileManager.ExportFolder} folder",
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
                    MonsterDBPlugin.LogWarning($"Failed to find prefab: {prefabName}");
                    return true;
                }

                if (!prefab.GetComponent<ItemDrop>())
                {
                    MonsterDBPlugin.LogWarning("Invalid, missing ItemDrop component");
                    return true;
                }
                
                bool isClone = false;
                string source = "";

                if (CloneManager.clones.TryGetValue(prefabName, out Clone c))
                {
                    isClone = true;
                    source = c.SourceName;
                }

                Write(prefab, isClone, source);
                return true;
            }, PrefabManager.GetAllPrefabNames<ItemDrop>);

        Command saveAll = new Command("write_all_egg", $"save all egg references to {FileManager.ExportFolder} folder", _ =>
        {
            List<GameObject> prefabs = PrefabManager.GetAllPrefabs<EggGrow>();
            for (var i = 0; i < prefabs.Count; ++i)
            {
                var prefab = prefabs[i];
                Write(prefab);
            }

            return true;
        });

        Command read = new Command("mod_egg", $"[fileName]: read egg reference from {FileManager.ImportFolder} folder",
            args =>
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

        Command revert = new Command("revert_egg", "[prefabName]: revert egg to factory settings", args =>
        {
            if (args.Length < 3)
            {
                MonsterDBPlugin.LogInfo("Invalid parameters");
                return true;
            }

            string prefabName = args.GetString(2);
            if (string.IsNullOrEmpty(prefabName))
            {
                MonsterDBPlugin.LogInfo("Invalid prefab");
                return true;
            }

            if (LoadManager.GetOriginal<BaseEgg>(prefabName) is not { } egg)
            {
                MonsterDBPlugin.LogInfo("Original data not found");
                return true;
            }

            egg.Update();
            LoadManager.UpdateSync();

            return true;
        }, LoadManager.GetOriginalKeys<BaseEgg>, adminOnly: true);

        Command clone = new Command("clone_egg", "[prefabName][newName]: must be an item", args =>
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
                MonsterDBPlugin.LogWarning($"Failed to find prefab: {prefabName}");
                return true;
            }

            if (!prefab.GetComponent<ItemDrop>())
            {
                MonsterDBPlugin.LogWarning("Invalid prefab, missing ItemDrop component");
                return true;
            }

            Clone(prefab, newName);
            return true;
        }, PrefabManager.GetAllPrefabNames<ItemDrop>, adminOnly: true);
    }

    public static string? Save(GameObject prefab, bool isClone = false, string clonedFrom = "")
    {
        if (!prefab.GetComponent<ItemDrop>()) return null;

        if (LoadManager.GetOriginal<BaseEgg>(prefab.name) is {} reference)
        {
            return ConfigManager.Serialize(reference);
        }

        reference = new BaseEgg();
        reference.Setup(prefab, isClone, clonedFrom);

        LoadManager.originals.Add(prefab.name, reference);
        
        return ConfigManager.Serialize(reference);
    }

    public static void Write(GameObject prefab,  bool isClone = false, string clonedFrom = "")
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
            LoadManager.UpdateSync();
        }
        catch
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize: {Path.GetFileName(filePath)}");
        }
    }

    public static void Clone(GameObject source, string cloneName, bool write = true)
    {
        if (CloneManager.prefabs.ContainsKey(cloneName)) return;
        if (!source.GetComponent<ItemDrop>()) return;
        
        Clone c = new Clone(source, cloneName);
        c.OnCreated += p =>
        {
            if (!p.GetComponent<EggGrow>())
            {
                p.AddComponent<EggGrow>();
                RegisterHoverOverride(p.name);
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

    private static void GetEggGrowth(EggGrow egg, out bool isGrowing, out double growPercentage)
    {
        float growStart = egg.m_nview.GetZDO().GetFloat(ZDOVars.s_growStart);
        isGrowing = growStart > 0.0;
        growPercentage = 0.0;
        if (isGrowing)
        {
            double timeElapsed = ZNet.instance.GetTimeSeconds() - growStart;
            growPercentage = Mathf.Clamp01((float)timeElapsed / egg.m_growTime) * 100f;
        }
    }

    private static string GetEggHoverText(ItemDrop item, EggGrow eggGrow)
    {
        item.Load();
        
        GetEggGrowth(eggGrow, out bool isGrowing, out double growPercentage);

        StringBuilder sb = new();
        sb.Append(item.m_itemData.m_shared.m_name);
        if (item.m_itemData.m_quality > 1)
        {
            sb.Append($"[{item.m_itemData.m_quality}]");
        }

        bool isStacked = item.m_itemData.m_stack > 1;
        
        if (isStacked)
        {
            sb.Append($" x{item.m_itemData.m_stack}");
            sb.Append(" $item_chicken_egg_stacked");
        }
        else
        {
            if (isGrowing)
            {
                string warm = Localization.instance.Localize("$item_chicken_egg_warm");
                warm = warm.Replace("(", string.Empty).Replace(")", string.Empty).Trim();
                sb.Append($" ({warm}");
                if (addPercentage)
                {
                    sb.Append($" {growPercentage:0.0}%");
                }
                sb.Append(")");
            }
            else
            {
                sb.Append(" $item_chicken_egg_cold");
            }
        }
        
        if (item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable && item.IsPiece())
        {
            string consumeText = item.m_itemData.m_shared.m_isDrink ? "$item_drink" : "$item_eat";
            sb.Append($"\n[<color=yellow><b>$KEY_Use</b></color>] {consumeText}");
        }
        else
        {
            sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
        }

        return Localization.instance.Localize(sb.ToString());
    }

    private static bool Patch_ItemDrop_GetHoverText(ItemDrop __instance, ref string __result)
    {
        if (!__instance.TryGetComponent(out EggGrow eggGrow) || !IsNewEgg(__instance)) return true;
        if (!eggGrow.m_nview || !eggGrow.m_nview.IsValid()) return true;
        __result = GetEggHoverText(__instance, eggGrow);
        return false;
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

        __result = GetEggHoverText(__instance.m_item, __instance);
        return false;
    }

    private static void Patch_EggGrow_UpdateEffects(EggGrow __instance, float grow)
    {
        if (!IsNewEgg(__instance.m_item)) return;
        ParticleSystem? ps = __instance.GetComponent<ParticleSystem>();
        if (ps == null) return;
            
        bool enablePS = grow == 0.0;
        if (enablePS)
        {
            ps.Play();
        }
        else
        {
            ps.Stop();
        }
    }
}