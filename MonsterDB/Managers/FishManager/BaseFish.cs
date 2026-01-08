using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseFish : BaseItem
{
    [YamlMember(Order = 8, Description = "If field removed, will remove component")] public FishRef? Fish;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        if (prefab == null || !prefab.TryGetComponent(out ItemDrop item)) return;
        GameVersion = Version.GetVersionString();
        ModVersion = MonsterDBPlugin.ModVersion;
        Type = BaseType.Fish;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        ItemData = new ItemDataSharedRef();
        ItemData.SetBasicFields(item.m_itemData.m_shared);
        Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>(true);
        Visuals = new VisualRef();
        Visuals.m_scale = prefab.transform.localScale;
        if (renderers.Length > 0)
        {
            Visuals.m_renderers = renderers.ToRef();
        }
        if (prefab.TryGetComponent(out Fish fish))
        {
            Fish = fish;
        }
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        base.Update();
        UpdateFish(prefab);
    }

    private void UpdateFish(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out Fish? component))
        {
            if (Fish != null)
            {
                component = prefab.AddComponent<Fish>();
            }
        }
        else
        {
            if (Fish == null)
            {
                prefab.Remove<Fish>();
                component = null;
            }
        }

        if (component == null || Fish == null) return;
        component.SetFieldsFrom(Fish);
    }
}