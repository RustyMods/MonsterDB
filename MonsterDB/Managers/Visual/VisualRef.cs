using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class VisualRef : Reference
{
    public Vector3Ref? m_scale;
    public List<LevelSetupRef>? m_levelSetups;
    public RendererRef[]? m_renderers;
    public LightRef[]? m_lights;
    public ParticleSystemRef[]? m_particleSystems;
    public int[]? m_modelIndex;
    public string[]? m_beards;
    public string[]? m_hairs;
    public string[]? m_hairColors;
    public string[]? m_skinColors;

    public void Update(GameObject prefab, bool isInstance, bool isItem)
    {
        UpdateRenderers(prefab, isInstance, isItem);
        UpdateLights(prefab, isInstance);
        UpdateParticleSystems(prefab, isInstance);
    }

    public void UpdateChildrenScale(GameObject prefab, Renderer[] renderers, bool log)
    {
        log &= ConfigManager.ShouldLogDetails();
        
        if (m_scale.HasValue)
        {
            for (int i = 0; i < renderers.Length; ++i)
            {
                Renderer renderer = renderers[i];
                GameObject go = renderer.gameObject;

                go.transform.localScale = Vector3.Scale(go.transform.localScale, m_scale.Value);
                    
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{prefab.name}][{go.name}] m_scale: {go.transform.localScale.ToString()}");
                }
            }
        }
    }

    public void UpdateScale(GameObject prefab, bool log)
    {
        log &= ConfigManager.ShouldLogDetails();
        if (m_scale.HasValue)
        {
            prefab.transform.localScale = m_scale.Value;
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{prefab.name}] m_scale: {prefab.transform.localScale.ToString()}");
            }
        }
    }
    public void UpdateRenderers(GameObject prefab, bool isInstance, bool isItem)
    {
        Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>(true);

        if (!isItem) UpdateScale(prefab, !isInstance);
        else UpdateChildrenScale(prefab, renderers, !isInstance);
        
        if (m_renderers == null) return;
        
        Dictionary<(string m_prefab, string? m_parent, int? m_index), RendererRef> exactMatchLookup = m_renderers
            .GroupBy(x => (x.m_prefab, x.m_parent, x.m_index))
            .ToDictionary(x => x.Key, x => x.First());
    
        Dictionary<string, RendererRef> fallbackLookup = m_renderers
            .GroupBy(x => x.m_prefab)
            .ToDictionary(g => g.Key, g => g.First());

        for (int index = 0; index < renderers.Length; ++index)
        {
            Renderer renderer = renderers[index];
            string? parent = renderer.transform.parent?.name;
            int i = renderer.transform.GetSiblingIndex();

            (string name, string? parent, int i) key = (renderer.name, parent, i);
            if (exactMatchLookup.TryGetValue(key, out RendererRef reference))
            {
                reference.Update(renderer, prefab.name, !isInstance);
            }
            else if (fallbackLookup.TryGetValue(renderer.name, out reference))
            {
                reference.Update(renderer, prefab.name, !isInstance);
            }
        }
    }

    public void UpdateLights(GameObject prefab, bool isInstance)
    {
        if (m_lights == null) return;
        
        Dictionary<(string m_prefab, string? m_parent, int? m_index), LightRef> exactMatchLookup = m_lights
            .GroupBy(x => (x.m_prefab, x.m_parent, x.m_index))
            .ToDictionary(x => x.Key, x => x.First());
    
        Dictionary<string, LightRef> fallbackLookup = m_lights
            .GroupBy(x => x.m_prefab)
            .ToDictionary(g => g.Key, g => g.First());
        
        Light[]? lights = prefab.GetComponentsInChildren<Light>(true);

        for (int i = 0; i < lights.Length; ++i)
        {
            Light light = lights[i];
            string? parent = light.transform.parent?.name;
            int index = light.transform.GetSiblingIndex();
            
            (string name, string? parent, int i) key = (light.name, parent, index);
            if (exactMatchLookup.TryGetValue(key, out LightRef reference))
            {
                reference.Update(light, prefab.name, !isInstance);
            }
            else if (fallbackLookup.TryGetValue(light.name, out reference))
            {
                reference.Update(light, prefab.name, !isInstance);
            }
        }
    }

    public void UpdateParticleSystems(GameObject prefab, bool isInstance)
    {
        if (m_particleSystems == null) return;
        
        Dictionary<(string m_prefab, string? m_parent, int? m_index), ParticleSystemRef> exactMatchLookup = m_particleSystems
            .GroupBy(x => (x.m_prefab, x.m_parent, x.m_index))
            .ToDictionary(x => x.Key, x => x.First());
    
        Dictionary<string, ParticleSystemRef> fallbackLookup = m_particleSystems
            .GroupBy(x => x.m_prefab)
            .ToDictionary(g => g.Key, g => g.First());
        
        ParticleSystem[]? lights = prefab.GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < lights.Length; ++i)
        {
            ParticleSystem ps = lights[i];
            string? parent = ps.transform.parent?.name;
            int index = ps.transform.GetSiblingIndex();
            
            (string name, string? parent, int i) key = (ps.name, parent, index);
            if (exactMatchLookup.TryGetValue(key, out ParticleSystemRef reference))
            {
                reference.Update(ps, prefab.name, !isInstance);
            }
            else if (fallbackLookup.TryGetValue(ps.name, out reference))
            {
                reference.Update(ps, prefab.name, !isInstance);
            }
        }
    }

    public void SetDefaultHumanFields()
    {
        m_modelIndex = new [] { 0, 1 };;
        m_hairs = new[]
        {
            "Hair1", "Hair2", "Hair3", "Hair4", "Hair5", 
            "Hair6", "Hair7", "Hair8", "Hair9", "Hair10", 
            "Hair11", "Hair12", "Hair13", "Hair14", "Hair15", 
            "Hair16", "Hair17", "Hair18", "Hair19", "Hair20", 
            "Hair21", "Hair22", "Hair23", "Hair24", "Hair25", 
            "Hair26", "Hair27", "Hair28", "Hair29", "Hair30", 
            "Hair31", "Hair32", "Hair33", "Hair34", "HairNone"
        };;
        m_beards = new[]
        {
            "Beard1", "Beard2", "Beard3", "Beard4", "Beard5",
            "Beard6", "Beard7", "Beard8", "Beard9", "Beard10",
            "Beard11", "Beard12", "Beard13", "Beard14", "Beard15",
            "Beard16", "Beard17", "Beard18", "Beard19", "Beard20",
            "Beard21", "Beard22", "Beard23", "Beard24", "Beard25",
            "Beard26", "BeardNone"
        };;
        m_skinColors = new[]
        {
            "#FFFFFF", // Base / Pale (no tint)
            "#FFF2EB", // Very Fair
            "#FFEBDB", // Fair with Pink undertone
            "#FFF0E0", // Fair
            "#FFE6D1", // Light
            "#FFE0C7", // Light Medium
            "#FAD9B8", // Medium
            "#F2D1AD", // Medium Tan
            "#EBC79E", // Tan
            "#E0B88F", // Deep Tan
            "#D9AD85", // Light Brown
            "#C79E7A", // Medium Brown
            "#FFE0BF", // Warm Beige
            "#FAE6CC", // Peachy Fair
            "#F2DBC2", // Golden Light
        };;
        m_hairColors = new[]
        {
            "#000000", // Black
            "#FAF0BF", // Platinum Blonde
            "#A15C00", // Brown
            "#26140D", // Dark Brown
            "#594026", // Medium Brown
            "#8C7359", // Light Brown
            "#F2DE82", // Golden Blonde
            "#D9BF73", // Dirty Blonde
            "#B8A685", // Sandy Blonde
            "#732E14", // Auburn
            "#8C1F0D", // Dark Red
            "#B8401F", // Ginger
            "#666666", // Dark Gray
            "#A6A6A6", // Silver Gray
            "#E0E0E6", // White / Silver
            "#404047", // Charcoal
        };;
    }
}