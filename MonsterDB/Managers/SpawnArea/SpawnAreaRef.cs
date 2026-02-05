using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class SpawnAreaRef : Reference
{
    public List<SpawnDataRef>? m_prefabs;
    [YamlMember(Alias = "m_levelUpChance")][DefaultValue(15f)] public float? m_levelupChance = 15f;
    [DefaultValue(30f)] public float? m_spawnIntervalSec = 30f;
    [DefaultValue(256f)] public float? m_triggerDistance = 256f;
    [DefaultValue(true)] public bool? m_setPatrolSpawnPoint = true;
    [DefaultValue(2f)] public float? m_spawnRadius = 2f;
    [DefaultValue(10f)] public float? m_nearRadius = 10f;
    [DefaultValue(1000f)] public float? m_farRadius = 1000f;
    [DefaultValue(3)] public int? m_maxNear = 3;
    [DefaultValue(20)] public int? m_maxTotal = 20;
    [DefaultValue(false)] public bool? m_onGroundOnly;
    public EffectListRef? m_spawnEffects;

    public SpawnAreaRef(){}
    public SpawnAreaRef(SpawnArea component) => Setup(component);
    
    [Serializable]
    public class SpawnDataRef : Reference
    {
        public string m_prefab = "";
        public float m_weight;
        public int m_maxLevel = 1;
        public int m_minLevel = 1;
        
        public SpawnDataRef(){}
        public SpawnDataRef(SpawnArea.SpawnData data) => Setup(data);

        public SpawnArea.SpawnData ToSpawnData()
        {
            if (string.IsNullOrEmpty(m_prefab)) return new SpawnArea.SpawnData();
            SpawnArea.SpawnData result = new SpawnArea.SpawnData()
            {
                m_prefab = PrefabManager.GetPrefab(m_prefab),
                m_weight = m_weight,
                m_maxLevel = m_maxLevel,
                m_minLevel = m_minLevel,
            };
            return result;
        }
    }
}

public static partial class Extensions
{
    public static List<SpawnAreaRef.SpawnDataRef> ToSpawnDataRefList(this List<SpawnArea.SpawnData> spawns)
    {
        return spawns
            .Select(s => new SpawnAreaRef.SpawnDataRef(s))
            .ToList();
    }

    public static List<SpawnArea.SpawnData> ToSpawnAreaSpawnDataList(this List<SpawnAreaRef.SpawnDataRef> spawns)
    {
        return spawns
            .Select(s => s.ToSpawnData())
            .Where(s => s.m_prefab != null)
            .ToList();
    }
}