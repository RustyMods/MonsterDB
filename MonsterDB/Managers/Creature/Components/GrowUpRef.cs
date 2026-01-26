using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

[HarmonyPatch(typeof(Growup), nameof(Growup.GrowUpdate))]
public static class GrowUp_ConditioanlGrow_Patch
{
    private static bool Prefix(Growup __instance)
    {
        if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner()) return true;
        if (!__instance.TryGetComponent(out Tameable component)) return true;
        if (component.IsTamed()) return true;
        __instance.m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
        return false;
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

    public static string GetGrowthPercentageText(this Growup __instance)
    {
        if (__instance.m_baseAI == null) return "$hud_growup_maturing 0%";
        double startTime = __instance.m_baseAI.GetTimeSinceSpawned().TotalSeconds;
        double percentage = startTime / __instance.m_growTime * 100f;
        return $"$hud_growup_maturing {percentage:0}%";
    }

    [HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
    private static class Character_GetHoverText
    {
        private static bool Prefix(Character __instance, ref string __result)
        {
            if (!__instance.m_nview.IsValid()) return true;
            
            StringBuilder sb = new();
            if (__instance.TryGetComponent(out Tameable tameable))
            {
                sb.Append(tameable.GetName());
                if (tameable.IsTamed())
                {
                    sb.AppendFormat(" ( {0}, {1}", "$hud_tame", tameable.GetStatusString());
                    if (__instance.TryGetComponent(out Growup growup) && AddGrowUpText())
                    {
                        sb.Append($", {growup.GetGrowthPercentageText()}");
                    }
                    sb.Append(" )");
                    sb.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $hud_pet");
                    bool gamepad = ZInput.IsNonClassicFunctionality() && ZInput.IsGamepadActive();

                    sb.AppendFormat("\n[<color=yellow><b>{0} + $KEY_Use</b></color>] $hud_rename",
                        gamepad ? 
                            "$KEY_AltKeys" : 
                            "$KEY_AltPlace");
                
                    if (__instance.TryGetComponent(out Saddle saddle) && (!saddle.HasSaddleItem() || saddle.HaveSaddle()))
                    {
                        saddle.GetHoverText(sb, gamepad);
                    }
                }
                else
                {
                    int tameness = tameable.GetTameness();
                    sb.AppendFormat(" ( {0}, {1} )", tameness <= 0 ? 
                        "$hud_wild" : 
                        $"$hud_tameness {tameness}%", tameable.GetStatusString());
                }
            }
            else if (__instance.TryGetComponent(out Growup growup) && AddGrowUpText())
            {
                sb.Append($"{__instance.m_name} ( {growup.GetGrowthPercentageText()} )");
            }

            __result = Localization.instance.Localize(sb.ToString());
            return false;
        }
    }
}