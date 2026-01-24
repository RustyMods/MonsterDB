using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class SaddleRef : Reference
{
    public string? m_hoverText;
    public float? m_maxUseRange;
    public Vector3Ref? m_detachOffset;
    public string? m_attachAnimation;
    public float? m_maxStamina;
    public float? m_runStaminaDrain;
    public float? m_swimStaminaDrain;
    public float? m_staminaRegen;
    public float? m_staminaRegenHungry;
    public EffectListRef? m_drownEffects;
    public string? m_mountIcon;
    [YamlMember(Description = "If added saddle by MonsterDB, use this to position attach point")] 
    public Vector3Ref? m_attachOffset;
    public string? m_attachParent;
    
    public SaddleRef(){}
    public SaddleRef(Sadle component) => Setup(component);
    
    protected override void UpdateGameObject<T>(T target, FieldInfo targetField, string targetName, string goName,
        bool log)
    {
        if (target is not Saddle component) return;
        
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