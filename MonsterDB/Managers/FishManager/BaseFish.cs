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
        SetupVersions();
        SetupVisuals(prefab);
        SetupItem(prefab);
        Type = BaseType.Fish;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        if (prefab.TryGetComponent(out Fish fish))
        {
            Fish = fish;
        }
    }

    public override void Update()
    {
        base.Update();
        SyncManager.files.PrefabToUpdate = Prefab;
        SyncManager.files.Add(this);
    }

    protected override void SaveDefault(GameObject prefab)
    {
        FishManager.Save(prefab, IsCloned, ClonedFrom);
    }

    protected override void UpdatePrefab(GameObject prefab)
    {
        base.UpdatePrefab(prefab);
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
        Fish.UpdateFields(component, prefab.name, true);
    }
}