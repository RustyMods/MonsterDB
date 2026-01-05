using System;
using JetBrains.Annotations;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class EggGrowRef : Reference
{
    public float? m_growTime;
    public string? m_grownPrefab;
    public bool? m_tamed;
    public float? m_updateInterval;
    public bool? m_requireNearbyFire;
    public bool? m_requireUnderRoof;
    public float? m_requireCoverPercentige;
    public EffectListRef? m_hatchEffect;
    public string? m_growingObject;
    public string? m_notGrowingObject;
}