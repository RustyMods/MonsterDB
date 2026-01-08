using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

public class CinderSpawnerRef : Reference
{
    public string? m_cinderPrefab;
    public float? m_cinderInterval = 2f;
    public float? m_cinderChance = 0.1f;
    public float? m_cinderVel = 5f;
    public float? m_spawnOffset = 1f;
    public Vector3Ref? m_spawnOffsetPoint;
    public int? m_spread = 4;
    public int? m_instancesPerSpawn = 1;
    public bool? m_spawnOnAwake;
    public bool? m_spawnOnProjectileHit;
    [YamlMember(Description = "Will affect all new cinderPrefab instances, including cinderPrefab from other components")]
    public CinderRef? m_cinder;

    public void Update(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out CinderSpawner cs)) return;
        cs.SetFieldsFrom(this);
        if (m_cinder != null && cs.m_cinderPrefab != null && 
            cs.m_cinderPrefab.name == m_cinder.m_prefab)
        {
            m_cinder.Update(cs.m_cinderPrefab);
        }
    }

    public static implicit operator CinderSpawnerRef(CinderSpawner cs)
    {
        CinderSpawnerRef reference = new CinderSpawnerRef();
        reference.ReferenceFrom(cs);
        if (cs.m_cinderPrefab != null && cs.m_cinderPrefab.TryGetComponent(out Cinder cinder))
        {
            reference.m_cinder = cinder;
        }
        return reference;
    }
}