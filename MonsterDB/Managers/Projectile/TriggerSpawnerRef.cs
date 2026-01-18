namespace MonsterDB;

public class TriggerSpawnerRef
{
    public string[]? m_creaturePrefabs;
    public int? m_maxLevel = 1;
    public int? m_minLevel = 1;
    public float? m_levelupChance = 10f;
    public float? m_spawnChance = 100f;
    public float? m_minSpawnInterval = 10f;
    public int? m_maxSpawned = 10;
    public float? m_maxExtraPerPlayer;
    public float? m_maxSpawnedRange = 30f;
    public bool? m_setHuntPlayer;
    public bool? m_setPatrolSpawnPoint;
    public bool? m_useSpawnerRotation;
}