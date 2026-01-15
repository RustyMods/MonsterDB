using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class MaterialRef : Reference
{
    public string m_name = "";
    public string m_shader = "";
    public string? m_color = "";
    public float? m_hue;
    public float? m_saturation;
    public float? m_value;
    public string? m_emissionTexture = "";
    public string? m_emissionColor = "";
    public string? m_mainTexture = "";
    public string? m_tintColor = "";

    public static implicit operator MaterialRef(Material mat)
    {
        MaterialRef reference = new MaterialRef
        {
            m_name = mat.name,
            m_shader = mat.shader.name,
            m_color = mat.HasProperty(ShaderRef._Color) ? mat.color.ToHex() : null,
            m_hue = mat.HasProperty(ShaderRef._Hue) ? mat.GetFloat(ShaderRef._Hue) : null,
            m_saturation = mat.HasProperty(ShaderRef._Saturation) ? mat.GetFloat(ShaderRef._Saturation) : null,
            m_value =  mat.HasProperty(ShaderRef._Value) ? mat.GetFloat(ShaderRef._Value) : null,
            m_emissionColor = mat.HasProperty(ShaderRef._EmissionColor) ? mat.GetColor(ShaderRef._EmissionColor).ToHex() : null,
            m_mainTexture = mat.mainTexture?.name ?? null,
            m_tintColor = mat.HasProperty(ShaderRef._TintColor) ? mat.GetColor(ShaderRef._TintColor).ToHex() : null,
            m_emissionTexture = mat.HasProperty(ShaderRef._EmissionMap) ? mat.GetTexture(ShaderRef._EmissionMap)?.name : null
        };
        
        return reference;
    }
}

public static partial class Extensions
{
    public static MaterialRef[] ToRef(this Material[] mats)
    {
        List<MaterialRef> refs = new();
        foreach (Material? mat in mats)
        {
            if (mat == null) continue;
            refs.Add(mat);
        }

        return refs.ToArray();
    }
    
    public static string ToHex(this Color color, bool includeAlpha = true)
    {
        Color32 c = color;

        return includeAlpha
            ? $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}"
            : $"#{c.r:X2}{c.g:X2}{c.b:X2}";
    }
    
    public static Color FromHex(this string hex, Color defaultValue)
    {
        if (string.IsNullOrWhiteSpace(hex)) return defaultValue;

        hex = hex.TrimStart('#');

        if (hex.Length != 6 && hex.Length != 8)
            return defaultValue;

        byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        byte a = hex.Length == 8
            ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber)
            : (byte)255;

        return new Color32(r, g, b, a);
    }
}