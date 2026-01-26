using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class ItemDataSharedRef : Reference
{
    public string m_prefab = "";
    public string? m_name;
    public string? m_description;
    public int? m_maxStackSize;
    public int? m_maxQuality;
    public float? m_scaleByQuality;
    public float? m_weight;
    public float? m_scaleWeightByQuality;
    public int? m_value;
    public bool? m_teleportable;
    public int? m_toolTier;
    public string[]? m_icons;
    public HitData.DamageTypes? m_damages;
    public float? m_attackForce;
    public float? m_backstabBonus;
    public bool? m_dodgeable;
    public bool? m_blockable;
    public bool? m_tamedOnly;
    public string? m_attackStatusEffect;
    [DefaultValue(1f)] public float? m_attackStatusEffectChance;
    public string? m_spawnOnHit;
    public string? m_spawnOnHitTerrain;
    public string? m_ammoType;
    public AttackRef? m_attack;
    public AttackRef? m_secondaryAttack;
    public float? m_useDurability;
    public bool? m_destroyBroken;
    public bool? m_canBeRepaired;
    public float? m_maxDurability;
    public float? m_durabilityPerLevel;
    public float m_useDurabilityDrain;
    public float? m_aiAttackRange;
    public float? m_aiAttackRangeMin;
    public float? m_aiAttackInterval;
    public float? m_aiAttackMaxAngle;
    public bool? m_aiInvertAngleCheck;
    public bool? m_aiWhenFlying;
    public float? m_aiWhenFlyingAltitudeMin;
    [DefaultValue(999999f)] public float? m_aiWhenFlyingAltitudeMax;
    public bool? m_aiWhenWalking;
    public bool? m_aiWhenSwiming;
    public bool? m_aiPrioritized;
    public bool? m_aiInDungeonOnly;
    public bool? m_aiInMistOnly;
    [DefaultValue(1f)] public float? m_aiMaxHealthPercentage;
    public float? m_aiMinHealthPercentage;
    public ItemDrop.ItemData.AiTarget? m_aiTargetType;
    public EffectListRef? m_hitEffect;
    public EffectListRef? m_hitTerrainEffect;
    public EffectListRef? m_blockEffect ;
    public EffectListRef? m_startEffect ;
    public EffectListRef? m_holdStartEffect ;
    public EffectListRef? m_triggerEffect ;
    public EffectListRef? m_trailStartEffect ;
    
    public ItemDataSharedRef(){}
    public ItemDataSharedRef(ItemDrop.ItemData.SharedData sharedData) => Setup(sharedData);

    public void SetBasicFields(ItemDrop.ItemData.SharedData d)
    {
        m_name = d.m_name;
        m_description = d.m_description;
        m_maxStackSize = d.m_maxStackSize;
        m_maxQuality = d.m_maxQuality;
        m_scaleByQuality = d.m_scaleByQuality;
        m_weight = d.m_weight;
        m_scaleWeightByQuality = d.m_scaleWeightByQuality;
        m_value = d.m_value;
        m_teleportable = d.m_teleportable;
        m_icons = d.m_icons.Select(x => x.name).ToArray();
    }
}

public static partial class Extensions
{
    public static List<string> ToItemNameList(this List<ItemDrop> items)
    {
        List<string> list = items
            .Where(x => x != null)
            .Select(x => x.name)
            .ToList();
        return list;
    }

    public static List<ItemDrop> FromRef(this List<string> il)
    {
        List<ItemDrop> items = il
            .Select(x => PrefabManager.GetPrefab(x)!)
            .Where(x => x != null)
            .Select(x => x.GetComponent<ItemDrop>())
            .Where(x => x != null)
            .ToList();
        return items;
    }

    public static string[] ToSpriteNameArray(this Sprite[] icons)
    {
        return icons.Select(x => x.name).ToArray();
    }

    public static Sprite[] ToSpriteArray(this string[] spriteNames)
    {
        return spriteNames
            .Select(x => TextureManager.GetSprite(x, null)!)
            .Where(x => x != null)
            .ToArray();
    }
}