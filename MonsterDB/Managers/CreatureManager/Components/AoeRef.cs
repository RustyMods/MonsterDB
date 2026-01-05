using System;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class AoeRef : Reference
{
    public string m_name = "";
    public bool? m_useAttackSettings = true;
    public HitData.DamageTypes? m_damage;
    public bool? m_scaleDamageByDistance;
    public bool? m_dodgeable;
    public bool? m_blockable;
    public int? m_toolTier;
    public float? m_attackForce;
    public float? m_backstabBonus = 4f;
    public string? m_statusEffect = "";
    public string? m_statusEffectIfBoss = "";
    public bool? m_attackForceForward;
    public string? m_spawnOnHitTerrain = "";
    public bool? m_hitTerrainOnlyOnce;
    public FootStep.GroundMaterial? m_spawnOnGroundType;
    public float? m_groundLavaValue = -1f;
    public float? m_hitNoise;
    public bool? m_placeOnGround;
    public bool? m_randomRotation;
    public int? m_maxTargetsFromCenter;
    public int? m_multiSpawnMin;
    public int? m_multiSpawnMax;
    public float? m_multiSpawnDistanceMin;
    public float? m_multiSpawnDistanceMax;
    public float? m_multiSpawnScaleMin;
    public float? m_multiSpawnScaleMax;
    public float? m_multiSpawnSpringDelayMax;
    public float? m_chainStartChance;
    public float? m_chainStartChanceFalloff = 0.8f;
    public float? m_chainChancePerTarget;
    public string? m_chainObj = "";
    public float? m_chainStartDelay;
    public int? m_chainMinTargets;
    public int? m_chainMaxTargets;
    public EffectListRef? m_chainEffects;
    public float? m_chainDelay;
    public float? m_chainChance;
    public float? m_damageSelf;
    public bool? m_hitOwner;
    public bool? m_hitParent = true;
    public bool? m_hitSame;
    public bool? m_hitFriendly = true;
    public bool? m_hitEnemy = true;
    public bool? m_hitCharacters = true;
    public bool? m_hitProps = true;
    public bool? m_hitTerrain;
    public bool? m_launchCharacters;
    public float? m_launchForceUpFactor = 0.5f;
    public bool? m_useTriggers;
    public bool? m_triggerEnterOnly;
    public float? m_radius = 4f;
    public float? m_activationDelay;
    public float? m_ttl = 4f;
    public float? m_ttlMax;
    public bool? m_hitAfterTtl;
    public float? m_hitInterval = 1f;
    public bool? m_hitOnEnable;
    public bool? m_attachToCaster;
    public EffectListRef? m_hitEffects;
    public EffectListRef? m_initiateEffect;
}

public static partial class Extensions
{
    public static AoeRef ToRef(this Aoe aoe)
    {
        var aoeRef = new AoeRef();
        aoeRef.ReferenceFrom(aoe);
        return aoeRef;
    }
}