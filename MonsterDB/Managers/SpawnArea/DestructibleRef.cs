using System;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class DestructibleRef : Reference
{
    public DestructibleType? m_destructibleType = DestructibleType.Default;
     public float? m_health = 1f;
    public HitData.DamageModifiers? m_damages;
    [YamlMember(Alias = "m_minDamageThreshold")] public float? m_minDamageTreshold;
     public int? m_minToolTier;
     public float? m_hitNoise;
     public float? m_destroyNoise;
     public bool? m_triggerPrivateArea;
     public float? m_ttl;
    public string? m_spawnWhenDestroyed;
    [YamlMember(Description = "Effects")] 
    public EffectListRef? m_destroyedEffect;
    public EffectListRef? m_hitEffect;
     public bool? m_autoCreateFragments;

    public DestructibleRef(){}
    public DestructibleRef(Destructible component) => Setup(component);
}