using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

public class RendererRef : Reference
{
    public string m_prefab = "";
    public string? m_parent;
    public int? m_index;
    public bool? m_active;
    public bool? m_enabled;
    public MaterialRef[]? m_materials;

    public void Update(Renderer renderer)
    {
        if (m_active.HasValue)
        {
            renderer.gameObject.SetActive(m_active.Value);
        }

        if (m_enabled.HasValue)
        {
            renderer.enabled = m_enabled.Value;   
        }

        if (m_materials != null)
        {
            UpdateMaterials(renderer, m_materials);
        }
    }
    
    private static void UpdateMaterials(Renderer renderer, MaterialRef[] materialRefs)
    {
        Dictionary<string, MaterialRef> dict = materialRefs
                .ToDict(f => f.m_name);
            
        Material[]? materials = renderer.sharedMaterials;
        for (int i = 0; i < materials.Length; ++i)
        {
            Material? mat = materials[i];
            if (mat == null) continue;
            string matName = mat.name.Replace("(Instance)", string.Empty);
            if (!dict.TryGetValue(matName, out MaterialRef matRef))
            {
                MonsterDBPlugin.LogWarning($"Failed to find material: {matName} in references");
                continue;
            }

            Material newMat = new Material(mat);
            newMat.name = matName;
            
            if (newMat.shader != null && newMat.shader.name != matRef.m_shader && !string.IsNullOrEmpty(matRef.m_shader))
            {
                newMat.shader = ShaderRef.GetShader(matRef.m_shader, newMat.shader);
            }
            if (newMat.HasProperty(ShaderRef._Color) && matRef.m_color != null)
            {
                if (!string.IsNullOrEmpty(matRef.m_color))
                {
                    newMat.color = matRef.m_color.FromHex(mat.color);
                }
            }
            if (matRef.m_hue != null && newMat.HasProperty(ShaderRef._Hue))
            {
                newMat.SetFloat(ShaderRef._Hue, matRef.m_hue.Value);
            }
            if (matRef.m_saturation != null && newMat.HasProperty(ShaderRef._Saturation))
            {
                newMat.SetFloat(ShaderRef._Saturation, matRef.m_saturation.Value);
            }
            if (matRef.m_value != null && newMat.HasProperty(ShaderRef._Value))
            {
                newMat.SetFloat(ShaderRef._Value, matRef.m_value.Value);
            }
            if (matRef.m_emissionColor != null && newMat.HasProperty(ShaderRef._EmissionColor))
            {
                if (!string.IsNullOrEmpty(matRef.m_emissionColor))
                {
                    newMat.SetColor(ShaderRef._EmissionColor, matRef.m_emissionColor.FromHex(mat.GetColor(ShaderRef._EmissionColor)));
                }
            }
            if (matRef.m_mainTexture != null && matRef.m_mainTexture != (newMat.mainTexture?.name ?? ""))
            {
                newMat.mainTexture = TextureManager.GetTexture(matRef.m_mainTexture, newMat.mainTexture);;
            }

            if (newMat.HasProperty(ShaderRef._TintColor) && matRef.m_tintColor != null)
            {
                if (!string.IsNullOrEmpty(matRef.m_tintColor))
                {
                    newMat.SetColor(ShaderRef._TintColor, matRef.m_tintColor.FromHex(newMat.GetColor(ShaderRef._TintColor)));
                }
            }

            if (matRef.m_emissionTexture != null && !string.IsNullOrEmpty(matRef.m_emissionTexture) &&
                newMat.HasProperty(ShaderRef._EmissionMap))
            {
                newMat.SetTexture(ShaderRef._EmissionMap, TextureManager.GetTexture(matRef.m_emissionTexture, newMat.GetTexture(ShaderRef._EmissionMap)));
            }
            
            materials[i] = newMat;
        }
        renderer.materials = materials;
        renderer.sharedMaterials = materials;
    }
}

public partial class Extensions
{
    public static RendererRef[] ToRef(this Renderer[] renderers)
    {
        RendererRef[] reference = renderers
        .Select(x => new RendererRef()
        {
            m_prefab = x.name,
            m_parent = x.transform.parent?.name,
            m_index = x.transform.GetSiblingIndex(),
            m_active = x.gameObject.activeSelf,
            m_enabled = x.enabled,
            m_materials = x.sharedMaterials.ToRef()
        })
        .OrderBy(x => x.m_parent)
        .ThenBy(x => x.m_index)
        .ToArray();
        return reference;
    }
}