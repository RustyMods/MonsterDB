using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public class Clone
{
    private GameObject? Source;
    private GameObject? Prefab;
    private readonly string PrefabName;
    private readonly string NewName;
    private bool Loaded;
    
    public event Action<GameObject>? OnCreated;

    public Clone(string prefabName, string newName)
    {
        PrefabName = prefabName;
        NewName = newName;
        PrefabManager.Clones.Add(this);
    }

    public Clone(GameObject prefab, string newName)
    {
        PrefabName = prefab.name;
        NewName = newName;
        Source = prefab;
    }

    internal void Create()
    {
        if (Loaded) return;

        if (Source == null)
        {
            if (PrefabManager.GetPrefab(PrefabName) is not { } prefab) return;
            Source = prefab;
        }
        
        Prefab = UnityEngine.Object.Instantiate(Source, CloneManager.GetRootTransform(), false);
        Prefab.name = NewName;
        PrefabManager.RegisterPrefab(Prefab);
        OnCreated?.Invoke(Prefab);
        CloneManager.clones[Prefab.name] = Prefab;
        Loaded = true;
    }
}