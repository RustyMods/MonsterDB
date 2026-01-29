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
    [DefaultValue(0f)] public float? m_attackHealthReturnHit;
    [DefaultValue(false)] public bool? m_attackKillsSelf;
    [DefaultValue(0f)] public float? m_speedFactor;
    [DefaultValue(0f)] public float? m_speedFactorRotation;
    [DefaultValue(0f)] public float? m_attackStartNoise;
    [DefaultValue(0f)] public float? m_attackHitNoise;
    [DefaultValue(1f)] public float? m_damageMultiplier;
    [YamlMember(Description = "For each missing health point, increase damage this much.")]
    [DefaultValue(0f)] public float? m_damageMultiplierPerMissingHP;
    [YamlMember(Description = "At 100% missing HP the damage will increase by this much, and gradually inbetween.")]
    [DefaultValue(0f)] public float? m_damageMultiplierByTotalHealthMissing;
    [YamlMember(Description = "For each missing health point, return one stamina point.")]
    [DefaultValue(0f)] public float? m_staminaReturnPerMissingHP;
    [DefaultValue(1f)] public float? m_forceMultiplier;
    [DefaultValue(1f)] public float? m_staggerMultiplier;
    [DefaultValue(0f)] public float? m_recoilPushback;
    [DefaultValue(0)] public int? m_selfDamage;
    public string? m_attackOriginJoint;
    [DefaultValue(1.5f)] public float? m_attackRange;
    [DefaultValue(0.6f)] public float? m_attackHeight;
    [DefaultValue(0f)] public float? m_attackHeightChar1;
    [DefaultValue(0f)] public float? m_attackHeightChar2;
    [DefaultValue(0f)] public float? m_attackOffset;
    public string? m_spawnOnTrigger;
    [DefaultValue(false)] public bool? m_toggleFlying;
    [DefaultValue(false)] public bool? m_attach;
    [DefaultValue(false)] public bool? m_cantUseInDungeon;
    [YamlMember(Description = "Loading")]
    [DefaultValue(false)] public bool? m_requiresReload;
    public string? m_reloadAnimation;
    [DefaultValue(2f)] public float? m_reloadTime;
    [YamlMember(Description = "Draw")]
    [DefaultValue(false)] public bool? m_bowDraw;
    [DefaultValue(0f)] public float? m_drawDurationMin;
    [DefaultValue(0f)] public float? m_drawStaminaDrain;
    [DefaultValue(0f)] public float? m_drawEitrDrain;
    public string? m_drawAnimationState;
    [YamlMember(Description = "Melee/AOE")]
    [DefaultValue(90f)] public float? m_attackAngle;
    [DefaultValue(0f)] public float? m_attackRayWidth;
    [DefaultValue(0f)] public float? m_attackRayWidthCharExtra;
    [DefaultValue(0f)] public float? m_maxYAngle;
    [DefaultValue(true)] public bool? m_lowerDamagePerHit;
    [YamlMember(Alias = "m_hitPointType")] public Attack.HitPointType? m_hitPointtype;
    [DefaultValue(false)] public bool? m_hitThroughWalls;
    [DefaultValue(true)] public bool? m_multiHit;
    [DefaultValue(false)] public bool? m_pickaxeSpecial;
    [DefaultValue(2f)] public float? m_lastChainDamageMultiplier;
    public DestructibleType? m_resetChainIfHit;
    public string? m_spawnOnHit;
    public float? m_spawnOnHitChance;
    [DefaultValue(1f)] public float? m_raiseSkillAmount;
    public DestructibleType? m_skillHitType;
    public Skills.SkillType? m_specialHitSkill;
    [YamlMember(Description = "Projectile")]
    public string? m_attackProjectile;
    [DefaultValue(10f)] public float? m_projectileVel;
    [DefaultValue(2f)] public float? m_projectileVelMin;
    [YamlMember(Description = "When not using Draw, randomize velocity between Velocity and Velocity Min")]
    [DefaultValue(false)] public bool? m_randomVelocity;
    [DefaultValue(10f)] public float? m_projectileAccuracy;
    [DefaultValue(20f)] public float? m_projectileAccuracyMin;
    [DefaultValue(false)] public bool? m_circularProjectileLaunch;
    [DefaultValue(false)] public bool? m_distributeProjectilesAroundCircle;
    [DefaultValue(false)] public bool? m_skillAccuracy;
    [DefaultValue(false)] public bool? m_useCharacterFacing;
    [DefaultValue(false)] public bool? m_useCharacterFacingYAim;
    [DefaultValue(0f)] public float? m_launchAngle;
    [DefaultValue(1)] public int? m_projectiles;
    [DefaultValue(1)] public int? m_projectileBursts;
    [DefaultValue(0f)] public float? m_burstInterval;
    [DefaultValue(false)] public bool? m_destroyPreviousProjectile;
    [DefaultValue(false)] public bool? m_perBurstResourceUsage;
    [DefaultValue(false)] public bool? m_harvest;
    [DefaultValue(0f)] public float? m_harvestRadius;
    [DefaultValue(0f)] public float? m_harvestRadiusMaxLevel;
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