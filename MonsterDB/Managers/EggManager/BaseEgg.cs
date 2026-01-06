using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class BaseEgg : Base
{
    [YamlMember(Order = 6)] public EggGrowRef? EggGrow;
    [YamlMember(Order = 7)] public VisualRef? Visuals;
    [YamlMember(Order = 8)] public ItemDataRef? Item;

    public override void Setup(GameObject prefab, bool isClone = false, string clonedFrom = "")
    {
        if (prefab == null) return;
        base.Setup(prefab, isClone, clonedFrom);
        Type = CreatureType.Egg;
        IsCloned = isClone;
        ClonedFrom = clonedFrom;
        Prefab = prefab.name;
        if (prefab.TryGetComponent(out EggGrow component))
        {
            EggGrow = new EggGrowRef();
            EggGrow.ReferenceFrom(component);
        }

        if (prefab.TryGetComponent(out ItemDrop item))
        {
            Item = new ItemDataRef();
            Item.ReferenceFrom(item.m_itemData.m_shared);
        }

        HashSet<Material> materials = new();
        foreach (Renderer? renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material? material in renderer.sharedMaterials)
            {
                if (material.name == "item_particle") continue;
                materials.Add(material);
            }
        }
        Visuals = new VisualRef
        {
            m_scale = prefab.transform.localScale,
            m_materials = materials.ToArray().ToRef()
        };
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;

        if (!prefab.TryGetComponent(out ItemDrop item)) return;

        EggManager.Save(prefab, IsCloned, ClonedFrom);

        UpdateItem(item);
        UpdateVisuals(prefab);
        UpdateEgg(prefab);
        
        base.Update();
    }

    private void UpdateItem(ItemDrop item)
    {
        if (Item != null)
        {
            item.m_itemData.m_shared.SetFieldsFrom(Item);
        }
    }

    private void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            if (Visuals.m_scale.HasValue)
            {
                prefab.transform.localScale = Visuals.m_scale.Value;
            }

            if (Visuals.m_materials != null)
            {
                Dictionary<string, MaterialRef> dict = Visuals.m_materials.ToDictionary(f => f.m_name);
                foreach (Renderer? renderer in prefab.GetComponentsInChildren<Renderer>(true))
                {
                    Material[]? materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        Material? mat = materials[i];
                        if (!dict.TryGetValue(mat.name, out MaterialRef matRef)) continue;
                        if (mat.shader.name != matRef.m_shader)
                        {
                            mat.shader = ShaderRef.GetShader(matRef.m_shader, mat.shader);
                        }
                        if (mat.HasProperty(ShaderRef._Color) && matRef.m_color != null)
                        {
                            if (!string.IsNullOrEmpty(matRef.m_color)) mat.color = matRef.m_color.FromHex(mat.color);
                        }
                        if (matRef.m_hue != null && mat.HasProperty(ShaderRef._Hue))
                        {
                            mat.SetFloat(ShaderRef._Hue, matRef.m_hue.Value);
                        }
                        if (matRef.m_saturation != null && mat.HasProperty(ShaderRef._Saturation))
                        {
                            mat.SetFloat(ShaderRef._Saturation, matRef.m_saturation.Value);
                        }
                        if (matRef.m_value != null && mat.HasProperty(ShaderRef._Value))
                        {
                            mat.SetFloat(ShaderRef._Value, matRef.m_value.Value);
                        }
                        if (matRef.m_emissionColor != null && mat.HasProperty(ShaderRef._EmissionColor))
                        {
                            if (!string.IsNullOrEmpty(matRef.m_emissionColor)) mat.SetColor(ShaderRef._EmissionColor, matRef.m_emissionColor.FromHex(mat.GetColor(ShaderRef._EmissionColor)));
                        }
                        if (matRef.m_mainTexture != null && matRef.m_mainTexture != (mat.mainTexture?.name ?? ""))
                        {
                            mat.mainTexture = TextureManager.GetTexture(matRef.m_mainTexture, mat.mainTexture);;
                        }
                    }
                }
            }
        }
    }

    private void UpdateEgg(GameObject prefab)
    {
        EggGrow? component = prefab.GetComponent<EggGrow>();
        if (component == null)
        {
            if (EggGrow != null && !string.IsNullOrEmpty(EggGrow.m_grownPrefab))
            {
                component = prefab.AddComponent<EggGrow>();
                EggManager.RegisterHoverOverride(prefab.name);
            }
        }
        else
        {
            if (EggGrow == null || string.IsNullOrEmpty(EggGrow.m_grownPrefab))
            {
                component = null;
                prefab.Remove<EggGrow>();
            }
        }

        if (component == null) return;
        
        if (EggGrow != null) component.SetFieldsFrom(EggGrow);
    }
}