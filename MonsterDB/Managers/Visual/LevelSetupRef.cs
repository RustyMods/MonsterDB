using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class LevelSetupRef : Reference
{
    public float m_scale = 1f;
    public float m_hue;
    public float m_saturation;
    public float m_value;
    public bool m_setEmissiveColor;
    public string m_emissiveColor = "#FFFFFFFF";
    public string m_enableObject = "";
    
    public LevelSetupRef(){}

    public LevelSetupRef(LevelEffects.LevelSetup setup)
    {
        m_scale = setup.m_scale;
        m_value = setup.m_value;
        m_enableObject = setup.m_enableObject?.name ?? "";
        m_hue = setup.m_hue;
        m_saturation = setup.m_saturation;
        m_setEmissiveColor = setup.m_setEmissiveColor;
        m_emissiveColor = setup.m_emissiveColor.ToRGBAString();
    }

    public void Set(LevelEffects.LevelSetup setup, Dictionary<string, Renderer> renderers)
    {
        setup.m_scale = m_scale;
        setup.m_hue = m_hue;
        setup.m_saturation = m_saturation;
        setup.m_value = m_value;
        setup.m_setEmissiveColor = m_setEmissiveColor;
        setup.m_emissiveColor = m_emissiveColor.FromHexOrRGBA(Color.white);
        if (!string.IsNullOrEmpty(m_enableObject) && 
            renderers.TryGetValue(m_enableObject, out Renderer? renderer))
        {
            setup.m_enableObject = renderer.gameObject;
        }
    }

    public override bool Equals<T>(T other)
    {
        if (other is not LevelSetupRef otherRef) return false;
        return m_scale.Equals(otherRef.m_scale) && 
               m_hue.Equals(otherRef.m_hue) &&
               m_saturation.Equals(otherRef.m_saturation) &&
               m_value.Equals(otherRef.m_value) &&
               m_setEmissiveColor.Equals(otherRef.m_setEmissiveColor) &&
               m_emissiveColor.Equals(otherRef.m_emissiveColor) &&
               m_enableObject.Equals(otherRef.m_enableObject);
    }
}

public static partial class Extensions
{
    public static List<LevelSetupRef> ToLevelSetupRefList(this List<LevelEffects.LevelSetup> ls)
    {
        List<LevelSetupRef> levelSetups = ls
            .Select(x => new LevelSetupRef(x))
            .ToList();
        return levelSetups;
    }
}