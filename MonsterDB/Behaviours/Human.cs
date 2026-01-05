using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public class Human : Humanoid
{
    public ItemDrop.ItemData? m_weaponLoaded;
    public int m_modelIndex;
    public Vector3 m_skinColor;
    public Vector3 m_hairColor;

    public int[]? m_models;
    public string[]? m_skinColors;
    public string[]? m_hairColors;
    public string[]? m_beards;
    public string[]? m_hairs;

    public override void Start()
    {
        base.Start();
        if (m_nview.IsValid() && !m_nview.GetZDO().GetBool(ZDOVars.s_addedDefaultItems))
        {
            if (m_models != null)
            {
                m_modelIndex = m_models[UnityEngine.Random.Range(0, m_models.Length)];
            }

            if (m_beards != null)
            {
                m_beardItem = m_beards[UnityEngine.Random.Range(0, m_beards.Length)];
            }

            if (m_hairs != null)
            {
                m_hairItem = m_hairs[UnityEngine.Random.Range(0, m_hairs.Length)];
            }
            if (m_skinColor != null)
            {
                string color = m_skinColors[UnityEngine.Random.Range(0, m_skinColors.Length)];
                m_skinColor = Utils.ColorToVec3(color.FromHex(Color.white));
            }
            if (m_hairColors != null)
            {
                string color = m_hairColors[UnityEngine.Random.Range(0, m_hairColors.Length)];
                m_hairColor = Utils.ColorToVec3(color.FromHex(Color.black));
            }
            
            m_nview.GetZDO().Set(ZDOVars.s_addedDefaultItems, true);
        }
    }

    public override bool StartAttack(Character? target, bool secondaryAttack)
    {
        if (InAttack() && !HaveQueuedChain() || InDodge() || !CanMove() || IsKnockedBack() || IsStaggering() || InMinorAction()) return false;
    
        ItemDrop.ItemData currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null || (!currentWeapon.HaveSecondaryAttack() && !currentWeapon.HavePrimaryAttack())) return false;

        bool secondary = currentWeapon.HaveSecondaryAttack() && UnityEngine.Random.value > 0.5;
        if (currentWeapon.m_shared.m_skillType is Skills.SkillType.Spears) secondary = false;
        
        if (m_currentAttack != null)
        {
            m_currentAttack.Stop();
            m_previousAttack = m_currentAttack;
            m_currentAttack = null;
        }
        Attack? attack = !secondary ? currentWeapon.m_shared.m_attack.Clone() : currentWeapon.m_shared.m_secondaryAttack.Clone();
        if (!attack.Start(this, m_body, m_zanim, m_animEvent, m_visEquipment, currentWeapon, m_previousAttack,
                m_timeSinceLastAttack, UnityEngine.Random.Range(0.5f, 1f))) return false;

        if (currentWeapon.m_shared.m_attack.m_requiresReload) SetWeaponLoaded(null);
        if (currentWeapon.m_shared.m_attack.m_bowDraw) currentWeapon.m_shared.m_attack.m_attackDrawPercentage = 0.0f;
        if (currentWeapon.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Torch) currentWeapon.m_durability -= 1.5f;
        
        ClearActionQueue();
        StartAttackGroundCheck();
        m_currentAttack = attack;
        m_currentAttackIsSecondary = secondary;
        m_lastCombatTimer = 0.0f;
        if (currentWeapon.m_shared.m_name == "$item_stafficeshards")
        {
            Invoke(nameof(StopCurrentAttack), 5f);
        }
        return true;
    }
    
    public override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
    {
        if (!isRagdoll)
        {
            visEq.SetLeftItem(m_leftItem != null ? m_leftItem.m_dropPrefab.name : "",
                m_leftItem != null ? m_leftItem.m_variant : 0);
            visEq.SetRightItem(m_rightItem != null ? m_rightItem.m_dropPrefab.name : "");
        }

        visEq.SetChestItem(m_chestItem != null ? m_chestItem.m_dropPrefab.name : "");
        visEq.SetLegItem(m_legItem != null ? m_legItem.m_dropPrefab.name : "");
        visEq.SetHelmetItem(m_helmetItem != null ? m_helmetItem.m_dropPrefab.name : "");
        visEq.SetShoulderItem(m_shoulderItem != null ? m_shoulderItem.m_dropPrefab.name : "",
            m_shoulderItem != null ? m_shoulderItem.m_variant : 0);
        visEq.SetUtilityItem(m_utilityItem != null ? m_utilityItem.m_dropPrefab.name : "");
        visEq.SetTrinketItem(m_trinketItem != null ? m_trinketItem.m_dropPrefab.name : "");
        visEq.SetBeardItem(m_beardItem);
        visEq.SetHairItem(m_hairItem);
        visEq.SetSkinColor(m_skinColor);
        visEq.SetHairColor(m_hairColor);
        visEq.SetModel(m_modelIndex);
    }
    
    private void SetWeaponLoaded(ItemDrop.ItemData? weapon)
    {
        if (weapon == m_weaponLoaded) return;
        m_weaponLoaded = weapon;
        m_nview.GetZDO().Set(ZDOVars.s_weaponLoaded, weapon != null);
    }
    
    private void StopCurrentAttack()
    {
        if (m_currentAttack == null) return;
        m_currentAttack.Stop();
        m_previousAttack = m_currentAttack;
        m_currentAttack = null;
    }
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.HaveAmmo))]
    private static class Attack_HaveAmmo
    {
        private static void Postfix(Humanoid character, ref bool __result)
        {
            if (__result || character is not Human) return;
            __result = true;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.FindAmmo))]
    private static class Attack_FindAmmo_Patch
    {
        private static void Postfix(Humanoid character, ItemDrop.ItemData weapon, ref ItemDrop.ItemData? __result)
        {
            if (__result != null || character is not Human viking) return;

            if (string.IsNullOrEmpty(weapon.m_shared.m_ammoType)) return;

            switch (weapon.m_shared.m_ammoType)
            {
                case "$ammo_arrows":
                    ItemDrop.ItemData? arrow = viking.GetInventory().AddItem("ArrowWood", 10, 1, 0, 0L, "");
                    if (arrow != null)
                    {
                        __result = arrow;
                    }
                    break;
                case "$ammo_bolts":
                    var bolt = viking.GetInventory().AddItem("BoltBone", 10, 1, 0, 0L, "");
                    if (bolt != null)
                    {
                        __result = bolt;
                    }
                    break;
            }
        }
    }
}