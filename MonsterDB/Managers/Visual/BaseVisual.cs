using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseVisual : Header
{
    [YamlMember(Order = 6)] public VisualRef? Visuals;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.Visual;
        Prefab = prefab.name;
        IsCloned = isClone;
        ClonedFrom = source;
        SetupVisuals(prefab);
    }

    public override VisualRef? GetVisualData() => Visuals;

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseVisual originalVisual) return;
        if (Visuals != null && originalVisual.Visuals != null) Visuals.ResetTo(originalVisual.Visuals);
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        UpdatePrefab(prefab);
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }
    
    private void SetupVisuals(GameObject prefab)
    {
        Visuals = new  VisualRef();
        Visuals.m_scale = prefab.transform.localScale;
        Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Visuals.m_renderers = renderers.ToRendererRefArray();
        }
        ParticleSystem[] particleSystems = prefab.GetComponentsInChildren<ParticleSystem>(true);
        if (particleSystems.Length > 0)
        {
            Visuals.m_particleSystems = particleSystems.ToParticleSystemRefArray();
        }
        Light[] lights = prefab.GetComponentsInChildren<Light>(true);
        if (lights.Length > 0)
        {
            Visuals.m_lights = lights.ToRef();
        }
    }

    private void UpdatePrefab(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.Update(prefab, false, prefab.GetComponent<ItemDrop>());
        }
    }
}