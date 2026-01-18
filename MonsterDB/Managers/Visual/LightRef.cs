using System;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class LightRef : Reference
{
    public string m_prefab = "";
    public string? m_parent;
    public int? m_index;
    public bool? m_active;
    public string? color;
    public LightType? type;
    public float? intensity;
    public float? range;

    public void Update(Light light, string targetName, bool log)
    {
        if (log && !string.IsNullOrEmpty(targetName))
        {
            if (LoadManager.resetting)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{light.name}] Resetting light");
            }
            else
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{light.name}] Updating light");
            }
        }
        
        log &= ConfigManager.ShouldLogDetails() && !string.IsNullOrEmpty(targetName);
        
        if (m_active.HasValue)
        {
            light.gameObject.SetActive(m_active.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{light.name}] m_active: {m_active.Value}");
            }
        }
        
        if (color != null)
        {
            light.color = color.FromHex(light.color);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{light.name}] m_color: {color}");
            }
        }

        if (type.HasValue)
        {
            light.type = type.Value;
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{light.name}] m_type: {type.Value}");
            }
        }

        if (intensity.HasValue)
        {
            light.intensity = intensity.Value;
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{light.name}] m_intensity: {intensity.Value}");
            }
        }

        if (range.HasValue)
        {
            light.range = range.Value;
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}]/[{light.name}] m_range: {range.Value}");
            }
        }
    }
}

public static partial class Extensions
{
    public static LightRef[] ToRef(this Light[] lights)
    {
        return lights
            .Select(x => new LightRef()
            {
                m_prefab = x.name,
                m_parent = x.transform.parent?.name,
                m_index = x.transform.GetSiblingIndex(),
                m_active = x.gameObject.activeSelf,
                color = x.color.ToHex(),
                type = x.type,
                intensity = x.intensity,
                range = x.range
            })
            .ToArray();
    }
}