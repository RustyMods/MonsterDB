using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using JetBrains.Annotations;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class TameableRef : Reference
{
    [DefaultValue(30f)] public float? m_fedDuration;
    [DefaultValue(1800f)] public float? m_tamingTime;
    [DefaultValue(false)] public bool? m_startsTamed;
    public EffectListRef? m_tamedEffect;
    public EffectListRef? m_sootheEffect;
    public EffectListRef? m_petEffect;
    [DefaultValue(false)] public bool? m_commandable;
    [DefaultValue(0f)] public float? m_unsummonDistance;
    [DefaultValue(0f)] public float? m_unsummonOnOwnerLogoutSeconds;
    public EffectListRef? m_unSummonEffect;
    [DefaultValue("None")] public Skills.SkillType? m_levelUpOwnerSkill;
    [DefaultValue(0f)] public float? m_levelUpFactor;
    public string? m_saddleItem;
    [DefaultValue(true)] public bool? m_dropSaddleOnDeath;
    public Vector3Ref? m_dropSaddleOffset;
    [DefaultValue(5f)] public float? m_dropItemVel;
    public List<string>? m_randomStartingName;
    [DefaultValue(60f)] public float? m_tamingSpeedMultiplierRange;
    [DefaultValue(2f)] public float? m_tamingBoostMultiplier;
    [DefaultValue(true)] public bool? m_nameBeforeText;
    [DefaultValue("$hud_tamelove")] public string? m_tameText;
    
    public TameableRef(){}
    public TameableRef(Tameable component) => Setup(component);
}
