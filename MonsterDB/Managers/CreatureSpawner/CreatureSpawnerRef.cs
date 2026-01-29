using System;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class CreatureSpawnerRef : Reference
{
    public string? m_creaturePrefab;
    [YamlMember(Description = "Level")]
    public int? m_maxLevel = 1;
    public int? m_minLevel = 1;
    public float? m_levelupChance = 10f;
    [YamlMember(Description = "Spawn settings", Alias = "m_respawnTimeMinutes")]
    public float? m_respawnTimeMinuts = 20f;
    public float? m_triggerDistance = 60f;
    public float? m_triggerNoise;
    public bool? m_spawnAtNight = true;
    public bool? m_spawnAtDay = true;
    public bool? m_requireSpawnArea;
    public bool? m_spawnInPlayerBase;
    public bool? m_wakeUpAnimation;
    public int? m_spawnInterval = 5;
    public string? m_requiredGlobalKey = "";
    public string? m_blockingGlobalKey = "";
    public bool? m_setPatrolSpawnPoint;
    [YamlMember(Description = "Spawn group blocking, Spawners sharing the same ID within eachothers radiuses will be grouped together, and will never spawn more than the specified max group size. Weight will also be taken into account, prioritizing those with higher weight randomly.")]
    public int? m_spawnGroupID;
    public int? m_maxGroupSpawned = 1;
    public float? m_spawnGroupRadius;
    public float? m_spawnerWeight = 1f;
    public EffectListRef? m_spawnEffects;
    
    public CreatureSpawnerRef(){}
    public CreatureSpawnerRef(CreatureSpawner component) => Setup(component);
}