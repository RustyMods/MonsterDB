using System;
using System.ComponentModel;

namespace MonsterDB;

[Serializable]
public class RagdollRef : Reference
{
    [DefaultValue(1f)] public float? m_velMultiplier;
    public float? m_ttl;
    public EffectListRef? m_removeEffect;
    public bool? m_float;
    public float? m_floatOffset;
    [DefaultValue(true)] public bool? m_dropItems;
}