using System;

namespace MonsterDB;

public class Condition : Attribute
{
    public bool icon;
    public bool melee;
    public bool bow;
    public bool reload;
    public bool bounce;
    public bool spawnOnHit;
    public bool randomSpawnOnHit;
    public bool noInstance;

    public bool ShouldSetupField(ItemDrop.ItemData.SharedData sharedData)
    {
        if (icon)
        {
            return sharedData.m_icons != null && sharedData.m_icons.Length != 0;
        }

        if (melee)
        {
            return sharedData.m_attack.m_attackRayWidth > 0;
        }

        if (bow)
        {
            return sharedData.m_attack.m_drawDurationMin > 0 || sharedData.m_attack.m_attackProjectile != null;
        }

        return true;
    }

    public bool ShouldSetupField(Projectile projectile)
    {
        if (bounce)
        {
            return projectile.m_bounce;
        }

        if (spawnOnHit)
        {
            return projectile.m_spawnOnHit != null;
        }

        if (randomSpawnOnHit)
        {
            return projectile.m_randomSpawnOnHit is { Count: > 0 };
        }

        return true;
    }
}