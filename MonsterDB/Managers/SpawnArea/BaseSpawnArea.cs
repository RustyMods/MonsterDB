using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseSpawnArea : Header
{
    [YamlMember(Order = 6, Description = "If removed, will remove component")] public SpawnAreaRef? SpawnArea;
    [YamlMember(Order = 7)] public DestructibleRef? Destructible;
    [YamlMember(Order = 8)] public VisualRef? Visuals;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.SpawnArea;
        Prefab = prefab.name;
        IsCloned = isClone;
        ClonedFrom = source;
        Visuals = new VisualRef(prefab);
        SetupSpawnArea(prefab);
        SetupDestructible(prefab);
    }

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseSpawnArea originalSpawnArea) return;
        if (SpawnArea != null && originalSpawnArea.SpawnArea != null)
        {
            SpawnArea.ResetTo(originalSpawnArea.SpawnArea);
        }

        if (Visuals != null && originalSpawnArea.Visuals != null)
        {
            Visuals.ResetTo(originalSpawnArea.Visuals);
        }

        if (Destructible != null && originalSpawnArea.Destructible != null)
        {
            Destructible.ResetTo(originalSpawnArea.Destructible);
        }
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;

        SpawnAreaManager.Save(prefab, IsCloned, ClonedFrom);
        
        UpdatePrefab(prefab);
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }

    private void UpdatePrefab(GameObject prefab)
    {
        UpdateVisuals(prefab);
        UpdateSpawnArea(prefab);
        UpdateDestructible(prefab);
    }

    private void UpdateSpawnArea(GameObject prefab)
    {
        if (SpawnArea != null)
        {
            if (!prefab.TryGetComponent(out SpawnArea component))
            {
                component = prefab.AddComponent<SpawnArea>();
            }
            SpawnArea.UpdateFields(component, prefab.name, true);
        }
        else
        {
            prefab.Remove<SpawnArea>();
        }
    }

    private void UpdateDestructible(GameObject prefab)
    {
        if (Destructible != null && prefab.TryGetComponent(out Destructible component))
        {
            Destructible.UpdateFields(component, prefab.name, true);
        }
    }
    
    private void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.Update(prefab, false, false);
        }
    }

    private void SetupSpawnArea(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out SpawnArea component)) return;
        SpawnArea = new SpawnAreaRef(component);
    }

    private void SetupDestructible(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out Destructible component)) return;
        Destructible = new DestructibleRef(component);
    }
}