using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;

namespace MonsterDB;

[Serializable]
public class GrowUpRef : Reference
{
    public float? m_growTime;
    public bool? m_inheritTame;
    public string? m_grownPrefab;
    public List<GrownEntry>? m_altGrownPrefabs;
    
    public GrowUpRef(){}

    public GrowUpRef(Growup growUp) => Setup(growUp);

    [Serializable]
    public class GrownEntry : Reference
    {
        public string m_prefab = "";
        [DefaultValue(1f)] public float m_weight;
        
        public GrownEntry(){}

        public GrownEntry(Growup.GrownEntry entry)
        {
            m_prefab = entry.m_prefab.name;
            m_weight = entry.m_weight;
        }

        public Growup.GrownEntry ToGrowUpGrowEntry() => new Growup.GrownEntry()
        {
            m_prefab = PrefabManager.GetPrefab(m_prefab),
            m_weight = m_weight
        };
    }
}

public static partial class Extensions
{
    public static List<GrowUpRef.GrownEntry> ToGrowUpRefEntryList(this List<Growup.GrownEntry> ge)
    {
        List<GrowUpRef.GrownEntry> growEntries = ge
            .Where(x => x.m_prefab != null)
            .Select(x => new GrowUpRef.GrownEntry(x))
            .ToList();
        return growEntries;
    }

    public static List<Growup.GrownEntry> FromRef(this List<GrowUpRef.GrownEntry> ge)
    {
        List<Growup.GrownEntry> growEntries = ge
            .Select(x => x.ToGrowUpGrowEntry())
            .Where(x => x.m_prefab != null)
            .ToList();
        return growEntries;
    }
}

public static class GrowUpText
{
    private static ConfigEntry<Toggle> addGrowUpText = null!;

    public static void Setup()
    {
        addGrowUpText = ConfigManager.config("Grow Up", "Add Progress Text", Toggle.Off,
            "If on, will add progress text on character hover");
    }

    private static bool AddGrowUpText() => addGrowUpText.Value is Toggle.On;

    [HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
    private static class Character_GetHoverText
    {
        private static void Postfix(Character __instance, ref string __result)
        {
            if (!AddGrowUpText()) return;
            if (!__instance.TryGetComponent(out Growup component) || !string.IsNullOrEmpty(__result) || component.m_baseAI == null) return;

            double startTime = component.m_baseAI.GetTimeSinceSpawned().TotalSeconds;
            double percentage = startTime / component.m_growTime * 100f;
            __result = Localization.instance.Localize($"{__instance.m_name} ( $hud_growup_maturing {percentage:0}% )");
        }
    }
}