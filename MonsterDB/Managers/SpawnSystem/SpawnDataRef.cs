using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class SpawnDataRef : Reference
{
    public string m_name = "";
    public bool m_enabled = true;
    public bool m_devDisabled;
    public string m_prefab = "";
    [YamlMember(DefaultValuesHandling = DefaultValuesHandling.Preserve)] 
    public Heightmap.Biome m_biome = Heightmap.Biome.None;
    [YamlMember(Description = "Edge, Median, Everything")] 
    public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;
    public int m_maxSpawned = 1;
    public float m_spawnInterval = 4f;
    public float m_spawnChance = 100f;
     public float m_spawnDistance = 10f;
    public float m_spawnRadiusMin;
    public float m_spawnRadiusMax;
    public string m_requiredGlobalKey = "";
    public List<string> m_requiredEnvironments = new List<string>();
     public int m_groupSizeMin = 1;
     public int m_groupSizeMax = 1;
     public float m_groupRadius = 3f;
    public bool m_spawnAtNight = true;
    public bool m_spawnAtDay = true;
     public float m_minAltitude = -1000f;
     public float m_maxAltitude = 1000f;
    public float m_minTilt;
     public float m_maxTilt = 35f;
     public bool m_inForest = true;
     public bool m_outsideForest = true;
    public bool m_inLava;
     public bool m_outsideLava = true;
    public bool m_canSpawnCloseToPlayer;
    public bool m_insidePlayerBase;
    public float m_minOceanDepth;
    public float m_maxOceanDepth;
    public bool m_huntPlayer;
     public float m_groundOffset = 0.5f;
    public float m_groundOffsetRandom;
    public float m_minDistanceFromCenter;
    public float m_maxDistanceFromCenter;
     public int m_maxLevel = 1;
     public int m_minLevel = 1;
    public float m_levelUpMinCenterDistance;
     public float m_overrideLevelupChance = -1f;
    public bool m_foldout;
    
    public SpawnDataRef(){}
    public SpawnDataRef(SpawnSystem.SpawnData data) => Setup(data);
}