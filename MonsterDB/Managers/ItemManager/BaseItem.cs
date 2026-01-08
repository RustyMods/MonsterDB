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
        if (!prefab.TryGetComponent(out ItemDrop component)) return;
        base.Setup(prefab, isClone, source);
        Type = BaseType.Item;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        ItemData = new ItemDataSharedRef();
        ItemData.ReferenceFrom(component.m_itemData.m_shared);
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
        if (ItemData == null) return;
        
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;

        ItemDrop? item = prefab.GetComponent<ItemDrop>();
        if (item == null) return;
        
        item.m_itemData.m_shared.SetFieldsFrom(ItemData);

        if (Visuals != null)
        {
            if (Visuals.m_scale.HasValue)
            {
                item.transform.localScale = Visuals.m_scale.Value;
            }

            Visuals.UpdateRenderers(prefab);
        }
        
        base.Update();
    }
}