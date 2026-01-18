using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class TameableRef : Reference
{
    public float? m_fedDuration;
    public float? m_tamingTime;
    public bool? m_startsTamed;
    public EffectListRef? m_tamedEffect;
    public EffectListRef? m_sootheEffect;
    public EffectListRef? m_petEffect;
    public bool? m_commandable;
    public float? m_unsummonDistance;
    public float? m_unsummonOnOwnerLogoutSeconds;
    public EffectListRef? m_unSummonEffect;
    public Skills.SkillType? m_levelUpOwnerSkill;
    public float? m_levelUpFactor;
    public string? m_saddleItem;
    public bool? m_dropSaddleOnDeath;
    public Vector3Ref? m_dropSaddleOffset;
    public float? m_dropItemVel;
    public List<string>? m_randomStartingName;
    public float? m_tamingSpeedMultiplierRange;
    public float? m_tamingBoostMultiplier;
    public bool? m_nameBeforeText;
    public string? m_tameText;

    public static implicit operator TameableRef(Tameable tameable)
    {
        TameableRef reference = new TameableRef();
        reference.Setup(tameable);
        return reference;
    }
}