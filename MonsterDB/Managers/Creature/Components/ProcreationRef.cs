using System;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class ProcreationRef : Reference
{
    public float? m_updateInterval;
    public float? m_totalCheckRange;
    public int? m_maxCreatures;
    public float? m_partnerCheckRange;
    public float? m_pregnancyChance;
    public float? m_pregnancyDuration;
    public int? m_requiredLovePoints;
    public string? m_offspring;
    public int? m_minOffspringLevel;
    public float? m_spawnOffset;
    public float? m_spawnOffsetMax;
    public bool? m_spawnRandomDirection;
    public string? m_seperatePartner;
    public string? m_noPartnerOffspring;
    public EffectListRef? m_birthEffects;
    public EffectListRef? m_loveEffects;
    
    public ProcreationRef(){}
    public ProcreationRef(Procreation component) => Setup(component);
}

public static class ProcreateText
{
    private static ConfigEntry<Toggle> addProcreateProgressText = null!;

    public static void Setup()
    {
        addProcreateProgressText = ConfigManager.config("Procreation", "Add Progress Text", Toggle.Off,
            "If on, will add procreation progress info to hover text");
    }

    private static bool AddProgressText() => addProcreateProgressText.Value is Toggle.On;


    [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetStatusString))]
    private static class Tameable_GetStatusString
    {
        private static void Postfix(Tameable __instance, ref string __result)
        {
            if (!AddProgressText()) return;
            if (!__instance.IsTamed() || !__instance.TryGetComponent(out Procreation component)) return;

            if (component.IsPregnant())
            {
                __result += ", $hud_procreate_pregnant";
            }
            else
            {
                int points = component.GetLovePoints();
                float percentage = (float)points / component.m_requiredLovePoints * 100f;
                __result += $", $hud_procreate_bonding {percentage}%";
            }
        }
    }
}