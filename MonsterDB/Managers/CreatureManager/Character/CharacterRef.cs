using System;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class CharacterRef : Reference
{
    [YamlMember(Order = 0)]  public string? m_name;
    [YamlMember(Order = 1)]  public string? m_group;
    [YamlMember(Order = 2)]  public Character.Faction? m_faction;
    [YamlMember(Order = 3)]  public bool? m_boss;
    [YamlMember(Order = 4)]  public bool? m_dontHideBossHud;
    [YamlMember(Order = 5)]  public string? m_bossEvent;
    [YamlMember(Order = 6)]  public string? m_defeatSetGlobalKey;
    [YamlMember(Order = 7)]  public bool? m_aiSkipTarget;

    [YamlMember(Order = 8, Description = "Speed")]  public float? m_crouchSpeed;
    [YamlMember(Order = 9)]  public float? m_walkSpeed;
    [YamlMember(Order = 10)] public float? m_speed;
    [YamlMember(Order = 11)] public float? m_turnSpeed;
    [YamlMember(Order = 12)] public float? m_runSpeed;
    [YamlMember(Order = 13)] public float? m_runTurnSpeed;
    [YamlMember(Order = 14)] public float? m_acceleration;

    [YamlMember(Order = 15, Description = "Jumping")] public float? m_jumpForce;
    [YamlMember(Order = 16)] public float? m_jumpForceForward;
    [YamlMember(Order = 17)] public float? m_jumpForceTiredFactor;
    [YamlMember(Order = 18)] public float? m_airControl;
    [YamlMember(Order = 19)] public float? m_jumpStaminaUsage;

    [YamlMember(Order = 20, Description = "Flying")] public bool? m_flying;
    [YamlMember(Order = 21)] public float? m_flySlowSpeed;
    [YamlMember(Order = 22)] public float? m_flyFastSpeed;
    [YamlMember(Order = 23)] public float? m_flyTurnSpeed;

    [YamlMember(Order = 24)] public bool? m_canSwim;
    [YamlMember(Order = 25)] public float? m_swimDepth;
    [YamlMember(Order = 26)] public float? m_swimSpeed;
    [YamlMember(Order = 27)] public float? m_swimTurnSpeed;
    [YamlMember(Order = 28)] public float? m_swimAcceleration;

    [YamlMember(Order = 29)] public Character.GroundTiltType? m_groundTilt;
    [YamlMember(Order = 30)] public float? m_groundTiltSpeed;

    [YamlMember(Order = 31, Description = "Gravity disabled while sleeping")] public bool? m_disableWhileSleeping;
    [YamlMember(Order = 32)] public bool? m_useAltStatusEffectScaling;
    [YamlMember(Order = 33)] public bool? m_tolerateWater;
    [YamlMember(Order = 34)] public bool? m_tolerateFire;
    [YamlMember(Order = 35)] public bool? m_tolerateSmoke;
    [YamlMember(Order = 36)] public bool? m_tolerateTar;

    [YamlMember(Order = 37, Description = "Effects")] public EffectListRef? m_hitEffects;
    [YamlMember(Order = 38)] public EffectListRef? m_critHitEffects;
    [YamlMember(Order = 39)] public EffectListRef? m_backstabHitEffects;
    [YamlMember(Order = 40)] public EffectListRef? m_deathEffects;
    [YamlMember(Order = 41)] public EffectListRef? m_waterEffects;
    [YamlMember(Order = 42)] public EffectListRef? m_tarEffects;
    [YamlMember(Order = 43)] public EffectListRef? m_slideEffects;
    [YamlMember(Order = 44)] public EffectListRef? m_jumpEffects;
    [YamlMember(Order = 45)] public EffectListRef? m_flyingContinuousEffect;
    [YamlMember(Order = 46)] public EffectListRef? m_pheromoneLoveEffect;

    [YamlMember(Order = 47, Description = "Base Health")] public float? m_health;
    [YamlMember(Order = 48)] public float? m_regenAllHPTime;
    [YamlMember(Order = 49)] public HitData.DamageModifiers? m_damageModifiers;
    [YamlMember(Order = 50)] public bool? m_staggerWhenBlocked;
    [YamlMember(Order = 51)] public float? m_staggerDamageFactor;
    [YamlMember(Order = 52)] public float? m_enemyAdrenalineMultiplier;

    [YamlMember(Order = 53, Description = "Lava")] public float? m_minLavaMaskThreshold;
    [YamlMember(Order = 54)] public float? m_heatBuildupBase;
    [YamlMember(Order = 55)] public float? m_heatCooldownBase;
    [YamlMember(Order = 56)] public float? m_heatBuildupWater;
    [YamlMember(Order = 57)] public float? m_heatWaterTouchMultiplier;
    [YamlMember(Order = 58)] public float? m_lavaDamageTickInterval;
    [YamlMember(Order = 59)] public float? m_heatLevelFirstDamageThreshold;
    [YamlMember(Order = 60)] public float? m_lavaFirstDamage;
    [YamlMember(Order = 61)] public float? m_lavaFullDamage;
    [YamlMember(Order = 62)] public float? m_lavaAirDamageHeight;
    [YamlMember(Order = 63)] public float? m_dayHeatGainRunning;
    [YamlMember(Order = 64)] public float? m_dayHeatGainStill;
    [YamlMember(Order = 65)] public float? m_dayHeatEquipmentStop;
    [YamlMember(Order = 66)] public float? m_lavaSlowMax;
    [YamlMember(Order = 67)] public float? m_lavaSlowHeight;
    [YamlMember(Order = 68)] public EffectListRef? m_lavaHeatEffects;
}