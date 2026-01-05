using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MonsterDB;

[Serializable]
public class LevelSetupRef : Reference
{
    public float? m_scale;
    public float? m_hue;
    public float? m_saturation;
    public float? m_value;
    public bool? m_setEmissiveColor;
    [DefaultValue("#FFFFFFFF")] public string? m_emissiveColor;
    public string? m_enableObject;
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
                m_emissiveColor = x.m_emissiveColor.ToHex()
            })
            .ToList();
        return levelSetups;
    }
}