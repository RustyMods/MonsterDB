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

    protected virtual void SetupItem(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ItemDrop component)) return;
        ItemData = new ItemDataSharedRef();
        ItemData.SetFrom(component.m_itemData.m_shared);
    }

    protected virtual void SetupVisuals(GameObject prefab)
    {
        Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Visuals = new VisualRef
            {
                m_scale = prefab.transform.localScale,
                m_renderers = renderers.ToRef()
            };
        }
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        SaveDefault(prefab);
        
        UpdatePrefab(prefab);
        
        base.Update();
    }

    protected virtual void SaveDefault(GameObject prefab)
    {
        ItemManager.Save(prefab, IsCloned, ClonedFrom);
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
        
        item.m_itemData.m_shared.SetFieldsFrom(ItemData);
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
                }
            }

            Visuals.UpdateRenderers(prefab);
        }
    }
}