using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class RendererRef : Reference
{
    public string m_prefab = "";
    public string? m_parent;
    public int? m_index;
    public bool? m_active;
    public bool? m_enabled;
    public MaterialRef[]? m_materials;
    
    public RendererRef(){}

    public RendererRef(Renderer renderer)
    {
        m_prefab = renderer.name;
        m_parent = renderer.transform.parent?.name;
        m_index = renderer.transform.GetSiblingIndex();
        m_active = renderer.gameObject.activeSelf;
        m_enabled = renderer.enabled;
        m_materials = renderer.sharedMaterials.ToMaterialRefArray();
    }

    public void Update(Renderer renderer, string targetName, bool log)
    {
        if (log && !string.IsNullOrEmpty(targetName))
        {
            if (LoadManager.resetting)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}].[{renderer.name}] Resetting Renderer");
            }
            else
            {
                MonsterDBPlugin.LogDebug($"[{targetName}].[{renderer.name}] Updating Renderer");
            }
        }
        
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
            UpdateMaterials(renderer, m_materials, $"{targetName}.{renderer.name}", log);
        }
    }
    
    private static Dictionary<string, Material> _cachedMaterials = new Dictionary<string, Material>();
    
    private static void UpdateMaterials(Renderer renderer, MaterialRef[] materialRefs, string targetName, bool log)
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
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}]/[{renderer.name}] {matName} not found in reference file");
                continue;
            }

            if (matRef.m_materialOverride != null)
            {
                if (_cachedMaterials.TryGetValue(matRef.m_materialOverride, out Material matOverride))
                {
                    mat = matOverride;
                }
                else
                {
                    CacheMaterials();
                    if (_cachedMaterials.TryGetValue(matRef.m_materialOverride, out matOverride))
                    {
                        mat = matOverride;
                    }
                }
            }

            Material newMat = new Material(mat);
            newMat.name = matName;

            matRef.Update(newMat, targetName, log);
            
            materials[i] = newMat;
        }
        renderer.materials = materials;
        renderer.sharedMaterials = materials;
    }

    private static void CacheMaterials()
    {
        Material[]? mats = Resources.FindObjectsOfTypeAll<Material>();
        for (int i = 0; i < mats.Length; ++i)
        {
            Material? mat = mats[i];
            if (mat == null || mat.GetInstanceID() <= 0 || _cachedMaterials.ContainsKey(mat.name)) continue;
            _cachedMaterials[mat.name] = mat;
        }
    }
}

public partial class Extensions
{
    public static RendererRef[] ToRendererRefArray(this Renderer[] renderers)
    {
        RendererRef[] reference = renderers
        .Select(x => new RendererRef(x))
        .OrderBy(x => x.m_parent)
        .ThenBy(x => x.m_index)
        .ToArray();
        return reference;
    }
}