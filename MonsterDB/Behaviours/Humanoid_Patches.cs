using HarmonyLib;

namespace MonsterDB;

public static class Humanoid_Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    private static class Humanoid_EquipItem_Patch
    {
        private static void Prefix(Humanoid __instance, ItemDrop.ItemData item)
        {
            if ( __instance.IsPlayer()) return;
            if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Customization) return;
            if (item.m_dropPrefab == null) return;
            string? name = Utils.GetPrefabName(item.m_dropPrefab);
            if (name.CustomStartsWith("Beard"))
            {
                __instance.SetBeard(name);
            }
            else if (name.CustomStartsWith("Hair"))
            {
                __instance.SetHair(name);
            }
        }
    }
}