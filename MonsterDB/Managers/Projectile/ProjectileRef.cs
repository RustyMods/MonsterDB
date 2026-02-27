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
     public float? m_aoe;
     public bool? m_dodgeable;
     public bool? m_blockable;
     public float? m_adrenaline;
     public float? m_attackForce;
     public float? m_backstabBonus;
    public string? m_statusEffect;
     public float? m_healthReturn;
     public float? m_ttl;
     public float? m_hitNoise;
    public EffectListRef? m_hitEffects;
    public EffectListRef? m_hitWaterEffects;
    [YamlMember(Description = "Bounce")]
     public bool? m_bounce;
    [Condition(bounce = true)] public bool? m_bounceOnWater;
    [YamlMember(Description = "0.0 - 1.0")]
    
    [Condition(bounce = true)] 
    public float? m_bouncePower;
    [Condition(bounce = true)] public float? m_bounceRoughness;
    [Condition(bounce = true)] public int? m_maxBounces;
    [Condition(bounce = true)] public float? m_minBounceVel;
    [YamlMember(Description = "Spawn on Hit")]
     public bool? m_respawnItemOnHit;
     public bool? m_spawnOnTtl;
    public string? m_spawnOnHit;
    [YamlMember(Description = "0.0 - 1.0")][Condition(spawnOnHit = true)] 
    public float? m_spawnOnHitChance;
    [Condition(spawnOnHit = true)] public int? m_spawnCount;
    public List<string>? m_randomSpawnOnHit;
    [Condition(randomSpawnOnHit = true)] public int? m_randomSpawnOnHitCount;
    [Condition(randomSpawnOnHit = true)] public bool? m_randomSpawnSkipLava;
     public bool? m_staticHitOnly;
     public bool? m_groundHitOnly;
    public Vector3Ref? m_spawnOffset;
     public bool? m_spawnRandomRotation;
     public bool? m_spawnFacingRotation;
    public EffectListRef? m_spawnOnHitEffects;
    [YamlMember(Description = "Projectile Spawning")]
     public bool? m_spawnProjectileNewVelocity;
     public float? m_spawnProjectileMinVel;
     public float? m_spawnProjectileMaxVel;
     public float? m_spawnProjectileRandomDir;
     public bool? m_spawnProjectileHemisphereDir;
     public bool? m_projectilesInheritHitData;
     public bool? m_onlySpawnedProjectilesDealDamage;
     public bool? m_divideDamageBetweenProjectiles;
    
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