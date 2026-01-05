using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
    public float? m_attackForce;
    public float? m_backstabBonus;
    public string? m_statusEffect;
    public float? m_healthReturn;
    public float? m_ttl;
    public float? m_hitNoise;
    public EffectListRef? m_hitEffects;
    public EffectListRef? m_hitWaterEffects;
    public bool? m_spawnOnTtl;
    public string? m_spawnOnHit;
    public float? m_spawnOnHitChance;
    public int? m_spawnCount;
    public List<string>? m_randomSpawnOnHit;
    public int? m_randomSpawnOnHitCount;
    public bool? m_randomSpawnSkipLava;
    public bool? m_staticHitOnly;
    public bool? m_groundHitOnly;
    public Vector3Ref? m_spawnOffset;
    public bool? m_spawnRandomRotation;
    public bool? m_spawnFacingRotation;
    public EffectListRef? m_spawnOnHitEffects;
    public bool? m_spawnProjectileNewVelocity;
    public float? m_spawnProjectileMinVel;
    public float? m_spawnProjectileMaxVel;
    public float? m_spawnProjectileRandomDir;
    public bool? m_spawnProjectileHemisphereDir;
    public bool? m_projectilesInheritHitData;
    public bool? m_onlySpawnedProjectilesDealDamage;
    public bool? m_divideDamageBetweenProjectiles;

    public static implicit operator ProjectileRef(Projectile pj)
    {
        ProjectileRef projectileRef = new ProjectileRef();
        projectileRef.ReferenceFrom(pj);
        projectileRef.m_prefab = pj.name;
        return projectileRef;
    }
}