using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class AoeRef : Reference
{
    public string m_name = "";
    [YamlMember(Description = "Attack (overridden by item)")] 
    public bool? m_useAttackSettings;
    public HitData.DamageTypes? m_damage;
    public bool? m_scaleDamageByDistance;
    public bool? m_dodgeable;
    public bool? m_blockable;
    public int? m_toolTier;
    public float? m_attackForce;
    public float? m_backstabBonus;
    public string? m_statusEffect;
    public string? m_statusEffectIfBoss ;
    public bool? m_attackForceForward;
    public string? m_spawnOnHitTerrain ;
    public bool? m_hitTerrainOnlyOnce;
    public FootStep.GroundMaterial? m_spawnOnGroundType;
    public float? m_groundLavaValue ;
    public float? m_hitNoise;
    public bool? m_placeOnGround;
    public bool? m_randomRotation;
    public int? m_maxTargetsFromCenter;
    [YamlMember(Description = "Multi Spawn (Lava Bomb)")] 
    public int? m_multiSpawnMin;
    public int? m_multiSpawnMax;
    public float? m_multiSpawnDistanceMin;
    public float? m_multiSpawnDistanceMax;
    public float? m_multiSpawnScaleMin;
    public float? m_multiSpawnScaleMax;
    public float? m_multiSpawnSpringDelayMax;
    [YamlMember(Description = "Chain Spawn")] 
    public float? m_chainStartChance;
    public float? m_chainStartChanceFalloff ;
    public float? m_chainChancePerTarget;
    public string? m_chainObj;
    public float? m_chainStartDelay;
    public int? m_chainMinTargets;
    public int? m_chainMaxTargets;
    public EffectListRef? m_chainEffects;
    public float? m_chainDelay;
    public float? m_chainChance;
    public float? m_damageSelf;
    [YamlMember(Description = "Ignore targets")]
    public bool? m_hitOwner;
    public bool? m_hitParent;
    public bool? m_hitSame;
    public bool? m_hitFriendly;
    public bool? m_hitEnemy;
    public bool? m_hitCharacters;
    public bool? m_hitProps;
    public bool? m_hitTerrain;
    public bool? m_launchCharacters;
    public float? m_launchForceUpFactor;
    public bool? m_useTriggers;
    public bool? m_triggerEnterOnly;
    public float? m_radius ;
    [YamlMember(Description = "Wait this long before we start doing any damage")]
    public float? m_activationDelay;
    public float? m_ttl ;
    [YamlMember(Description = "When set, ttl will be a random value between tll and ttlMax")]
    public float? m_ttlMax;
    public bool? m_hitAfterTtl;
    public float? m_hitInterval ;
    public bool? m_hitOnEnable;
    public bool? m_attachToCaster;
    public EffectListRef? m_hitEffects;
    public EffectListRef? m_initiateEffect;
    
    public AoeRef(){}

    public AoeRef(Aoe aoe) => Setup(aoe);
}