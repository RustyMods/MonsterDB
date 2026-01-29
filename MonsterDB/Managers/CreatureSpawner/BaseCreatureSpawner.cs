using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseCreatureSpawner : Header
{
    [YamlMember(Order = 6)] public CreatureSpawnerRef? CreatureSpawner;
    [YamlMember(Order = 7)] public RandomSpawnRef? RandomSpawn;
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.CreatureSpawner;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        SetupCreatureSpawner(prefab);
        SetupRandomSpawn(prefab);
    }

    public void SetupCreatureSpawner(GameObject prefab)
    {
        if (prefab.TryGetComponent(out CreatureSpawner component))
        {
            CreatureSpawner = new CreatureSpawnerRef(component);
        }
    }

    public void SetupRandomSpawn(GameObject prefab)
    {
        if (prefab.TryGetComponent(out RandomSpawn component))
        {
            RandomSpawn = new RandomSpawnRef(component);
        }
    }
    
    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseCreatureSpawner originalSpawner) return;
        if (CreatureSpawner != null && originalSpawner.CreatureSpawner != null)
        {
            CreatureSpawner.ResetTo(originalSpawner.CreatureSpawner);
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
        UpdateCreatureSpawner(prefab);
    }

    private void UpdateCreatureSpawner(GameObject prefab)
    {
        if (CreatureSpawner == null || !prefab.TryGetComponent(out CreatureSpawner component)) return;
        CreatureSpawner.UpdateFields(component, prefab.name, true);
    }

    private void UpdateRandomSpawn(GameObject prefab)
    {
        if (RandomSpawn == null || !prefab.TryGetComponent(out RandomSpawn component)) return;
        RandomSpawn.UpdateFields(component, prefab.name, true);
    }
}