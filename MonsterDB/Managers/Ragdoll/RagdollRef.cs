using System;
using System.ComponentModel;

namespace MonsterDB;

[Serializable]
public class RagdollRef : Reference
{
    public float? m_velMultiplier;
    public float? m_ttl;
    public EffectListRef? m_removeEffect;
    public bool? m_float;
    public float? m_floatOffset;
    [DefaultValue(true)] public bool? m_dropItems;
    
    public RagdollRef(){}
    public RagdollRef(Ragdoll doll) => Setup(doll);
}