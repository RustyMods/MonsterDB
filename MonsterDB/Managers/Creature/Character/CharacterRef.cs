using System;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class CharacterRef : Reference
{
    [YamlMember(Order = 0)]  public string? m_name;
    [YamlMember(Order = 1)]  public string? m_group;
    [YamlMember(Order = 2)]  public Character.Faction? m_faction;
    [YamlMember(Order = 3)][DefaultValue(false)]   public bool? m_boss;
    [YamlMember(Order = 4)][DefaultValue(false)]   public bool? m_dontHideBossHud;
    [YamlMember(Order = 5)]  public string? m_bossEvent;
    [YamlMember(Order = 6)]  public string? m_defeatSetGlobalKey;
    [YamlMember(Order = 7)][DefaultValue(false)]   public bool? m_aiSkipTarget;

    [YamlMember(Order = 8, Description = "Speed")][DefaultValue(2f)]   public float? m_crouchSpeed;
    [YamlMember(Order = 9)][DefaultValue(5f)]   public float? m_walkSpeed;
    [YamlMember(Order = 10)][DefaultValue(10f)]  public float? m_speed;
    [YamlMember(Order = 11)][DefaultValue(300f)]  public float? m_turnSpeed;
    [YamlMember(Order = 12)][DefaultValue(20f)]  public float? m_runSpeed;
    [YamlMember(Order = 13)][DefaultValue(300f)]  public float? m_runTurnSpeed;
    [YamlMember(Order = 14)][DefaultValue(1f)]  public float? m_acceleration;

    [YamlMember(Order = 15, Description = "Jumping")][DefaultValue(10f)]  public float? m_jumpForce;
    [YamlMember(Order = 16)][DefaultValue(0f)]  public float? m_jumpForceForward;
    [YamlMember(Order = 17)][DefaultValue(0.7f)]  public float? m_jumpForceTiredFactor;
    [YamlMember(Order = 18)][DefaultValue(0.1f)]  public float? m_airControl;
    [YamlMember(Order = 19)][DefaultValue(10f)]  public float? m_jumpStaminaUsage;

    [YamlMember(Order = 20, Description = "Flying")][DefaultValue(false)]  public bool? m_flying;
    [YamlMember(Order = 21)][DefaultValue(5f)]  public float? m_flySlowSpeed;
    [YamlMember(Order = 22)][DefaultValue(12f)]  public float? m_flyFastSpeed;
    [YamlMember(Order = 23)][DefaultValue(12f)]  public float? m_flyTurnSpeed;

    [YamlMember(Order = 24)][DefaultValue(true)]  public bool? m_canSwim;
    [YamlMember(Order = 25)][DefaultValue(2f)]  public float? m_swimDepth;
    [YamlMember(Order = 26)][DefaultValue(2f)]  public float? m_swimSpeed;
    [YamlMember(Order = 27)][DefaultValue(100f)]  public float? m_swimTurnSpeed;
    [YamlMember(Order = 28)][DefaultValue(0.05f)]  public float? m_swimAcceleration;

    [YamlMember(Order = 29)] public Character.GroundTiltType? m_groundTilt;
    [YamlMember(Order = 30)][DefaultValue(50f)]  public float? m_groundTiltSpeed;

    [YamlMember(Order = 31, Description = "Gravity disabled while sleeping")][DefaultValue(false)]  public bool? m_disableWhileSleeping;
    [YamlMember(Order = 32)][DefaultValue(false)]  public bool? m_useAltStatusEffectScaling;
    [YamlMember(Order = 33)][DefaultValue(true)]  public bool? m_tolerateWater;
    [YamlMember(Order = 34)][DefaultValue(false)]  public bool? m_tolerateFire;
    [YamlMember(Order = 35)][DefaultValue(true)]  public bool? m_tolerateSmoke;
    [YamlMember(Order = 36)][DefaultValue(false)]  public bool? m_tolerateTar;

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
    [YamlMember(Order = 48)][DefaultValue(3600f)]  public float? m_regenAllHPTime;
    [YamlMember(Order = 49)] public HitData.DamageModifiers? m_damageModifiers;
    [YamlMember(Order = 50)][DefaultValue(true)]  public bool? m_staggerWhenBlocked;
    [YamlMember(Order = 51)][DefaultValue(0f)]  public float? m_staggerDamageFactor;
    [YamlMember(Order = 52)][DefaultValue(1f)]  public float? m_enemyAdrenalineMultiplier;

    [YamlMember(Order = 53, Description = "Lava")][DefaultValue(0.05f)]  public float? m_minLavaMaskThreshold;
    [YamlMember(Order = 54)][DefaultValue(1.5f)]  public float? m_heatBuildupBase;
    [YamlMember(Order = 55)][DefaultValue(1f)]  public float? m_heatCooldownBase;
    [YamlMember(Order = 56)][DefaultValue(2f)]  public float? m_heatBuildupWater;
    [YamlMember(Order = 57)][DefaultValue(0.2f)]  public float? m_heatWaterTouchMultiplier;
    [YamlMember(Order = 58)][DefaultValue(0.2f)]  public float? m_lavaDamageTickInterval;
    [YamlMember(Order = 59)][DefaultValue(0.7f)]  public float? m_heatLevelFirstDamageThreshold;
    [YamlMember(Order = 60)][DefaultValue(10f)]  public float? m_lavaFirstDamage;
    [YamlMember(Order = 61)][DefaultValue(100f)]  public float? m_lavaFullDamage;
    [YamlMember(Order = 62)][DefaultValue(3f)]  public float? m_lavaAirDamageHeight;
    [YamlMember(Order = 63)][DefaultValue(0.2f)]  public float? m_dayHeatGainRunning;
    [YamlMember(Order = 64)][DefaultValue(-0.05f)]  public float? m_dayHeatGainStill;
    [YamlMember(Order = 65)][DefaultValue(0.5f)]  public float? m_dayHeatEquipmentStop;
    [YamlMember(Order = 66)][DefaultValue(0.5f)]  public float? m_lavaSlowMax;
    [YamlMember(Order = 67)][DefaultValue(0.8f)]  public float? m_lavaSlowHeight;
    [YamlMember(Order = 68)] public EffectListRef? m_lavaHeatEffects;
    
    public CharacterRef(){}

    public CharacterRef(Character character) => Setup(character);
    
}