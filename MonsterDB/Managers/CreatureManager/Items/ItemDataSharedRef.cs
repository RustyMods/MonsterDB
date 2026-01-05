using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class ItemDataSharedRef : Reference
{
    public string m_prefab = "";
    public int? m_toolTier;
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
    public AttackRef? m_attack;
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
}

public static partial class Extensions
{
    public static List<string> ToRef(this List<ItemDrop> items)
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
}