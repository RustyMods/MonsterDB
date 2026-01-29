using HarmonyLib;

namespace MonsterDB;

[HarmonyPatch(typeof(Growup), nameof(Growup.GrowUpdate))]
public static class GrowUp_ConditioanlGrow_Patch
{
    private static bool Prefix(Growup __instance)
    {
        if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner()) return true;
        if (!__instance.TryGetComponent(out Character character) || character.IsTamed()) return true;
        __instance.m_nview.GetZDO().Set(ZDOVars.s_spawnTime, ZNet.instance.GetTime().Ticks);
        return false;
    }
}