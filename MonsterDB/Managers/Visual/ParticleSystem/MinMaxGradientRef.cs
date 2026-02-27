using System;
using System.ComponentModel;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class MinMaxGradientRef : Reference
{
    [YamlMember(Description = "Color, Gradient, TwoColors, TwoGradients, RandomColor")] 
    public ParticleSystemGradientMode? m_mode;
    public string? m_color;
    public string? m_colorMin;
    public string? m_colorMax;
    public GradientRef? m_gradient;
    public GradientRef? m_gradientMin;
    public GradientRef? m_gradientMax;
    
    public MinMaxGradientRef(){}

    public MinMaxGradientRef(ParticleSystem.MinMaxGradient gradient)
    {
        m_mode = gradient.mode;
        m_color = gradient.color.ToRGBAString();
        m_colorMin = gradient.colorMin.ToRGBAString();
        m_colorMax = gradient.colorMax.ToRGBAString();
        if (gradient.gradient != null) m_gradient = gradient.gradient;
        if (gradient.gradientMin != null) m_gradientMin = gradient.gradientMin;
        if (gradient.gradientMax != null) m_gradientMax = gradient.gradientMax;
    }

    public void Update(ParticleSystem.MinMaxGradient gradient)
    {
        if (m_mode != null)
        {
            m_mode = gradient.mode;
        }
        if (m_color != null && !string.IsNullOrEmpty(m_color))
        {
            gradient.color = m_color.FromHexOrRGBA(gradient.color);
        }

        if (m_colorMin != null && !string.IsNullOrEmpty(m_colorMin))
        {
            gradient.colorMin = m_colorMin.FromHexOrRGBA(gradient.colorMin);
        }

        if (m_colorMax != null && !string.IsNullOrEmpty(m_colorMax))
        {
            gradient.colorMax = m_colorMax.FromHexOrRGBA(gradient.colorMax);
        }

        if (m_gradient != null)
        {
            m_gradient.Update(gradient.gradient);
        }

        if (m_gradientMin != null)
        {
            m_gradientMin.Update(gradient.gradientMin);
        }

        if (m_gradientMax != null)
        {
            m_gradientMax.Update(gradient.gradientMax);
        }
    }
}