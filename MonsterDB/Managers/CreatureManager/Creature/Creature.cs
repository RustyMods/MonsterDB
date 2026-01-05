using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class Creature : BaseCreature
{
    [YamlMember(Order = 8)] public VisualRef? Visuals;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Visuals = new VisualRef();
        Visuals.m_scale = prefab.transform.localScale;

        LevelEffects levelEffects = prefab.GetComponentInChildren<LevelEffects>();
        Renderer? renderer = null;
        if (levelEffects != null)
        {
            Visuals.m_levelSetups = levelEffects.m_levelSetups.ToRef();
            renderer = levelEffects.m_mainRender;
        }

        if (renderer == null && prefab.TryGetComponent(out VisEquipment visEq))
        {
            renderer = visEq.m_bodyModel;
        }
        
        if (renderer == null) renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();

        if (renderer != null)
        {
            Visuals.m_materials = renderer.sharedMaterials.ToRef();
        }
    }

    protected void UpdateLevelEffects(LevelEffects? levelEffects, out Renderer? mainRenderer)
    {
        mainRenderer = null;
        if (levelEffects != null && Visuals != null && Visuals.m_levelSetups != null)
        {
            mainRenderer = levelEffects.m_mainRender;
            List<LevelEffects.LevelSetup> setups = new();
            foreach (LevelSetupRef? levelRef in Visuals.m_levelSetups)
            {
                LevelEffects.LevelSetup levelSetup = new LevelEffects.LevelSetup();
                levelSetup.SetFieldsFrom(levelRef);
                setups.Add(levelSetup);
            }
            levelEffects.m_levelSetups = setups;
        }
    }

    protected void UpdateMaterials(Renderer? renderer)
    {
        if (Visuals != null && Visuals.m_materials != null && renderer != null)
        {
            Dictionary<string, MaterialRef> dict = Visuals.m_materials
                .ToDictionary(f => f.m_name);
            
            Material[]? materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; ++i)
            {
                Material? mat = materials[i];
                if (mat == null) continue;
                if (!dict.TryGetValue(mat.name, out MaterialRef matRef))
                {
                    MonsterDBPlugin.LogWarning($"Failed to find material: {mat.name} in references");
                    continue;
                }
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
            renderer.materials = materials;
            renderer.sharedMaterials = materials;
        }
    }
    protected void UpdateScale(GameObject prefab, Character character)
    {
        if (Visuals == null || !Visuals.m_scale.HasValue) return;
        prefab.transform.localScale = Visuals.m_scale.Value;
        if (character != null && character.m_deathEffects != null && character.m_deathEffects.m_effectPrefabs != null)
        {
            foreach (EffectList.EffectData? effect in character.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab != null && effect.m_prefab.GetComponent<Ragdoll>())
                {
                    effect.m_prefab.transform.localScale = Visuals.m_scale.Value;
                    break;
                }
            }
        }
    }
}

