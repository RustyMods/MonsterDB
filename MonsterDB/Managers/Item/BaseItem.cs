using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseItem : Header
{
    [YamlMember(Order = 6)] public ItemDataSharedRef? ItemData;
    [YamlMember(Order = 7)] public VisualRef? Visuals;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.Item;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        SetupItem(prefab);
        SetupVisuals(prefab);
    }

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseItem originalItem) return;
        if (ItemData != null && originalItem.ItemData != null) ItemData.ResetTo(originalItem.ItemData);
        if (Visuals != null && originalItem.Visuals != null) Visuals.ResetTo(originalItem.Visuals);
    }

    protected virtual void SetupItem(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ItemDrop component)) return;
        ItemData = new ItemDataSharedRef();
        ItemData.Setup(component.m_itemData.m_shared);
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
        SaveDefault(prefab);
        
        UpdatePrefab(prefab);
        
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }

    protected virtual void SaveDefault(GameObject prefab)
    {
        ItemManager.TrySave(prefab, out _, IsCloned, ClonedFrom);
    }

    protected virtual void UpdatePrefab(GameObject prefab)
    {
        UpdateItem(prefab);
        UpdateVisuals(prefab);
    }

    protected virtual void UpdateItem(GameObject prefab)
    {
        if (ItemData == null) return;
        ItemDrop? item = prefab.GetComponent<ItemDrop>();
        if (item == null) return;
        ItemData.UpdateFields(item.m_itemData.m_shared, prefab.name, true);
    }

    protected virtual void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            if (Visuals.m_scale.HasValue)
            {
                Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    Renderer? renderer = renderers[i];
                    GameObject go = renderer.gameObject;
                    go.transform.localScale = Visuals.m_scale.Value;
                    if (ConfigManager.ShouldLogDetails())
                    {
                        MonsterDBPlugin.LogDebug($"[{prefab.name}][{go.name}] m_scale: {go.transform.localScale.ToString()}");
                    }
                }
            }
            
            Visuals.Update(prefab, false);
        }
    }
}