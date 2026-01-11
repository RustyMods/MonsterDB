using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class MonsterAIRef : BaseAIRef
{
    public float? m_alertRange;
    public bool? m_fleeIfHurtWhenTargetCantBeReached;
    public float? m_fleeUnreachableSinceAttacking;
    public float? m_fleeUnreachableSinceHurt;
    public bool? m_fleeIfNotAlerted;
    public float? m_fleeIfLowHealth;
    public float? m_fleeTimeSinceHurt;
    public bool? m_fleeInLava;
    public float? m_fleePheromoneMin;
    public float? m_fleePheromoneMax;
    public bool? m_circulateWhileCharging;
    public bool? m_circulateWhileChargingFlying;
    public bool? m_enableHuntPlayer;
    public bool? m_attackPlayerObjects;
    public int? m_privateAreaTriggerTreshold;
    public float? m_interceptTimeMax;
    public float? m_interceptTimeMin;
    public float? m_maxChaseDistance;
    public float? m_minAttackInterval;
    public float? m_circleTargetInterval;
    public float? m_circleTargetDuration;
    public float? m_circleTargetDistance;
    public bool? m_sleeping;
    public float? m_wakeupRange;
    public bool? m_noiseWakeup;
    public float? m_maxNoiseWakeupRange;
    public EffectListRef? m_wakeupEffects;
    public EffectListRef? m_sleepEffects;
    public float? m_wakeUpDelayMin;
    public float? m_wakeUpDelayMax;
    public float? m_fallAsleepDistance;
    public bool? m_avoidLand;
    [YamlMember(Description = "Tameable")]
    public List<string>? m_consumeItems;
    public float? m_consumeRange;
    public float? m_consumeSearchRange;
    public float? m_consumeSearchInterval;
}