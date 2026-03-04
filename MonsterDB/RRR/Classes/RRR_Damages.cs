using System;

namespace MonsterDB;

[Serializable]
public class RRR_Damages
{
    public float? fDamage ;
    public float? fBlunt ;
    public float? fSlash ;
    public float? fPierce ;
    public float? fChop ;
    public float? fPickaxe ;
    public float? fFire ;
    public float? fFrost ;
    public float? fLightning ;
    public float? fPoison ;
    public float? fSpirit ;

    public HitData.DamageTypes ToDamages()
    {
        return new HitData.DamageTypes
        {
            m_damage = fDamage ?? 0f,
            m_blunt =  fBlunt ?? 0f,
            m_slash =  fSlash ?? 0f,
            m_pierce =  fPierce ?? 0f,
            m_chop =  fChop ?? 0f,
            m_pickaxe =  fPickaxe ?? 0f,
            m_fire =  fFire ?? 0f,
            m_frost =  fFrost ?? 0f,
            m_lightning =  fLightning ?? 0f,
            m_poison =  fPoison ?? 0f,
            m_spirit = fSpirit ?? 0f,
        };
    }
}