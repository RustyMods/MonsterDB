using System;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class ParticleSystemRef : Reference
{
    public string m_prefab = "";
    public string? m_parent;
    public int? m_index;
    public MinMaxGradientRef? m_startColor;
    public CustomDataModuleRef? m_customData;
    public MinMaxGradientRef? m_colorOverLifetime;
    
    public void Update(ParticleSystem ps, string targetName, bool log)
    {
        bool changed = false;
        if (m_startColor != null)
        {
            ParticleSystem.MainModule module = ps.main;
            ParticleSystem.MinMaxGradient start = ps.main.startColor;

            m_startColor.Update(start);

            module.startColor = start;
            changed = true;
        }

        if (m_customData != null)
        {
            ParticleSystem.CustomDataModule custom = ps.customData;
            m_customData.Update(custom);
            changed = true;
        }

        if (m_colorOverLifetime != null)
        {
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            m_colorOverLifetime.Update(colorOverLifetime.color);
            changed = true;
        }

        if (changed && log && !string.IsNullOrEmpty(targetName))
        {
            if (LoadManager.resetting)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{ps.name}] Particle System Reverted");
            }
            else
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{ps.name}] Particle System Updated");
            }
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
            m_customData = new CustomDataModuleRef();
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
                m_color = x.color.ToRGBAString()
            })
            .ToArray();
    }

    public static GradientColorKey[] FromRef(this GradientColorKeyRef[] keys)
    {
        return keys
            .Select(x => new GradientColorKey()
            {
                time = x.m_time,
                color = x.m_color.FromHexOrRGBA(Color.white)
            })
            .ToArray();
    }
}

