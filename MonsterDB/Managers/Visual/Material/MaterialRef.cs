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

    public MaterialRef(){}

    public MaterialRef(Material mat)
    {
        m_name = mat.name;
        m_shader = mat.shader.name;
        m_color = mat.HasProperty(ShaderRef._Color) ? mat.color.ToRGBAString() : null;
        m_hue = mat.HasProperty(ShaderRef._Hue) ? mat.GetFloat(ShaderRef._Hue) : null;
        m_saturation = mat.HasProperty(ShaderRef._Saturation) ? mat.GetFloat(ShaderRef._Saturation) : null;
        m_value =  mat.HasProperty(ShaderRef._Value) ? mat.GetFloat(ShaderRef._Value) : null;
        m_emissionColor = mat.HasProperty(ShaderRef._EmissionColor) ? mat.GetColor(ShaderRef._EmissionColor).ToRGBAString() : null;
        m_mainTexture = mat.mainTexture?.name ?? null;
        m_tintColor = mat.HasProperty(ShaderRef._TintColor) ? mat.GetColor(ShaderRef._TintColor).ToRGBAString() : null;
        m_emissionTexture = mat.HasProperty(ShaderRef._EmissionMap) ? mat.GetTexture(ShaderRef._EmissionMap)?.name : null;
    }
    public void Update(Material material, string targetName, bool log)
    {
        if (string.IsNullOrEmpty(targetName)) targetName = "Unknown";
        log &= ConfigManager.ShouldLogDetails();
        
        if (material.shader != null && material.shader.name != m_shader && !string.IsNullOrEmpty(m_shader))
        {
            material.shader = ShaderRef.GetShader(m_shader, material.shader);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_shader: {material.shader.name}");
            }
        }
        if (material.HasProperty(ShaderRef._Color) && m_color != null)
        {
            if (!string.IsNullOrEmpty(m_color))
            {
                material.color = m_color.FromHexOrRGBA(material.color);
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_color: {m_color}");
                }
            }
        }
        if (m_hue != null && material.HasProperty(ShaderRef._Hue))
        {
            material.SetFloat(ShaderRef._Hue, m_hue.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_hue: {m_hue.Value}");
            }
        }
        if (m_saturation != null && material.HasProperty(ShaderRef._Saturation))
        {
            material.SetFloat(ShaderRef._Saturation, m_saturation.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_value: {m_saturation.Value}");
            }
        }
        if (m_value != null && material.HasProperty(ShaderRef._Value))
        {
            material.SetFloat(ShaderRef._Value, m_value.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_value: {m_value.Value}");
            }
        }
        if (m_emissionColor != null && material.HasProperty(ShaderRef._EmissionColor))
        {
            if (!string.IsNullOrEmpty(m_emissionColor))
            {
                material.SetColor(ShaderRef._EmissionColor, m_emissionColor.FromHexOrRGBA(material.GetColor(ShaderRef._EmissionColor)));
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_emissionColor: {m_emissionColor}");
                }
            }
        }
        if (m_mainTexture != null)
        {
            material.mainTexture = TextureManager.GetTexture(m_mainTexture, material.mainTexture);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] mainTexture: {material.mainTexture?.name ?? "null"}");
            }
        }

        if (material.HasProperty(ShaderRef._TintColor) && m_tintColor != null)
        {
            if (!string.IsNullOrEmpty(m_tintColor))
            {
                material.SetColor(ShaderRef._TintColor, m_tintColor.FromHexOrRGBA(material.GetColor(ShaderRef._TintColor)));
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_tintColor: {m_tintColor}");
                }
            }
        }

        if (m_emissionTexture != null && !string.IsNullOrEmpty(m_emissionTexture) &&
            material.HasProperty(ShaderRef._EmissionMap))
        {
            Texture? tex = TextureManager.GetTexture(m_emissionTexture, material.GetTexture(ShaderRef._EmissionMap));
            material.SetTexture(ShaderRef._EmissionMap, tex);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_emissionTexture: {tex?.name ?? "null"}");
            }
        }
    }
}

public static partial class Extensions
{
    public static MaterialRef[] ToMaterialRefArray(this Material[] mats)
    {
        List<MaterialRef> refs = new();
        foreach (Material? mat in mats)
        {
            if (mat == null) continue;
            refs.Add(new MaterialRef(mat));
        }

        return refs.ToArray();
    }
    
    public static string ToRGBAString(this Color color)
    {
        return color.ToString();
    }

    public static string ToHexString(this Color color, bool includeAlpha = true)
    {
        Color32 c = color;

        return includeAlpha
            ? $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}"
            : $"#{c.r:X2}{c.g:X2}{c.b:X2}";
    }
    
    public static Color FromHexOrRGBA(this string hex, Color defaultValue)
    {
        if (string.IsNullOrWhiteSpace(hex)) return defaultValue;

        if (hex.StartsWith("#"))
        {
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
        if (hex.StartsWith("RGBA"))
        {
            int start = hex.IndexOf('(') + 1;
            int end = hex.IndexOf(')');
            string[] values = hex.Substring(start, end - start).Split(',');
            if (values.Length != 4) return defaultValue;
            if (!float.TryParse(values[0], out float r)) return defaultValue;
            if (!float.TryParse(values[1], out float g)) return defaultValue;
            if (!float.TryParse(values[2], out float b)) return defaultValue;
            if (!float.TryParse(values[3], out float a)) return defaultValue;
            return new Color(r, g, b, a);
        }

        return defaultValue;
    }
}