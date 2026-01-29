using System.Text;
using BepInEx.Configuration;
using HarmonyLib;

namespace MonsterDB;

public static class GrowUpText
{
    private static ConfigEntry<Toggle> addGrowUpText = null!;

    public static void Setup()
    {
        addGrowUpText = ConfigManager.config("Grow Up", "Add Progress Text", Toggle.Off,
            "If on, will add progress text on character hover");
    }

    public static bool AddGrowUpText() => addGrowUpText.Value is Toggle.On;

    public static string GetGrowthPercentageText(this Growup __instance)
    {
        if (__instance.m_baseAI == null) return "$hud_growup_maturing 0%";
        double startTime = __instance.m_baseAI.GetTimeSinceSpawned().TotalSeconds;
        double percentage = startTime / __instance.m_growTime * 100f;
        return $"$hud_growup_maturing {percentage:0}%";
    }
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

[HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
public static class Character_GetHoverText_Patch
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
                if (__instance.TryGetComponent(out Growup growup) && GrowUpText.AddGrowUpText())
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
        else if (__instance.TryGetComponent(out Growup growup) && GrowUpText.AddGrowUpText() && __instance.IsTamed())
        {
            sb.Append($"{__instance.m_name} ( {growup.GetGrowthPercentageText()} )");
        }

        __result = Localization.instance.Localize(sb.ToString());
        return false;
    }
}