using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseProjectile : Header
{
    [YamlMember(Order = 6)] public ProjectileRef? ProjectileData;
    [YamlMember(Order = 7)] public VisualRef? Visuals;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.Projectile;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;

        SetupProjectile(prefab);
        SetupVisuals(prefab);
    }

    protected virtual void SetupProjectile(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out Projectile component)) return;
        ProjectileData = component;
    }
    
    protected virtual void SetupVisuals(GameObject prefab)
    {
        Visuals = new VisualRef()
        {
            m_scale = prefab.transform.localScale,
        };
        
        Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Visuals.m_renderers = renderers.ToRef();
        }
        ParticleSystem[] particleSystems = prefab.GetComponentsInChildren<ParticleSystem>(true);
        if (particleSystems.Length > 0)
        {
            Visuals.m_particleSystems = particleSystems.ToRef();
        }
        Light[] lights = prefab.GetComponentsInChildren<Light>(true);
        if (lights.Length > 0)
        {
            Visuals.m_lights = lights.ToRef();
        }
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        
        ProjectileManager.TrySave(prefab, out _, IsCloned, ClonedFrom);
        
        UpdatePrefab(prefab);
        base.Update();
    }

    protected virtual void UpdatePrefab(GameObject prefab)
    {
        UpdateVisuals(prefab);
        UpdateProjectile(prefab);
    }

    protected void UpdateProjectile(GameObject prefab)
    {
        if (ProjectileData == null || !prefab.TryGetComponent(out Projectile component)) return;
        component.SetFieldsFrom(ProjectileData);
    }
    
    protected virtual void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            if (Visuals.m_scale.HasValue)
            {
                prefab.transform.localScale = Visuals.m_scale.Value;
            }

            Visuals.UpdateRenderers(prefab);
            Visuals.UpdateParticleSystems(prefab);
            Visuals.UpdateLights(prefab);
        }
    }
}