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
    public string? m_materialOverride;
    public float? m_glossiness;
    public float? m_metallic;
    public float? m_bumpScale;
    public string? m_bumpTexture;
    public float? m_flowSpeedAll;
    public string? m_flowMaskTexture;
    public float? m_flowSpeedY;
    public string? m_flowTexture;
    public string? m_flowColor;
    public string? m_edgeColor;
    public string? m_emissiveColor;

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
        m_glossiness = mat.HasProperty(ShaderRef._Glossiness) ? mat.GetFloat(ShaderRef._Glossiness) : null;
        m_metallic = mat.HasProperty(ShaderRef._Metallic) ? mat.GetFloat(ShaderRef._Metallic) : null;
        m_bumpScale = mat.HasProperty(ShaderRef._BumpScale) ? mat.GetFloat(ShaderRef._BumpScale) : null;
        m_bumpTexture = mat.HasProperty(ShaderRef._BumpMap) ? mat.GetTexture(ShaderRef._BumpMap)?.name : null;
        m_flowMaskTexture = mat.HasProperty(ShaderRef._FlowMaskTex) ? mat.GetTexture(ShaderRef._FlowMaskTex)?.name : null;
        m_flowSpeedAll = mat.HasProperty(ShaderRef._FlowSpeedAll) ? mat.GetFloat(ShaderRef._FlowSpeedAll) : null;
        m_flowSpeedY = mat.HasProperty(ShaderRef._FlowSpeedY) ? mat.GetFloat(ShaderRef._FlowSpeedY) : null;
        m_flowTexture = mat.HasProperty(ShaderRef._FlowTexture) ? mat.GetTexture(ShaderRef._FlowTexture)?.name : null;
        m_flowColor = mat.HasProperty(ShaderRef._FlowColor) ? mat.GetColor(ShaderRef._FlowColor).ToRGBAString() : null;
        m_edgeColor = mat.HasProperty(ShaderRef._SSS) ? mat.GetColor(ShaderRef._SSS).ToRGBAString() : null;
        m_emissiveColor = mat.HasProperty(ShaderRef._EmissiveColor)
            ? mat.GetColor(ShaderRef._EmissiveColor).ToRGBAString()
            : null;
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
                Color color = m_emissionColor.FromHexOrRGBA(material.GetColor(ShaderRef._EmissionColor));
                material.SetColor(ShaderRef._EmissionColor, color * color.a);
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

        if (m_glossiness != null && material.HasProperty(ShaderRef._Glossiness))
        {
            material.SetFloat(ShaderRef._Glossiness, m_glossiness.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_glossiness: {m_glossiness.Value}");
            }
        }

        if (m_metallic != null && material.HasProperty(ShaderRef._Metallic))
        {
            material.SetFloat(ShaderRef._Metallic, m_metallic.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_metallic: {m_metallic.Value}");
            }
        }

        if (m_bumpScale != null && material.HasProperty(ShaderRef._BumpScale))
        {
            material.SetFloat(ShaderRef._BumpScale, m_bumpScale.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_bumpScale: {m_bumpScale.Value}");
            }
        }

        if (m_bumpTexture != null && material.HasProperty(ShaderRef._BumpMap))
        {
            Texture? tex = TextureManager.GetTexture(m_bumpTexture, material.GetTexture(ShaderRef._BumpMap));
            material.SetTexture(ShaderRef._BumpMap, tex);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_bumpTexture: {tex?.name ?? "null"}");
            }
        }

        if (m_flowMaskTexture != null && material.HasProperty(ShaderRef._FlowMaskTex))
        {
            Texture? tex = TextureManager.GetTexture(m_flowMaskTexture, material.GetTexture(ShaderRef._FlowMaskTex));
            material.SetTexture(ShaderRef._FlowMaskTex, tex);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_flowMaskTex: {tex?.name ?? "null"}");
            }
        }

        if (m_flowSpeedAll != null && material.HasProperty(ShaderRef._FlowSpeedAll))
        {
            material.SetFloat(ShaderRef._FlowSpeedAll, m_flowSpeedAll.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_flowSpeedAll: {m_flowSpeedAll.Value}");
            }
        }

        if (m_flowSpeedY != null && material.HasProperty(ShaderRef._FlowSpeedY))
        {
            material.SetFloat(ShaderRef._FlowSpeedY, m_flowSpeedY.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_flowSpeedY: {m_flowSpeedY.Value}");
            }
        }

        if (m_flowTexture != null && material.HasProperty(ShaderRef._FlowTexture))
        {
            Texture? tex = TextureManager.GetTexture(m_flowTexture, material.GetTexture(ShaderRef._FlowTexture));
            material.SetTexture(ShaderRef._FlowTexture, tex);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_flowTexture: {tex?.name ?? "null"}");
            }
        }

        if (m_flowColor != null && material.HasProperty(ShaderRef._FlowColor))
        {
            Color color = m_flowColor.FromHexOrRGBA(material.GetColor(ShaderRef._FlowColor));
            material.SetColor(ShaderRef._FlowColor, color);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_flowColor: {color}");
            }
        }

        if (m_edgeColor != null && material.HasProperty(ShaderRef._SSS))
        {
            Color color = m_edgeColor.FromHexOrRGBA(material.GetColor(ShaderRef._SSS));
            material.SetColor(ShaderRef._SSS, color);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_edgeColor: {color}");
            }
        }

        if (m_emissiveColor != null && material.HasProperty(ShaderRef._EmissiveColor))
        {
            Color color = m_emissiveColor.FromHexOrRGBA(material.GetColor(ShaderRef._EmissiveColor));
            material.SetColor(ShaderRef._EmissiveColor, color);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{material.name}] m_emissiveColor: {color}");
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
    
    public static Color FromHexOrRGBA(this string input, Color defaultValue)
    {
        if (string.IsNullOrWhiteSpace(input)) return defaultValue;

        if (input.StartsWith("#"))
        {
            input = input.TrimStart('#');

            if (input.Length != 6 && input.Length != 8)
                return defaultValue;

            byte r = byte.Parse(input.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(input.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(input.Substring(4, 2), NumberStyles.HexNumber);
            byte a = input.Length == 8
                ? byte.Parse(input.Substring(6, 2), NumberStyles.HexNumber)
                : (byte)255;

            return new Color32(r, g, b, a);
        }
        if (input.StartsWith("RGBA"))
        {
            int start = input.IndexOf('(') + 1;
            int end = input.IndexOf(')');
            string[] values = input.Substring(start, end - start).Split(',');
            if (values.Length != 4) return defaultValue;
            if (!float.TryParse(values[0], NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out float r)) return defaultValue;
            if (!float.TryParse(values[1], NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out float g)) return defaultValue;
            if (!float.TryParse(values[2], NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out float b)) return defaultValue;
            if (!float.TryParse(values[3], NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out float a)) return defaultValue;
            return new Color(r, g, b, a);
        }

        return defaultValue;
    }
}