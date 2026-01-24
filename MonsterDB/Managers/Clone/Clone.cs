using System;
using UnityEngine;

namespace MonsterDB;

public class Clone
{
    private GameObject? Source;
    private GameObject? Prefab;
    public readonly string PrefabName;
    private readonly string NewName;
    private bool Loaded;
    
    public event Action<GameObject>? OnCreated;

    public Clone(string prefabName, string newName)
    {
        PrefabName = prefabName;
        NewName = newName;
        CloneManager.clones[newName] = this;
    }

    public Clone(GameObject prefab, string newName)
    {
        PrefabName = prefab.name;
        NewName = newName;
        Source = prefab;
        CloneManager.clones[newName] = this;
    }

    internal GameObject? Create()
    {
        if (Loaded) return Prefab;

        if (Source == null)
        {
            if (PrefabManager.GetPrefab(PrefabName) is not { } prefab) return Prefab;
            Source = prefab;
        }
        
        Prefab = UnityEngine.Object.Instantiate(Source, CloneManager.GetRootTransform(), false);
        Prefab.name = NewName;
        OnCreated?.Invoke(Prefab);
        PrefabManager.RegisterPrefab(Prefab);
        CloneManager.prefabs[Prefab.name] = Prefab;
        Loaded = true;
        return Prefab;
    }
}