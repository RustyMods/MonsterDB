using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class LevelSetupRef : Reference
{
    [DefaultValue(1f)] public float m_scale = 1f;
    public float m_hue;
    public float m_saturation;
    public float m_value;
    public bool m_setEmissiveColor;
    [DefaultValue("#FFFFFFFF")] public string m_emissiveColor = "#FFFFFFFF";
    public string m_enableObject = "";

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
}

public static partial class Extensions
{
    public static List<LevelSetupRef> ToRef(this List<LevelEffects.LevelSetup> ls)
    {
        List<LevelSetupRef> levelSetups = ls
            .Select(x => new LevelSetupRef()
            {
                m_scale = x.m_scale,
                m_value = x.m_value,
                m_enableObject = x.m_enableObject?.name ?? "",
                m_hue = x.m_hue,
                m_saturation = x.m_saturation,
                m_setEmissiveColor = x.m_setEmissiveColor,
                m_emissiveColor = x.m_emissiveColor.ToRGBAString()
            })
            .ToList();
        return levelSetups;
    }
}