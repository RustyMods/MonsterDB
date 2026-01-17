using System;

namespace MonsterDB;

[Serializable]
public class MovementDamageRef : Reference
{
    public string? m_runDamageObject;
    public AoeRef? m_areaOfEffect;

    public static implicit operator MovementDamageRef(MovementDamage md)
    {
        MovementDamageRef reference = new MovementDamageRef();
        if (md.m_runDamageObject != null)
        {
            reference.m_runDamageObject = md.m_runDamageObject.name;
            Aoe? aoe = md.m_runDamageObject.GetComponent<Aoe>();
            if (aoe != null)
            {
                reference.m_areaOfEffect = new AoeRef();
                reference.m_areaOfEffect.m_damage = aoe.m_damage;
            }
        }

        return reference;
    }
}