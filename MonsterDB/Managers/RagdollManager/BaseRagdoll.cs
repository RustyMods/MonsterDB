using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseRagdoll : Header
{
    [YamlMember(Order = 6)] public RagdollRef? Ragdoll;
    [YamlMember(Order = 7)] public VisualRef? Visuals;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        if (!prefab.TryGetComponent(out Ragdoll component)) return;
        base.Setup(prefab, isClone, source);
        Type = BaseType.Ragdoll;
        Ragdoll = new RagdollRef();
        Ragdoll.Setup(component);
        IsCloned = isClone;
        ClonedFrom = source;
        Prefab = prefab.name;
        SetupVisuals(prefab);
    }

    private void SetupVisuals(GameObject prefab)
    {
        Visuals = new  VisualRef();
        Visuals.m_scale = prefab.transform.localScale;
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
        UpdatePrefab(prefab);
        base.Update();
        SyncManager.files.PrefabToUpdate = Prefab;
        SyncManager.files.Add(this);
    }

    private void UpdatePrefab(GameObject prefab)
    {
        if (Ragdoll != null && prefab.TryGetComponent(out Ragdoll component))
        {
            Ragdoll.UpdateFields(component, prefab.name, true);
        }

        if (Visuals != null)
        {
            Visuals.Update(prefab, false);
            if (Visuals.m_scale.HasValue)
            {
                prefab.transform.localScale =  Visuals.m_scale.Value;
                if (ConfigManager.ShouldLogDetails())
                {
                    MonsterDBPlugin.LogDebug($"[{prefab.name}] m_scale: {prefab.transform.localScale.ToString()}");
                }
            }
        }
    }
}