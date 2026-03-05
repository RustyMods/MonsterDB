using System;

namespace MonsterDB;

[Serializable]
public class RRR_Character
{
    public string? sName ;
    public string? sFaction ;
    public bool? bBoss ;
    public string? sBossEvent ;
    public string? sDefeatSetGlobalKey ;
    public float? fMoveSpeedMulti ;
    public bool? bTolerateWater ;
    public bool? bTolerateFire ;
    public bool? bTolerateSmoke ;
    public bool? bStaggerWhenBlocked ;
    public float? fHealth ;
    public RRR_DamageModifiers? DamageTaken ;

    public void Setup(CharacterRef reference)
    {
        reference.m_name = sName;
        if (sFaction != null) reference.m_faction = FactionManager.GetFaction(sFaction);
        reference.m_boss = bBoss;
        reference.m_bossEvent = sBossEvent;
        reference.m_defeatSetGlobalKey = sDefeatSetGlobalKey;
        reference.m_tolerateWater = bTolerateWater;
        reference.m_tolerateFire = bTolerateFire;
        reference.m_tolerateSmoke = bTolerateSmoke;
        reference.m_staggerWhenBlocked = bStaggerWhenBlocked;
        reference.m_health = fHealth;
        if (DamageTaken != null && reference.m_damageModifiers != null)
        {
            HitData.DamageModifiers modifiers = reference.m_damageModifiers.Value;
            modifiers.m_blunt = GetDamageModifier(DamageTaken.m_blunt, modifiers.m_blunt);
            modifiers.m_pierce = GetDamageModifier(DamageTaken.m_pierce, modifiers.m_pierce);
            modifiers.m_slash = GetDamageModifier(DamageTaken.m_slash, modifiers.m_slash);
            modifiers.m_pickaxe = GetDamageModifier(DamageTaken.m_pickaxe, modifiers.m_pickaxe);
            modifiers.m_chop = GetDamageModifier(DamageTaken.m_chop, modifiers.m_chop);
            modifiers.m_fire = GetDamageModifier(DamageTaken.m_fire, modifiers.m_fire);
            modifiers.m_frost = GetDamageModifier(DamageTaken.m_frost, modifiers.m_frost);
            modifiers.m_lightning = GetDamageModifier(DamageTaken.m_lightning, modifiers.m_lightning);
            modifiers.m_poison = GetDamageModifier(DamageTaken.m_poison, modifiers.m_poison);
            modifiers.m_spirit = GetDamageModifier(DamageTaken.m_spirit, modifiers.m_spirit);
            reference.m_damageModifiers = modifiers;
        }
    }

    public HitData.DamageModifier GetDamageModifier(string? input, HitData.DamageModifier defaultValue) => input != null ?
        Enum.TryParse(input, true, out HitData.DamageModifier mod) ? mod : defaultValue : defaultValue;
}