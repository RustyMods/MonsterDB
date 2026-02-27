using System;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class AttackRef : Reference
{
    public Attack.AttackType? m_attackType;
    public string? m_attackAnimation;
    public string? m_chargeAnimationBool;
    public int? m_attackRandomAnimations;
    public int? m_attackChainLevels;
    public bool? m_loopingAttack;
    public bool? m_consumeItem;
    public bool? m_hitTerrain;
    public float? m_attackHealthReturnHit;
    public bool? m_attackKillsSelf;
    public float? m_speedFactor;
    public float? m_speedFactorRotation;
    public float? m_attackStartNoise;
    public float? m_attackHitNoise;
    public float? m_damageMultiplier;
    [YamlMember(Description = "For each missing health point, increase damage this much.")]
    public float? m_damageMultiplierPerMissingHP;
    [YamlMember(Description = "At 100% missing HP the damage will increase by this much, and gradually inbetween.")]
    public float? m_damageMultiplierByTotalHealthMissing;
    [YamlMember(Description = "For each missing health point, return one stamina point.")]
    public float? m_staminaReturnPerMissingHP;
    public float? m_forceMultiplier;
    public float? m_staggerMultiplier;
    public float? m_recoilPushback;
    public int? m_selfDamage;
    public string? m_attackOriginJoint;
    public float? m_attackRange;
    public float? m_attackHeight;
    public float? m_attackHeightChar1;
    public float? m_attackHeightChar2;
    public float? m_attackOffset;
    public string? m_spawnOnTrigger;
    public bool? m_toggleFlying;
    public bool? m_attach;
    public bool? m_cantUseInDungeon;
    [YamlMember(Description = "Loading")]
    public bool? m_requiresReload;
    public string? m_reloadAnimation;
    public float? m_reloadTime;
    [YamlMember(Description = "Draw")]
    public bool? m_bowDraw;
    public float? m_drawDurationMin;
    public float? m_drawStaminaDrain;
    public float? m_drawEitrDrain;
    public string? m_drawAnimationState;
    [YamlMember(Description = "Melee/AOE")]
    public float? m_attackAngle;
    public float? m_attackRayWidth;
    public float? m_attackRayWidthCharExtra;
    public float? m_maxYAngle;
    public bool? m_lowerDamagePerHit;
    [YamlMember(Alias = "m_hitPointType")] public Attack.HitPointType? m_hitPointtype;
    public bool? m_hitThroughWalls;
    public bool? m_multiHit;
    public bool? m_pickaxeSpecial;
    public float? m_lastChainDamageMultiplier;
    public DestructibleType? m_resetChainIfHit;
    public string? m_spawnOnHit;
    public float? m_spawnOnHitChance;
    public float? m_raiseSkillAmount;
    public DestructibleType? m_skillHitType;
    public Skills.SkillType? m_specialHitSkill;
    [YamlMember(Description = "Projectile")]
    public string? m_attackProjectile;
    public float? m_projectileVel;
    public float? m_projectileVelMin;
    [YamlMember(Description = "When not using Draw, randomize velocity between Velocity and Velocity Min")]
    public bool? m_randomVelocity;
    public float? m_projectileAccuracy;
    public float? m_projectileAccuracyMin;
    public bool? m_circularProjectileLaunch;
    public bool? m_distributeProjectilesAroundCircle;
    public bool? m_skillAccuracy;
    public bool? m_useCharacterFacing;
    public bool? m_useCharacterFacingYAim;
    public float? m_launchAngle;
    public int? m_projectiles;
    public int? m_projectileBursts;
    public float? m_burstInterval;
    public bool? m_destroyPreviousProjectile;
    public bool? m_perBurstResourceUsage;
    public bool? m_harvest;
    public float? m_harvestRadius;
    public float? m_harvestRadiusMaxLevel;
    [YamlMember(Description = "Attack Effects")]
    public EffectListRef? m_hitEffect;
    public EffectListRef? m_hitTerrainEffect;
    public EffectListRef? m_startEffect;
    public EffectListRef? m_triggerEffect;
    public EffectListRef? m_trailStartEffect;
    public EffectListRef? m_burstEffect;
}

public static partial class Extensions
{
    public static AttackRef ToAttackRef(this Attack att)
    {
        AttackRef attackRef = new AttackRef();
        attackRef.Setup(att);
        return attackRef;
    }
}