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
}

[Serializable]
public class GradientColorKeyRef : Reference
{
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.Preserve)] public string m_color = "";
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.Preserve)] public float m_time;
}