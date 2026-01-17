using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseProjectile : Header
{
    [YamlMember(Order = 6)] public ProjectileRef? ProjectileData;
    [YamlMember(Order = 7)] public TeleportAbilityRef? TeleportAbility;
    [YamlMember(Order = 8)] public TriggerSpawnAbilityRef? TriggerSpawnAbility;
    [YamlMember(Order = 9)] public VisualRef? Visuals;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.Projectile;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;

        SetupProjectile(prefab);
        SetupTeleportAbility(prefab);
        SetupVisuals(prefab);
    }

    protected virtual void SetupTeleportAbility(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out TeleportAbility component)) return;
        TeleportAbility = component;
    }

    protected virtual void SetupTriggerSpawnAbility(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out TriggerSpawnAbility component)) return;
        TriggerSpawnAbility = new TriggerSpawnAbilityRef();
        TriggerSpawnAbility.m_range = component.m_range;
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
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }

    protected virtual void UpdatePrefab(GameObject prefab)
    {
        UpdateVisuals(prefab);
        UpdateProjectile(prefab);
        UpdateTeleportAbility(prefab);
        UpdateTriggerSpawnAbility(prefab);
    }

    protected void UpdateTriggerSpawnAbility(GameObject prefab)
    {
        if (TriggerSpawnAbility == null || !TriggerSpawnAbility.m_range.HasValue || !prefab.TryGetComponent(out TriggerSpawnAbility component)) return;
        component.m_range = TriggerSpawnAbility.m_range.Value;
        if (ConfigManager.ShouldLogDetails())
        {
            MonsterDBPlugin.LogDebug($"[{prefab.name}] TriggerSpawnAbility.m_range: {component.m_range}");
        }
    }

    protected void UpdateTeleportAbility(GameObject prefab)
    {
        if (TeleportAbility == null || !prefab.TryGetComponent(out TeleportAbility component)) return;
        TeleportAbility.Update(component, prefab.name);
    }

    protected void UpdateProjectile(GameObject prefab)
    {
        if (ProjectileData == null || !prefab.TryGetComponent(out Projectile component)) return;
        ProjectileData.UpdateFields(component, prefab.name, true);
    }
    
    protected virtual void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            if (Visuals.m_scale.HasValue)
            {
                prefab.transform.localScale = Visuals.m_scale.Value;
                if (ConfigManager.ShouldLogDetails())
                {
                    MonsterDBPlugin.LogDebug($"[{prefab.name}] m_scale: {prefab.transform.localScale.ToString()}");
                }
            }

            Visuals.Update(prefab, false);
        }
    }
}