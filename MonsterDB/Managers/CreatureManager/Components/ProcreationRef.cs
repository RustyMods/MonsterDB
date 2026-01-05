using System;

namespace MonsterDB;

[Serializable]
public class ProcreationRef : Reference
{
    public float? m_updateInterval;
    public float? m_totalCheckRange;
    public int? m_maxCreatures;
    public float? m_partnerCheckRange;
    public float? m_pregnancyChance;
    public float? m_pregnancyDuration;
    public int? m_requiredLovePoints;
    public string? m_offspring;
    public int? m_minOffspringLevel;
    public float? m_spawnOffset;
    public float? m_spawnOffsetMax;
    public bool? m_spawnRandomDirection;
    public string? m_seperatePartner;
    public string? m_noPartnerOffspring;
    public EffectListRef? m_birthEffects;
    public EffectListRef? m_loveEffects;

    public static implicit operator ProcreationRef(Procreation procreation)
    {
        ProcreationRef reference = new ProcreationRef();
        reference.ReferenceFrom(procreation);
        return reference;
    }
}