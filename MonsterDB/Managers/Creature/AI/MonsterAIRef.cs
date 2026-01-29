using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class MonsterAIRef : BaseAIRef
{
    [DefaultValue(9999f)] public float? m_alertRange;
    [DefaultValue(true)] public bool? m_fleeIfHurtWhenTargetCantBeReached;
    [DefaultValue(30f)] public float? m_fleeUnreachableSinceAttacking;
    [DefaultValue(20f)] public float? m_fleeUnreachableSinceHurt;
    [DefaultValue(false)] public bool? m_fleeIfNotAlerted;
    [DefaultValue(0f)] public float? m_fleeIfLowHealth;
    [DefaultValue(20f)] public float? m_fleeTimeSinceHurt;
    [DefaultValue(true)] public bool? m_fleeInLava;
    [DefaultValue(3f)] public float? m_fleePheromoneMin;
    [DefaultValue(8f)] public float? m_fleePheromoneMax;
    [DefaultValue(false)] public bool? m_circulateWhileCharging;
    [DefaultValue(false)] public bool? m_circulateWhileChargingFlying;
    [DefaultValue(false)] public bool? m_enableHuntPlayer;
    [DefaultValue(true)] public bool? m_attackPlayerObjects;
    [YamlMember(Alias = "m_privateAreaTriggerThreshold")] [DefaultValue(4)] public int? m_privateAreaTriggerTreshold;
    [DefaultValue(0f)] public float? m_interceptTimeMax;
    [DefaultValue(0f)] public float? m_interceptTimeMin;
    [DefaultValue(0f)] public float? m_maxChaseDistance;
    [DefaultValue(0f)] public float? m_minAttackInterval;
    [DefaultValue(0f)] public float? m_circleTargetInterval;
    [DefaultValue(5f)] public float? m_circleTargetDuration;
    [DefaultValue(10f)] public float? m_circleTargetDistance;
    [DefaultValue(false)] public bool? m_sleeping;
    [DefaultValue(5f)] public float? m_wakeupRange;
    [DefaultValue(false)] public bool? m_noiseWakeup;
    [DefaultValue(50f)] public float? m_maxNoiseWakeupRange;
    public EffectListRef? m_wakeupEffects;
    public EffectListRef? m_sleepEffects;
    [DefaultValue(0f)] public float? m_wakeUpDelayMin;
    [DefaultValue(0f)] public float? m_wakeUpDelayMax;
    [DefaultValue(0f)] public float? m_fallAsleepDistance;
    [DefaultValue(false)] public bool? m_avoidLand;
    [YamlMember(Description = "Tameable")]
    public List<string>? m_consumeItems;
    [DefaultValue(2f)] public float? m_consumeRange;
    [DefaultValue(5f)] public float? m_consumeSearchRange;
    [DefaultValue(10f)] public float? m_consumeSearchInterval;
    
    public MonsterAIRef(){}
    public MonsterAIRef(MonsterAI ai) => Setup(ai);
}