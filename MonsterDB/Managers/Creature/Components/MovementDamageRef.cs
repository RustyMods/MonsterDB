using System;
using System.Reflection;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class MovementDamageRef : Reference
{
    public string? m_runDamageObject;
    public AoeRef? m_areaOfEffect;
    
    public MovementDamageRef(){}

    public MovementDamageRef(MovementDamage md)
    {
        if (md.m_runDamageObject != null)
        {
            m_runDamageObject = md.m_runDamageObject.name;
            Aoe? aoe = md.m_runDamageObject.GetComponent<Aoe>();
            if (aoe != null)
            {
                m_areaOfEffect = new AoeRef
                {
                    m_damage = aoe.m_damage
                };
            }
        }
    }

    protected override void UpdateGameObject<T>(T target, FieldInfo targetField, string targetName, string goName,
        bool log)
    {
        if (target is not MovementDamage component) return;
        
        if (string.IsNullOrEmpty(goName))
        {
            targetField.SetValue(target, null);
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: null");
        }
        else
        {
            Transform child = Utils.FindChild(component.transform, goName);
            if (child != null)
            {
                targetField.SetValue(target, child.gameObject);
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {child.name}");
            }
        }
    }
}