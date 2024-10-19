using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MonsterDB.Solution.Methods;
using UnityEngine;

namespace MonsterDB.Solution.Behaviors;

public class Human : Humanoid
{
    public ItemDrop.ItemData? m_weaponLoaded;
    private readonly List<Player.MinorActionData> m_actionQueue = new();
    public EffectList m_equipStartEffects = new();
    private float m_actionQueuePause;
    public string m_actionAnimation = "";

    public override void Awake()
    {
        base.Awake();
        UpdateComponent();
    }

    public override void OnRagdollCreated(Ragdoll ragdoll)
    {
        if (!ragdoll.TryGetComponent(out VisEquipment component)) return;
        SetupVisEquipment(component, true);
        Vector3 hairColor = m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero);
        Vector3 skinColor = m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one);
        component.SetHairColor(hairColor);
        component.SetSkinColor(skinColor);
    }

    private void UpdateComponent()
    {
        if (!CreatureManager.m_data.TryGetValue(gameObject.name.Replace("(Clone)", string.Empty), out CreatureData creatureData)) return;
        Vector3 scale = Helpers.GetScale(creatureData.m_scale);
            
        m_defaultItems = new List<GameObject>().ToArray();
        m_randomWeapon = new List<GameObject>().ToArray();
        m_randomShield = new List<GameObject>().ToArray();
        m_randomArmor = new List<GameObject>().ToArray();
        m_randomSets = new List<ItemSet>().ToArray();
        m_randomItems = new List<RandomItem>().ToArray();

        HumanoidMethods.UpdateItems(ref m_defaultItems, creatureData.m_defaultItems, scale);
        HumanoidMethods.UpdateItems(ref m_randomWeapon, creatureData.m_randomWeapons, scale);
        HumanoidMethods.UpdateItems(ref m_randomShield, creatureData.m_randomShields, scale);
        HumanoidMethods.UpdateItems(ref m_randomArmor, creatureData.m_randomArmors, scale);
        HumanoidMethods.UpdateRandomSets(ref m_randomSets, creatureData.m_randomSets, scale);
        HumanoidMethods.UpdateRandomItems(ref m_randomItems, creatureData.m_randomItems, scale);
    }

    public void FixedUpdate()
    {
        float fixedDeltaTime = Time.deltaTime;
        if (!m_nview) return;
        if (m_nview.GetZDO() == null) return;
        if (!m_nview.IsOwner()) return;
        if (IsDead()) return;
        UpdateActionQueue(fixedDeltaTime);
        UpdateWeaponLoading(GetCurrentWeapon(), fixedDeltaTime);
    }
    
    public void UpdateWeaponLoading(ItemDrop.ItemData? weapon, float dt)
    {
        if (weapon == null || !weapon.m_shared.m_attack.m_requiresReload) return;
        if (m_weaponLoaded == weapon || !weapon.m_shared.m_attack.m_requiresReload || IsReloadActionQueued()) return;
        QueueReloadAction();
    }
    
    public void UpdateActionQueue(float dt)
    {
        if (m_actionQueuePause > 0.0)
        {
            --m_actionQueuePause;
            if (m_actionAnimation.IsNullOrWhiteSpace()) return;
            m_animator.SetBool(m_actionAnimation, false);
            m_actionAnimation = "";
        }
        else if (InAttack())
        {
            if (m_actionAnimation.IsNullOrWhiteSpace()) return;
            m_animator.SetBool(m_actionAnimation, false);
            m_actionAnimation = "";
        }
        else if (m_actionQueue.Count == 0)
        {
            if (m_actionAnimation.IsNullOrWhiteSpace()) return;
            m_animator.SetBool(m_actionAnimation, false);
            m_actionAnimation = "";
        }
        else
        {
            Player.MinorActionData? action = m_actionQueue[0];
            if (!m_actionAnimation.IsNullOrWhiteSpace() && m_actionAnimation != action.m_animation)
            {
                m_animator.SetBool(m_actionAnimation, false);
                m_actionAnimation = "";
            }
            m_animator.SetBool(action.m_animation, true);
            m_actionAnimation = action.m_animation;
            if (action.m_time == 0.0 && action.m_startEffect != null)
            {
                action.m_startEffect.Create(transform.position, Quaternion.identity);
            }

            action.m_time += dt;
            if (action.m_time <= action.m_duration) return;
            m_actionQueue.RemoveAt(0);
            m_animator.SetBool(m_actionAnimation, false);
            m_actionAnimation = "";
            if (!action.m_doneAnimation.IsNullOrWhiteSpace())
            {
                m_animator.SetTrigger(action.m_doneAnimation);
            }
            
            switch (action.m_type)
            {
                case Player.MinorActionData.ActionType.Equip:
                    EquipItem(action.m_item);
                    break;
                case Player.MinorActionData.ActionType.Unequip:
                    UnequipItem(action.m_item);
                    break;
                case Player.MinorActionData.ActionType.Reload:
                    SetWeaponLoaded(action.m_item);
                    break;
            }

            m_actionQueuePause = 0.3f;
        }
    }

    public override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
    {
        if (!isRagdoll)
        {
            visEq.SetLeftItem(m_leftItem != null ? m_leftItem.m_dropPrefab.name : "", m_leftItem?.m_variant ?? 0);
            visEq.SetRightItem(m_rightItem != null ? m_rightItem.m_dropPrefab.name : "");
            visEq.SetLeftBackItem(m_hiddenLeftItem != null ? m_hiddenLeftItem.m_dropPrefab.name : "", m_hiddenLeftItem?.m_variant ?? 0);
            visEq.SetRightBackItem(m_hiddenRightItem != null ? m_hiddenRightItem.m_dropPrefab.name : "");
        }
        visEq.SetChestItem(m_chestItem != null ? m_chestItem.m_dropPrefab.name : "");
        visEq.SetLegItem(m_legItem != null ? m_legItem.m_dropPrefab.name : "");
        visEq.SetHelmetItem(m_helmetItem != null ? m_helmetItem.m_dropPrefab.name : "");
        visEq.SetShoulderItem(m_shoulderItem != null ? m_shoulderItem.m_dropPrefab.name : "", m_shoulderItem?.m_variant ?? 0);
        visEq.SetUtilityItem(m_utilityItem != null ? m_utilityItem.m_dropPrefab.name : "");
        visEq.SetBeardItem(m_beardItem);
        visEq.SetHairItem(m_hairItem);
        // if (TryGetComponent(out Visuals randomHuman))
        // {
        //     visEq.SetHairColor(randomHuman.m_hairColor);
        // }
    }
    
    public override bool ToggleEquipped(ItemDrop.ItemData item)
    {
        if (!item.IsEquipable()) return false;
        if (item.m_shared.m_equipDuration <= 0.0)
        {
            if (IsItemEquiped(item)) UnequipItem(item);
            else EquipItem(item);
        }
        else if (IsItemEquiped(item)) QueueUnequipAction(item);
        else QueueEquipAction(item);
        return true;
    }
    
    private void QueueUnequipAction(ItemDrop.ItemData? item)
    {
        if (item == null) return;
        if (IsEquipActionQueued(item))
        {
            RemoveEquipAction(item);
        }
        else
        {
            CancelReloadAction();
            m_actionQueue.Add(new Player.MinorActionData()
            {
                m_item = item,
                m_type = Player.MinorActionData.ActionType.Unequip,
                m_duration = item.m_shared.m_equipDuration,
                m_animation = "equipping"
            });
        }
    }
    
    public void QueueEquipAction(ItemDrop.ItemData? item)
    {
        if (item == null) return;
        if (IsEquipActionQueued(item))
        {
            RemoveEquipAction(item);
        }
        else
        {
            CancelReloadAction();
            Player.MinorActionData minorActionData = new Player.MinorActionData
            {
                m_item = item,
                m_type = Player.MinorActionData.ActionType.Equip,
                m_duration = item.m_shared.m_equipDuration,
                m_animation = "equipping"
            };
            if (minorActionData.m_duration >= 1.0)
            {
                minorActionData.m_startEffect = m_equipStartEffects;
            }
            m_actionQueue.Add(minorActionData);
        }
    }
    
    public void CancelReloadAction()
    {
        foreach (Player.MinorActionData? action in m_actionQueue)
        {
            if (action.m_type is not Player.MinorActionData.ActionType.Reload) continue;
            m_actionQueue.Remove(action);
            break;
        }
    }
    
    private bool IsEquipActionQueued(ItemDrop.ItemData? item)
    {
        return item != null && m_actionQueue.Any(action => action.m_type is Player.MinorActionData.ActionType.Equip or Player.MinorActionData.ActionType.Unequip && action.m_item == item);
    }

    public override bool InMinorAction()
    {
        return base.InMinorAction();
    }

    public override bool HaveEitr(float amount = 0)
    {
        return base.HaveEitr(amount);
    }

    public override float GetMaxEitr() => 9999f;

    public override bool StartAttack(Character? target, bool secondaryAttack)
    {
        if (InAttack() && !HaveQueuedChain() || InDodge() || !CanMove() || IsKnockedBack() || IsStaggering() ||
            InMinorAction()) return false;
    
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
                m_timeSinceLastAttack, Random.Range(0.5f, 1f))) return false;

        if (currentWeapon.m_shared.m_attack.m_requiresReload) SetWeaponLoaded(null);
        if (currentWeapon.m_shared.m_attack.m_bowDraw) currentWeapon.m_shared.m_attack.m_attackDrawPercentage = 0.0f;
        // if (currentWeapon.m_shared.m_itemType is not ItemDrop.ItemData.ItemType.Torch) currentWeapon.m_durability -= 1.5f;
        
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
    
    public override void ClearActionQueue() => m_actionQueue.Clear();
    
    public bool IsQueueActive() => m_actionQueue.Count > 0;
    
    private bool IsReloadActionQueued() => m_actionQueue.Any(action => action.m_type is Player.MinorActionData.ActionType.Reload);

    private void QueueReloadAction()
    {
        if (IsReloadActionQueued()) return;
        ItemDrop.ItemData? currentWeapon = GetCurrentWeapon();
        if (currentWeapon == null || !currentWeapon.m_shared.m_attack.m_requiresReload) return;
        m_actionQueue.Add(new Player.MinorActionData()
        {
            m_item = currentWeapon,
            m_type = Player.MinorActionData.ActionType.Reload,
            m_duration = currentWeapon.GetWeaponLoadingTime(),
            m_animation = currentWeapon.m_shared.m_attack.m_reloadAnimation,
            m_doneAnimation = currentWeapon.m_shared.m_attack.m_reloadAnimation + "_done",
            m_staminaDrain = currentWeapon.m_shared.m_attack.m_reloadStaminaDrain,
            m_eitrDrain = currentWeapon.m_shared.m_attack.m_reloadEitrDrain
        });
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
    
    [HarmonyPatch(typeof(Humanoid), nameof(EquipBestWeapon))]
    private static class EquipBestWeapon_Patch
    {
        private static bool Prefix(Humanoid __instance)
        {
            if (__instance is not Human component) return true;
            List<ItemDrop.ItemData> allItems = component.GetInventory().GetAllItems();
            if (allItems.Count == 0 || component.InAttack()) return true;
            ModifyRangedItems(allItems);
            return true;
        }

        private static void ModifyRangedItems(List<ItemDrop.ItemData> allItems)
        {
            foreach (ItemDrop.ItemData? item in allItems)
            {
                if (!item.IsWeapon()) continue;
                
                switch (item.m_shared.m_skillType)
                {
                    case Skills.SkillType.Bows:
                        item.m_shared.m_attack.m_projectileVel = 30f;
                        item.m_shared.m_attack.m_projectileVelMin = 2f;
                        item.m_shared.m_attack.m_projectileAccuracy = 2f;
                        item.m_shared.m_attack.m_projectileAccuracyMin = 20f;
                        item.m_shared.m_attack.m_useCharacterFacingYAim = true;
                        item.m_shared.m_attack.m_launchAngle = 0f;
                        item.m_shared.m_attack.m_projectiles = 1;
                        item.m_shared.m_aiAttackRange = 30f;
                        item.m_shared.m_aiAttackRangeMin = 5f;
                        item.m_shared.m_aiAttackInterval = 12f;
                        item.m_shared.m_aiAttackMaxAngle = 15f;
                        item.m_shared.m_aiWhenFlying = true;
                        item.m_shared.m_aiWhenWalking = true;
                        item.m_shared.m_aiWhenSwiming = true;
                        break;
                    case Skills.SkillType.ElementalMagic:
                        item.m_shared.m_aiAttackRange = 20f;
                        item.m_shared.m_aiAttackRangeMin = 5f;
                        item.m_shared.m_aiAttackInterval = item.m_shared.m_name == "$item_stafficeshards" ? 30f : 12f;
                        item.m_shared.m_aiAttackMaxAngle = 15f;
                        item.m_shared.m_aiWhenFlying = true;
                        item.m_shared.m_aiWhenWalking = true;
                        item.m_shared.m_aiWhenSwiming = true;
                        break;
                    case Skills.SkillType.BloodMagic:
                        item.m_shared.m_aiAttackRange = 20f;
                        item.m_shared.m_aiAttackRangeMin = 5f;
                        item.m_shared.m_aiAttackInterval = 30f;
                        item.m_shared.m_aiAttackMaxAngle = 15f;
                        item.m_shared.m_aiWhenFlying = true;
                        item.m_shared.m_aiWhenWalking = true;
                        item.m_shared.m_aiWhenSwiming = true;
                        break;
                    case Skills.SkillType.Crossbows:
                        item.m_shared.m_attack.m_projectileVel = 200f;
                        item.m_shared.m_attack.m_projectileVelMin = 2f;
                        item.m_shared.m_attack.m_projectileAccuracy = 2f;
                        item.m_shared.m_attack.m_projectileAccuracyMin = 20f;
                        item.m_shared.m_attack.m_useCharacterFacingYAim = true;
                        item.m_shared.m_attack.m_launchAngle = 0f;
                        item.m_shared.m_attack.m_projectiles = 1;
                        item.m_shared.m_aiAttackRange = 40f;
                        item.m_shared.m_aiAttackRangeMin = 5f;
                        item.m_shared.m_aiAttackInterval = 12f;
                        item.m_shared.m_aiAttackMaxAngle = 15f;
                        item.m_shared.m_aiWhenFlying = true;
                        item.m_shared.m_aiWhenWalking = true;
                        item.m_shared.m_aiWhenSwiming = true;
                        item.m_shared.m_attack.m_attackProjectile = ZNetScene.instance.GetPrefab("DvergerArbalest_projectile");
                        break;
                    default:
                        if (item.m_shared.m_attack.m_attackType is Attack.AttackType.Projectile)
                        {
                            item.m_shared.m_attack.m_projectileVel = 30f;
                            item.m_shared.m_attack.m_projectileVelMin = 2f;
                            item.m_shared.m_attack.m_projectileAccuracy = 2f;
                            item.m_shared.m_attack.m_projectileAccuracyMin = 20f;
                            item.m_shared.m_attack.m_useCharacterFacingYAim = true;
                            item.m_shared.m_attack.m_launchAngle = 0f;
                            item.m_shared.m_attack.m_projectiles = 1;
                            item.m_shared.m_aiAttackRange = 30f;
                            item.m_shared.m_aiAttackRangeMin = 5f;
                            item.m_shared.m_aiAttackInterval = 12f;
                            item.m_shared.m_aiAttackMaxAngle = 15f;
                            item.m_shared.m_aiWhenFlying = true;
                            item.m_shared.m_aiWhenWalking = true;
                            item.m_shared.m_aiWhenSwiming = true;
                        }
                        break;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.HaveAmmo))]
    private static class Attack_HaveAmmo_Patch
    {
        private static void Postfix(Humanoid character, ref bool __result)
        {
            if (character is not Human companion) return;
            __result = true;
        }
    }
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.EquipAmmoItem))]
    private static class Attack_EquipAmmoItem_Patch
    {
        private static void Prefix(Humanoid character, ItemDrop.ItemData weapon, ref bool __result)
        {
            if (character is not Human companion) return;
            switch (weapon.m_shared.m_ammoType)
            {
                case "$ammo_arrows":
                    var arrow = ObjectDB.instance.GetItemPrefab("ArrowWood");
                    if (arrow.TryGetComponent(out ItemDrop arrowComponent))
                    {
                        ItemDrop.ItemData? cloneArrow = arrowComponent.m_itemData.Clone();
                        character.GetInventory().AddItem(cloneArrow);
                    }

                    break;
                case "$ammo_bolts":
                    var bolt = ObjectDB.instance.GetItemPrefab("BoltBone");
                    if (bolt.TryGetComponent(out ItemDrop boltComponent))
                    {
                        ItemDrop.ItemData? cloneArrow = boltComponent.m_itemData.Clone();
                        character.GetInventory().AddItem(cloneArrow);
                    }

                    break;
            }

            __result = true;
        }
    }
}