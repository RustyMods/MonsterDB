using System.Collections;
using UnityEngine;

namespace MonsterDB;

public class Human : Humanoid
{
    public int m_modelIndex;
    public Vector3 m_skinColor;
    public Vector3 m_hairColor;

    public int[]? m_models;
    public string[]? m_skinColors;
    public string[]? m_hairColors;
    public string[]? m_beards;
    public string[]? m_hairs;

    public bool m_reloading;
    public bool m_weaponLoaded;
    private readonly WaitForSeconds m_weaponLoadedDelay = new WaitForSeconds(1f);
    private Coroutine? m_reloadCoroutine;

    public override void Start()
    {
        base.Start();
        if (m_baseAI is MonsterAI monsterAI)
        {
            monsterAI.m_onConsumedItem += OnConsumeItem;
        }
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
            if (m_skinColors != null)
            {
                string color = m_skinColors[UnityEngine.Random.Range(0, m_skinColors.Length)];
                m_skinColor = Utils.ColorToVec3(color.FromHexOrRGBA(Color.white));
            }
            if (m_hairColors != null)
            {
                string color = m_hairColors[UnityEngine.Random.Range(0, m_hairColors.Length)];
                m_hairColor = Utils.ColorToVec3(color.FromHexOrRGBA(Color.black));
            }
            
            m_nview.GetZDO().Set(ZDOVars.s_addedDefaultItems, true);
            SetupVisEquipment(m_visEquipment, false);
        }
    }

    private void OnConsumeItem(ItemDrop item)
    {
        string trigger = item.m_itemData.m_shared.m_isDrink ? "emote_drink" : "eat";
        m_animator.SetTrigger(trigger);
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
    public override float GetMaxEitr() => 9999f;

    public void LateUpdate()
    {
        if (m_currentAttack == null || !m_currentAttack.m_loopingAttack) return;
        int projectilesFired = Mathf.CeilToInt(m_lastCombatTimer / m_currentAttack.m_burstInterval);
        if (projectilesFired > 10)
        {
            m_currentAttack.Stop();
            m_previousAttack = m_currentAttack;
            m_currentAttack = null;
        }
    }

    public override void ResetLoadedWeapon()
    {
        m_weaponLoaded = false;
        StartWeaponReload();
    }

    public override bool IsWeaponLoaded()
    {
        if (!m_weaponLoaded) StartWeaponReload();
        return m_weaponLoaded;
    }

    public void StartWeaponReload()
    {
        if (m_reloading || m_weaponLoaded) return;
        ItemDrop.ItemData? weapon = GetCurrentWeapon();
        if (weapon == null || !weapon.m_shared.m_attack.m_requiresReload) return;
        if (m_reloadCoroutine != null)
        {
            StopCoroutine(m_reloadCoroutine);
            m_reloadCoroutine = null;
        }
        m_reloadCoroutine = StartCoroutine(ReloadRoutine(weapon.m_shared.m_attack.m_reloadAnimation, weapon.m_shared.m_attack.m_reloadTime));
    }

    private IEnumerator ReloadRoutine(string reloadAnimation, float reloadTime)
    {
        m_reloading = true;
        m_zanim.SetBool(reloadAnimation, true);
        yield return new WaitForSeconds(reloadTime);
        m_zanim.SetTrigger(reloadAnimation + "_done");
        m_zanim.SetBool(reloadAnimation, false);
        yield return m_weaponLoadedDelay;
        m_weaponLoaded = true;
        m_reloadCoroutine = null;
        m_reloading = false;
    }
}