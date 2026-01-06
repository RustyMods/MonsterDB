using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseItem : Base
{
    [YamlMember(Order = 6)] public ItemDataSharedRef? ItemData;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        if (!prefab.TryGetComponent(out ItemDrop component)) return;
        base.Setup(prefab, isClone, source);
        Type = CreatureType.Item;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        ItemData = new ItemDataSharedRef();
        ItemData.ReferenceFrom(component.m_itemData.m_shared);
    }

    public override void Update()
    {
        if (ItemData == null) return;
        
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;

        ItemDrop? item = prefab.GetComponent<ItemDrop>();
        if (item == null) return;
        
        item.m_itemData.m_shared.SetFieldsFrom(ItemData);
        
        base.Update();
    }
}