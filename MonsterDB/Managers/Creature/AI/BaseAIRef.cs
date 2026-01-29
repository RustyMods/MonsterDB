using System;
using System.ComponentModel;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class BaseAIRef : Reference
{
    [DefaultValue(50f)] public float? m_viewRange;
    [DefaultValue(90f)] public float? m_viewAngle;
    [DefaultValue(9999f)] public float? m_hearRange;
    [DefaultValue(false)] public bool? m_mistVision;
    
    public EffectListRef? m_alertedEffects;
    public EffectListRef? m_idleSound;
    [DefaultValue(5f)] public float? m_idleSoundInterval;
    [DefaultValue(0.5f)] public float? m_idleSoundChance;
    
    public Pathfinding.AgentType? m_pathAgentType;
    [DefaultValue(10f)] public float? m_moveMinAngle;
    [DefaultValue(true)] public bool? m_smoothMovement;
    [DefaultValue(false)] public bool? m_serpentMovement;
    [DefaultValue(20f)] public float? m_serpentTurnRadius;
    [DefaultValue(0f)] public float? m_jumpInterval;
    
    [DefaultValue(2f)] public float? m_randomCircleInterval;
    [DefaultValue(5f)] public float? m_randomMoveInterval;
    [DefaultValue(4f)] public float? m_randomMoveRange;
    [DefaultValue(false)] public bool? m_randomFly;
    
    [DefaultValue(1f)] public float? m_chanceToTakeoff;
    [DefaultValue(1f)] public float? m_chanceToLand;
    
    [YamlMember(Description = "Flying")]
    [DefaultValue(10f)] public float? m_groundDuration;
    [DefaultValue(10f)] public float? m_airDuration;
    [DefaultValue(5f)] public float? m_maxLandAltitude;
    [DefaultValue(5f)] public float? m_takeoffTime;
    [DefaultValue(3f)] public float? m_flyAltitudeMin;
    [DefaultValue(10f)] public float? m_flyAltitudeMax;
    [DefaultValue(32f)] public float? m_flyAbsMinAltitude;
    
    [DefaultValue(false)] public bool? m_avoidFire;
    [DefaultValue(false)] public bool? m_afraidOfFire;
    [DefaultValue(true)] public bool? m_avoidWater;
    [DefaultValue(true)] public bool? m_avoidLava;
    [DefaultValue(false)] public bool? m_skipLavaTargets;
    [DefaultValue(true)] public bool? m_avoidLavaFlee;
    
    [DefaultValue(false)] public bool? m_aggravatable;
    [DefaultValue(false)] public bool? m_passiveAggresive;
    
    public string? m_spawnMessage;
    public string? m_deathMessage;
    public string? m_alertedMessage;
    
    [DefaultValue(25f)] public float? m_fleeRange;
    [DefaultValue(45f)] public float? m_fleeAngle;
    [DefaultValue(2f)] public float? m_fleeInterval;
    [DefaultValue(false)] public bool? m_patrol;
}