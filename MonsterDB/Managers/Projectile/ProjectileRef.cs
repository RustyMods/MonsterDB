using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class ProjectileRef : Reference
{
    public string m_prefab = "";
    public ProjectileType? m_type;
    public HitData.DamageTypes? m_damage;
    [DefaultValue(0f)] public float? m_aoe;
    [DefaultValue(false)] public bool? m_dodgeable;
    [DefaultValue(false)] public bool? m_blockable;
    [DefaultValue(false)] public float? m_adrenaline;
    [DefaultValue(0f)] public float? m_attackForce;
    [DefaultValue(4f)] public float? m_backstabBonus;
    public string? m_statusEffect;
    [DefaultValue(0f)] public float? m_healthReturn;
    [DefaultValue(4f)] public float? m_ttl;
    [DefaultValue(50f)] public float? m_hitNoise;
    public EffectListRef? m_hitEffects;
    public EffectListRef? m_hitWaterEffects;
    [YamlMember(Description = "Bounce")]
    [DefaultValue(false)] public bool? m_bounce;
    [DefaultValue(false)][Condition(bounce = true)] public bool? m_bounceOnWater;
    [YamlMember(Description = "0.0 - 1.0")]
    [DefaultValue(0.85f)]
    [Condition(bounce = true)] 
    public float? m_bouncePower;
    [DefaultValue(0.3f)][Condition(bounce = true)] public float? m_bounceRoughness;
    [DefaultValue(99f)][Condition(bounce = true)] public int? m_maxBounces;
    [DefaultValue(0.25f)][Condition(bounce = true)] public float? m_minBounceVel;
    [YamlMember(Description = "Spawn on Hit")]
    [DefaultValue(false)] public bool? m_respawnItemOnHit;
    [DefaultValue(false)] public bool? m_spawnOnTtl;
    public string? m_spawnOnHit;
    [YamlMember(Description = "0.0 - 1.0")][DefaultValue(1f)][Condition(spawnOnHit = true)] 
    public float? m_spawnOnHitChance;
    [DefaultValue(1)][Condition(spawnOnHit = true)] public int? m_spawnCount;
    public List<string>? m_randomSpawnOnHit;
    [DefaultValue(1)][Condition(randomSpawnOnHit = true)] public int? m_randomSpawnOnHitCount;
    [DefaultValue(false)][Condition(randomSpawnOnHit = true)] public bool? m_randomSpawnSkipLava;
    [DefaultValue(false)] public bool? m_staticHitOnly;
    [DefaultValue(false)] public bool? m_groundHitOnly;
    public Vector3Ref? m_spawnOffset;
    [DefaultValue(false)] public bool? m_spawnRandomRotation;
    [DefaultValue(false)] public bool? m_spawnFacingRotation;
    public EffectListRef? m_spawnOnHitEffects;
    [YamlMember(Description = "Projectile Spawning")]
    [DefaultValue(false)] public bool? m_spawnProjectileNewVelocity;
    [DefaultValue(1f)] public float? m_spawnProjectileMinVel;
    [DefaultValue(5f)] public float? m_spawnProjectileMaxVel;
    [DefaultValue(0f)] public float? m_spawnProjectileRandomDir;
    [DefaultValue(false)] public bool? m_spawnProjectileHemisphereDir;
    [DefaultValue(false)] public bool? m_projectilesInheritHitData;
    [DefaultValue(false)] public bool? m_onlySpawnedProjectilesDealDamage;
    [DefaultValue(false)] public bool? m_divideDamageBetweenProjectiles;
    
    public ProjectileRef(){}

    public ProjectileRef(Projectile projectile)
    {
        Setup(projectile);
        m_prefab = projectile.name;
    }

    public override bool ShouldSetupField<V>(FieldInfo targetField, V source)
    {
        if (source is Projectile projectile)
        {
            Condition? conditions = targetField.GetCustomAttribute<Condition>();
            if (conditions == null) return true;
            return conditions.ShouldSetupField(projectile);
        }
        return true;
    }
}