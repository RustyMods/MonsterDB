using System;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class DestructibleRef : Reference
{
    public DestructibleType? m_destructibleType = DestructibleType.Default;
    [DefaultValue(1f)] public float? m_health = 1f;
    public HitData.DamageModifiers? m_damages;
    [YamlMember(Alias = "m_minDamageThreshold")][DefaultValue(0f)] public float? m_minDamageTreshold;
    [DefaultValue(0)] public int? m_minToolTier;
    [DefaultValue(0f)] public float? m_hitNoise;
    [DefaultValue(0f)] public float? m_destroyNoise;
    [DefaultValue(false)] public bool? m_triggerPrivateArea;
    [DefaultValue(0f)] public float? m_ttl;
    public string? m_spawnWhenDestroyed;
    [YamlMember(Description = "Effects")] 
    public EffectListRef? m_destroyedEffect;
    public EffectListRef? m_hitEffect;
    [DefaultValue(false)] public bool? m_autoCreateFragments;

    public DestructibleRef(){}
    public DestructibleRef(Destructible component) => Setup(component);
}