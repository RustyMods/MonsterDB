using System;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class DropProjectileOverDistanceRef : Reference
{
    public string? m_projectilePrefab;
    public float? m_distancePerProjectile = 5f;
    public float? m_spawnHeight = 1f;
    public bool? m_snapToGround;
    [YamlMember(Description = "If higher than 0, will force a spawn if nothing has spawned in that amount of time.")]
    public float? m_timeToForceSpawn = -1f;
    public float? m_minVelocity;
    public float? m_maxVelocity;
    public ProjectileRef? m_projectile;
    public AoeRef? m_projectileAoe;

    public static implicit operator DropProjectileOverDistanceRef(DropProjectileOverDistance dpod)
    {
        DropProjectileOverDistanceRef reference = new DropProjectileOverDistanceRef();
        reference.Setup(dpod);
        if (dpod.m_projectilePrefab != null)
        {
            if (dpod.m_projectilePrefab.TryGetComponent(out Projectile projectile))
            {
                reference.m_projectile = projectile;
            }

            if (dpod.m_projectilePrefab.TryGetComponent(out Aoe aoe))
            {
                reference.m_projectileAoe = aoe.ToRef();
            }
        }
        return reference;
    }
}