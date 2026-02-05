using System;
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

    [Obsolete]
    public static void WriteEggYML(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        GameObject? prefab = PrefabManager.GetPrefab(prefabName);

        if (prefab == null)
        {
            args.Context.LogWarning($"Failed to find prefab: {prefabName}");
            return;
        }

        if (!prefab.GetComponent<ItemDrop>())
        {
            args.Context.LogWarning("Invalid, missing ItemDrop component");
            return;
        }

        bool isClone = CloneManager.IsClone(prefab.name, out string source);
        Write(prefab, isClone, source);
    }

    [Obsolete]
    public static List<string> GetEggOptions(int i, string word) => i switch
    {
        2 => PrefabManager.GetAllPrefabNames<ItemDrop>(),
        _ => new List<string>()
    };

    [Obsolete]
    public static void WriteAllEggYML(Terminal.ConsoleEventArgs args)
    {
        List<GameObject> prefabs = PrefabManager.GetAllPrefabs<EggGrow>();
        for (int i = 0; i < prefabs.Count; ++i)
        {
            GameObject? prefab = prefabs[i];
            Write(prefab);
        }
        args.Context.AddString($"Exported {prefabs.Count} egg files");
    }

    [Obsolete]
    public static void ReadEgg(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        string filePath = Path.Combine(FileManager.ImportFolder, prefabName + ".yml");
        Read(filePath);
    }

    [Obsolete]
    public static void ResetEgg(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid prefab");
            return;
        }

        if (LoadManager.GetOriginal<BaseEgg>(prefabName) is not { } egg)
        {
            args.Context.LogWarning("Original data not found");
            return;
        }

        egg.Update();
        LoadManager.UpdateSync();
    }

    public static void ResetEgg(Terminal context, string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            context.LogWarning("Invalid parameters");
            return;
        }

        if (prefabName.Equals("all", StringComparison.InvariantCultureIgnoreCase))
        {
            LoadManager.ResetAll<BaseEgg>();
            LoadManager.UpdateSync();
            return;
        }

        if (!LoadManager.Reset<BaseEgg>(prefabName))
        {
            context.LogWarning("Original data not found");
            return;
        }
        
        LoadManager.UpdateSync();
    }

    [Obsolete]
    public static List<string> GetResetOptions(int i, string word) => i switch
    {
        2 => LoadManager.GetOriginalKeys<BaseEgg>(),
        _ => new List<string>()
    };

    [Obsolete]
    public static void CloneEgg(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        string newName = args.GetString(3);
        if (string.IsNullOrEmpty(prefabName) || string.IsNullOrEmpty(newName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        GameObject? prefab = PrefabManager.GetPrefab(prefabName);
        if (prefab == null)
        {
            args.Context.LogWarning($"Failed to find prefab: {prefabName}");
            return;
        }

        if (!prefab.GetComponent<ItemDrop>())
        {
            args.Context.LogWarning("Invalid prefab, missing ItemDrop component");
            return;
        }

        Clone(prefab, newName);
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