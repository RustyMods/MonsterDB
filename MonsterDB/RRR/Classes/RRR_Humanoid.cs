using System;
using System.Collections.Generic;
using System.Linq;

namespace MonsterDB;

[Serializable]
public class RRR_Humanoid
{
    public RRR_Damages? dtAttackDamageOverride ;
    public float? fAttackDamageTotalOverride ;
    public string? sAttackProjectileOverride ;
    public List<string>? aDefaultItems ;
    public List<string>? aRandomArmor ;
    public List<string>? aRandomShield ;
    public List<RRR_RandomSet>? aaRandomSets ;
    public List<RRR_CustomAttacks>? aAdvancedCustomAttacks ;
    public List<string>? aHitEffects ;
    public List<string>? aDeathEffects ;
    public List<string>? aConsumeItemEffects ;

    public void Setup(HumanoidRef reference)
    {
        reference.m_defaultItems = aDefaultItems?.ToArray();
        reference.m_randomArmor = aRandomArmor?.ToArray();
        reference.m_randomShield = aRandomShield?.ToArray();
        reference.m_randomSets = aaRandomSets?.ToList().Select(s => s.ToItemSet()).ToArray();
        if (aHitEffects != null) reference.m_hitEffects = new EffectListRef(aHitEffects.ToArray());
        if (aDeathEffects != null) reference.m_deathEffects = new EffectListRef(aDeathEffects.ToArray());
        if (aConsumeItemEffects != null) reference.m_consumeItemEffects = new EffectListRef(aConsumeItemEffects.ToArray());
    }
}