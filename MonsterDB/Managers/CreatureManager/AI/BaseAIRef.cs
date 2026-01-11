using System;
using JetBrains.Annotations;
using UnityEngine.Internal;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class BaseAIRef : Reference
{
    public float? m_viewRange;
    public float? m_viewAngle;
    public float? m_hearRange;
    public bool? m_mistVision;
    
    public EffectListRef? m_alertedEffects;
    public EffectListRef? m_idleSound;
    public float? m_idleSoundInterval;
    public float? m_idleSoundChance;
    
    public Pathfinding.AgentType? m_pathAgentType;
    public float? m_moveMinAngle;
    public bool? m_smoothMovement;
    public bool? m_serpentMovement;
    public float? m_serpentTurnRadius;
    public float? m_jumpInterval;
    
    public float? m_randomCircleInterval;
    public float? m_randomMoveInterval;
    public float? m_randomMoveRange;
    public bool? m_randomFly;
    
    public float? m_chanceToTakeoff;
    public float? m_chanceToLand;
    
    [YamlMember(Description = "Flying")]
    public float? m_groundDuration;
    public float? m_airDuration;
    public float? m_maxLandAltitude;
    public float? m_takeoffTime;
    public float? m_flyAltitudeMin;
    public float? m_flyAltitudeMax;
    public float? m_flyAbsMinAltitude;
    
    public bool? m_avoidFire;
    public bool? m_afraidOfFire;
    public bool? m_avoidWater;
    public bool? m_avoidLava;
    public bool? m_skipLavaTargets;
    public bool? m_avoidLavaFlee;
    
    public bool? m_aggravatable;
    public bool? m_passiveAggresive;
    
    public string? m_spawnMessage;
    public string? m_deathMessage;
    public string? m_alertedMessage;
    
    public float? m_fleeRange;
    public float? m_fleeAngle;
    public float? m_fleeInterval;
    public bool? m_patrol;
}