using System;
using System.Collections.Generic;

namespace MonsterDB;

[Serializable]
public class RRR_MonsterAI
{
    public float? fAlertRange ;
    public bool? bFleeIfHurtWhenTargetCantBeReached ;
    public bool? bFleeIfNotAlerted ;
    public float? fFleeIfLowHealth ;
    public bool? bCirculateWhileCharging ;
    public bool? bCirculateWhileChargingFlying ;
    public bool? bEnableHuntPlayer ;
    public bool? bAttackPlayerObjects ;
    public bool? bAttackPlayerObjectsWhenAlerted ;
    public float? fInterceptTimeMax ;
    public float? fInterceptTimeMin ;
    public float? fMaxChaseDistance ;
    public float? fMinAttackInterval ;
    public float? fCircleTargetInterval ;
    public float? fCircleTargetDuration ;
    public List<string>? aConsumeItems ;

    public void Setup(MonsterAIRef reference)
    {
        reference.m_alertRange = fAlertRange;
        reference.m_fleeIfHurtWhenTargetCantBeReached = bFleeIfHurtWhenTargetCantBeReached;
        reference.m_fleeIfNotAlerted = bFleeIfNotAlerted;
        reference.m_fleeIfLowHealth = fFleeIfLowHealth;
        reference.m_circulateWhileCharging = bCirculateWhileCharging;
        reference.m_circulateWhileChargingFlying = bCirculateWhileChargingFlying;
        reference.m_enableHuntPlayer = bEnableHuntPlayer;
        reference.m_attackPlayerObjects = bAttackPlayerObjects;
        reference.m_interceptTimeMax = fInterceptTimeMax;
        reference.m_interceptTimeMin = fInterceptTimeMin;
        reference.m_maxChaseDistance = fMaxChaseDistance;
        reference.m_minAttackInterval = fMinAttackInterval;
        reference.m_circleTargetInterval = fCircleTargetInterval;
        reference.m_circleTargetDuration = fCircleTargetDuration;
        reference.m_consumeItems = aConsumeItems;
    }
}