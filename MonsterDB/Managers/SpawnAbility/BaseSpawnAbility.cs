using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseSpawnAbility : Header
{
    [YamlMember(Order = 6)] public SpawnAbilityRef? SpawnAbility;
    [YamlMember(Order = 7)] public VisualRef? Visuals;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.SpawnAbility;
        Prefab = prefab.name;
        IsCloned = isClone;
        ClonedFrom = source;
        Visuals = new VisualRef(prefab);
        SetupSpawnAbility(prefab);
    }
    
    public override VisualRef? GetVisualData() => Visuals;

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseSpawnAbility originalAbility) return;
        if (SpawnAbility != null && originalAbility.SpawnAbility != null) SpawnAbility.ResetTo(originalAbility.SpawnAbility);
        if (Visuals != null && originalAbility.Visuals != null) Visuals.ResetTo(originalAbility.Visuals);
    }

    private void SetupSpawnAbility(GameObject prefab)
    {
        if (prefab.TryGetComponent(out SpawnAbility spawnAbility))
        {
            SpawnAbility = new SpawnAbilityRef(spawnAbility);
        }
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

    private void UpdatePrefab(GameObject prefab)
    {
        UpdateVisuals(prefab);
        UpdateSpawnAbility(prefab);
    }

    private void UpdateSpawnAbility(GameObject prefab)
    {
        if (SpawnAbility != null && prefab.TryGetComponent(out SpawnAbility component))
        {
            SpawnAbility.UpdateFields(component, prefab.name, true);
        }
    }
    
    protected virtual void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.Update(prefab, false, false);
        }
    }
}