using System;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class ParticleSystemRef : Reference
{
    public string m_prefab = "";
    public string? m_parent;
    public int? m_index;
    public MinMaxGradientRef? m_startColor;
    public ParticleCustomDataRef? m_customData;
    public MinMaxGradientRef? m_colorOverLifetime;
    
    public void Update(ParticleSystem ps)
    {
        if (m_startColor != null)
        {
            ParticleSystem.MainModule module = ps.main;
            ParticleSystem.MinMaxGradient start = ps.main.startColor;

            m_startColor.Update(start);

            module.startColor = start;
            MonsterDBPlugin.LogDebug($"Changing particle system Start Color: {ps.name}");

        }

        if (m_customData != null)
        {
            ParticleSystem.CustomDataModule custom = ps.customData;
            m_customData.Update(custom);
            MonsterDBPlugin.LogDebug($"Changing particle system Custom Data: {ps.name}");
        }

        if (m_colorOverLifetime != null)
        {
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            m_colorOverLifetime.Update(colorOverLifetime.color);
        }
    }

    public void Set(ParticleSystem ps)
    {
        m_prefab = ps.gameObject.name;
        m_parent = ps.transform.parent?.name;
        m_index = ps.transform.GetSiblingIndex();
        m_startColor = new MinMaxGradientRef();
        m_startColor.Set(ps.main.startColor);
        
        ParticleSystem.CustomDataModule custom = ps.customData;
        if (custom.enabled)
        {
            m_customData = new ParticleCustomDataRef();
            m_customData.Set(custom);
        }
        
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
        if (colorOverLifetime.enabled)
        {
            ParticleSystem.MinMaxGradient color = colorOverLifetime.color;
            m_colorOverLifetime = new MinMaxGradientRef();
            m_colorOverLifetime.Set(color);
        }
    }
}

[Serializable]
public class ParticleCustomDataRef : Reference
{
    public MinMaxGradientRef? m_custom1;
    public MinMaxGradientRef? m_custom2;

    public void Set(ParticleSystem.CustomDataModule cm)
    {
        m_custom1 = new MinMaxGradientRef();
        m_custom1.Set(cm.GetColor(ParticleSystemCustomData.Custom1));
        m_custom2 = new MinMaxGradientRef();
        m_custom2.Set(cm.GetColor(ParticleSystemCustomData.Custom2));
    }

    public void Update(ParticleSystem.CustomDataModule cm)
    {
        var custom1 = cm.GetColor(ParticleSystemCustomData.Custom1);
        var custom2 = cm.GetColor(ParticleSystemCustomData.Custom2);

        if (m_custom1 != null)
        {
            m_custom1.Update(custom1);
        }

        if (m_custom2 != null)
        {
            m_custom2.Update(custom2);
        }
        
        cm.SetColor(ParticleSystemCustomData.Custom1, custom1);
        cm.SetColor(ParticleSystemCustomData.Custom2, custom2);
    }
}

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

    public void Set(ParticleSystem.MinMaxGradient gradient)
    {
        m_mode = gradient.mode;
        m_color = gradient.color.ToHex();
        m_colorMin = gradient.colorMin.ToHex();
        m_colorMax = gradient.colorMax.ToHex();
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
            gradient.color = m_color.FromHex(gradient.color);
        }

        if (m_colorMin != null && !string.IsNullOrEmpty(m_colorMin))
        {
            gradient.colorMin = m_colorMin.FromHex(gradient.colorMin);
        }

        if (m_colorMax != null && !string.IsNullOrEmpty(m_colorMax))
        {
            gradient.colorMax = m_colorMax.FromHex(gradient.colorMax);
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
    public float m_alpha;
    public float m_time;
}

[Serializable]
public class GradientColorKeyRef : Reference
{
    public string m_color = "";
    public float m_time;
}

public static partial class Extensions
{
    public static ParticleSystemRef[] ToRef(this ParticleSystem[] particleSystems)
    {
        return particleSystems
            .Select(x => x.ToRef())
            .ToArray();
    }
    public static ParticleSystemRef ToRef(this ParticleSystem ps)
    {
        var reference = new ParticleSystemRef();
        reference.Set(ps);
        return reference;
    }
    
    public static GradientAlphaKeyRef[] ToRef(this GradientAlphaKey[] keys)
    {
        return keys
            .Select(x => new GradientAlphaKeyRef()
            {
                m_time = x.time,
                m_alpha = x.alpha
            })
            .ToArray();
    }

    public static GradientAlphaKey[] FromRef(this GradientAlphaKeyRef[] keys)
    {
        return keys
            .Select(x => new GradientAlphaKey()
            {
                time = x.m_time,
                alpha = x.m_alpha
            })
            .ToArray();
    }

    public static GradientColorKeyRef[] ToRef(this GradientColorKey[] keys)
    {
        return keys
            .Select(x => new GradientColorKeyRef()
            {
                m_time = x.time,
                m_color = x.color.ToHex()
            })
            .ToArray();
    }

    public static GradientColorKey[] FromRef(this GradientColorKeyRef[] keys)
    {
        return keys
            .Select(x => new GradientColorKey()
            {
                time = x.m_time,
                color = x.m_color.FromHex(Color.white)
            })
            .ToArray();
    }
}

