using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class EggGrowRef : Reference
{
    public float? m_growTime;
    public string? m_grownPrefab;
    public bool? m_tamed;
    public float? m_updateInterval;
    public bool? m_requireNearbyFire;
    public bool? m_requireUnderRoof;
    public float? m_requireCoverPercentige;
    public EffectListRef? m_hatchEffect;
    public string? m_growingObject;
    public string? m_notGrowingObject;
    
    public EggGrowRef(){}
    public EggGrowRef(EggGrow egg) => Setup(egg);
    
    protected override void UpdateGameObject<T>(T target, FieldInfo targetField, string targetName, string goName,
        bool log)
    {
        if (targetField.Name == "m_growingObject" || targetField.Name == "m_notGrowingObject")
        {
            if (target is not EggGrow component) return;
        
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
        else
        {
            base.UpdateGameObject(target, targetField, targetName, goName, log);
        }
    }
}