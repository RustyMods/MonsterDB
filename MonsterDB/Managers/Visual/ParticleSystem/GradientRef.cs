using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class GradientRef : Reference
{
    [YamlMember(Description = "Blend, Fixed, PerceptualBlend")] public GradientMode? m_mode;
    [YamlMember(Description = "Set opacity over time")] public GradientAlphaKeyRef[]? m_alphaKeys;
    [YamlMember(Description = "Set color over time")] public GradientColorKeyRef[]? m_colorKeys;
    [YamlMember(Description = "Uninitialized, Gamma, Linear")] public ColorSpace? m_colorSpace;

    public void Update(Gradient grad)
    {
        if (m_mode.HasValue)
        {
            grad.mode = m_mode.Value;
        }

        if (m_colorSpace.HasValue)
        {
            grad.colorSpace = m_colorSpace.Value;
        }

        if (m_alphaKeys != null)
        {
            grad.alphaKeys = m_alphaKeys.FromRef();
        }

        if (m_colorKeys != null)
        {
            grad.colorKeys = m_colorKeys.FromRef();
        }
    }

    public static implicit operator GradientRef(Gradient grad)
    {
        return new GradientRef()
        {
            m_mode = grad.mode,
            m_alphaKeys = grad.alphaKeys.ToRef(),
            m_colorKeys = grad.colorKeys.ToRef(),
            m_colorSpace = grad.colorSpace
        };
    }
}

[Serializable]
public class GradientAlphaKeyRef : Reference
{
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.Preserve)] public float m_alpha;
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.Preserve)] public float m_time;
    
    public GradientAlphaKeyRef(){}

    public GradientAlphaKeyRef(GradientAlphaKey key)
    {
        m_time = key.time;
        m_alpha = key.alpha;
    }

    public GradientAlphaKey ToGradientAlphaKey() => new GradientAlphaKey() { time = m_time, alpha = m_alpha };
}

[Serializable]
public class GradientColorKeyRef : Reference
{
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.Preserve)] public string m_color = "";
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.Preserve)] public float m_time;
    
    public GradientColorKeyRef(){}

    public GradientColorKeyRef(GradientColorKey key)
    {
        m_color = key.color.ToRGBAString();
        m_time = key.time;
    }

    public GradientColorKey ToGradientColorKey() => new GradientColorKey()
        { color = m_color.FromHexOrRGBA(Color.white), time = m_time };
}