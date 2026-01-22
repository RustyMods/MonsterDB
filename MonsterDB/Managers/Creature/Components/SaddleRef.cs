using System;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class SaddleRef : Reference
{
    public string? m_hoverText;
    public float? m_maxUseRange;
    public Vector3Ref? m_detachOffset;
    public string? m_attachAnimation;
    public float? m_maxStamina;
    public float? m_runStaminaDrain;
    public float? m_swimStaminaDrain;
    public float? m_staminaRegen;
    public float? m_staminaRegenHungry;
    public EffectListRef? m_drownEffects;
    public string? m_mountIcon;
    [YamlMember(Description = "If added saddle by MonsterDB, use this to position attach point")] 
    public Vector3Ref? m_attachOffset;
    
    public SaddleRef(){}
    public SaddleRef(Sadle component) => Setup(component);
}