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

    public DropProjectileOverDistanceRef(){}

    public DropProjectileOverDistanceRef(DropProjectileOverDistance component)
    {
        Setup(component);
        if (component.m_projectilePrefab != null)
        {
            if (component.m_projectilePrefab.TryGetComponent(out Projectile projectile))
            {
                m_projectile = new ProjectileRef(projectile);
            }

            if (component.m_projectilePrefab.TryGetComponent(out Aoe aoe))
            {
                m_projectileAoe = new AoeRef(aoe);
            }
        }
    }
}