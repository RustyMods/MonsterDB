using System;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class TriggerSpawnAbilityRef : Reference
{
    [YamlMember(Description = "Triggers nearby spawners within range")]
    public float? m_range;
    
    public TriggerSpawnAbilityRef(){}

    public TriggerSpawnAbilityRef(TriggerSpawnAbility ability)
    {
        m_range = ability.m_range;
    }
}