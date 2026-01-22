using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class CinderRef : Reference
{
    [YamlMember(Description = "If prefab != cinderPrefab, this will not update")]
    public string m_prefab = "";
    public string? m_firePrefab;
    public string? m_houseFirePrefab;
    public float? m_gravity;
    public float? m_drag;
    public float? m_windStrength;
    public int? m_spread = 4;
    [YamlMember(Description = "0.0 - 1.0")]
    public float m_chanceToIgniteGrass;
    public EffectListRef? m_hitEffects;
    
    public CinderRef(){}

    public CinderRef(Cinder component)
    {
        Setup(component);
        m_prefab = component.name;
    }

    public void Update(GameObject prefab, bool isInstance)
    {
        if (!prefab.TryGetComponent(out Cinder cinder)) return;
        UpdateFields(cinder, prefab.name, !isInstance);
    }
}