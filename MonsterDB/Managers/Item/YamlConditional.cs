using System;

namespace MonsterDB;

public class YamlConditional : Attribute
{
    public bool icon;
    public bool melee;
    public bool bow;
    public bool reload;

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
}