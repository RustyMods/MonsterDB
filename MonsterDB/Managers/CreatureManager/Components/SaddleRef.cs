using System;
using JetBrains.Annotations;

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

    public static implicit operator SaddleRef(Sadle saddle)
    {
        SaddleRef reference = new SaddleRef();
        reference.ReferenceFrom(saddle);
        return reference;
    }
}