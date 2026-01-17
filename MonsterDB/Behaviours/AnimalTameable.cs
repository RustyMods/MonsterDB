using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public class AnimalTameable : MonoBehaviour
{
    public List<ItemDrop> m_consumeItems = new();
    public float m_consumeRange = 2f;
    public float m_consumeSearchRange = 5f;
    public float m_consumeSearchInterval = 10f;
    public EffectList m_consumeItemEffects = new();
    public float m_consumeSearchTimer;
    public ItemDrop? m_consumeTarget;
    
    private static readonly int Consume = Animator.StringToHash("consume");

    [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.IdleMovement))]
    private static class AnimalAI_IdleMovement_Patch
    {
        private static bool Prefix(BaseAI __instance, float dt)
        {
            if (!__instance.TryGetComponent(out AnimalTameable animalTameable)) return true;
            if (__instance is not AnimalAI animalAI) return true;
            if (!__instance.TryGetComponent(out Tameable component)) return true;

            return !animalTameable.UpdateConsumeItem(__instance.m_character, animalAI, component, dt);
        }
    }
    
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.TamingUpdate))]
    private static class Tameable_TamingUpdate_Patch
    {
        private static void Postfix(Tameable __instance)
        {
            if (!ShouldPatchTameable(__instance) || __instance.IsHungry()) return;

            __instance.DecreaseRemainingTime(3f);
            if (__instance.GetRemainingTime() <= 0.0)
            {
                __instance.Tame();
            }
            else
            {
                __instance.m_sootheEffect.Create(__instance.transform.position, __instance.transform.rotation);
            }
        }
    }

    private static bool ShouldPatchTameable(Tameable __instance)
    {
        return __instance.m_nview.IsValid() && 
               __instance.m_nview.IsOwner() && 
               !__instance.IsTamed() &&
               __instance.m_character && 
               !__instance.m_monsterAI;
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.Tame))]
    private static class Tameable_Tame_Patch
    {
        private static void Postfix(Tameable __instance)
        {
            if (!ShouldPatchTameable(__instance)) return;

            __instance.m_character.SetTamed(true);
            __instance.m_tamedEffect.Create(__instance.transform.position, __instance.transform.rotation);
            Player closestPlayer = Player.GetClosestPlayer(__instance.transform.position, 30f);
            if (!closestPlayer) return;
            
            closestPlayer.Message(MessageHud.MessageType.Center, __instance.m_character.m_name + " $hud_tamedone");

        }
    }
    
    public bool UpdateConsumeItem(Character character, AnimalAI animalAI, Tameable tameable, float dt)
    {
        if (m_consumeItems.Count == 0)
        {
            return false;
        }
        
        m_consumeSearchTimer += dt;
        if (m_consumeSearchTimer > m_consumeSearchInterval)
        {
            m_consumeSearchTimer = 0.0f;
            if (!tameable.IsHungry())
            {
                return false;
            }
            ItemDrop? consumeTarget = FindClosestConsumableItem(animalAI, m_consumeSearchRange);
            m_consumeTarget = consumeTarget;
        }

        if (!m_consumeTarget)
        {
            return false;
        }
        
        if (animalAI.MoveTo(dt, m_consumeTarget.transform.position, m_consumeRange, false))
        {
            animalAI.LookAt(m_consumeTarget.transform.position);
            if (animalAI.IsLookingAt(m_consumeTarget.transform.position, 20f) && m_consumeTarget.RemoveOne())
            {
                tameable.OnConsumedItem(m_consumeTarget);
                m_consumeItemEffects.Create(character.transform.position, Quaternion.identity);
                character.m_animator.SetTrigger(Consume);
                m_consumeTarget = null;
            }
        }

        return true;
    }

    public ItemDrop? FindClosestConsumableItem(AnimalAI animalAI, float maxRange)
    {
        if (MonsterAI.m_itemMask == 0)
        {
            MonsterAI.m_itemMask = LayerMask.GetMask("item");
        }

        Collider[] results = new Collider[20];
        int size = Physics.OverlapSphereNonAlloc(animalAI.transform.position, maxRange, results, MonsterAI.m_itemMask);
        ItemDrop? itemDrop = null;
        float closest = 999999f;
        for (var index = 0; index < size; ++index)
        {
            Collider collider = results[index];
            if (collider == null) continue;
            if (collider.attachedRigidbody)
            {
                ItemDrop? component = collider.attachedRigidbody.GetComponent<ItemDrop>();
                if (!(component == null) &&
                    component.GetComponent<ZNetView>().IsValid() &&
                    CanConsume(component.m_itemData))
                {
                    float distance = Vector3.Distance(component.transform.position, animalAI.transform.position);
                    if (itemDrop == null || distance < closest)
                    {
                        itemDrop = component;
                        closest = distance;
                    }
                }
            }
        }

        return itemDrop != null && animalAI.HavePath(itemDrop.transform.position) ? itemDrop : null;
    }

    public bool CanConsume(ItemDrop.ItemData item)
    {
        if (m_consumeItems.Count == 0)
        {
            return false;
        }
        foreach (ItemDrop? consumeItem in m_consumeItems)
        {
            if (consumeItem.m_itemData.m_shared.m_name == item.m_shared.m_name)
            {
                return true;
            }
        }
        return false;
    }
}