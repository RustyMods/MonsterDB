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

    public override VisualRef? GetVisualData() => Visuals;

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseItem originalItem) return;
        if (ItemData != null && originalItem.ItemData != null) ItemData.ResetTo(originalItem.ItemData);
        if (Visuals != null && originalItem.Visuals != null) Visuals.ResetTo(originalItem.Visuals);
    }

    protected void SetupItem(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ItemDrop component)) return;
        ItemData = new ItemDataSharedRef(component.m_itemData.m_shared);
    }

    protected void SetupVisuals(GameObject prefab)
    {
        Visuals = new VisualRef(prefab);
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

    protected void UpdateItem(GameObject prefab)
    {
        if (ItemData == null) return;
        ItemDrop? item = prefab.GetComponent<ItemDrop>();
        if (item == null) return;
        ItemData.UpdateFields(item.m_itemData.m_shared, prefab.name, true);
    }

    protected void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.Update(prefab, false, true);
        }
    }
}