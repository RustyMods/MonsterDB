using System;
using System.Collections.Generic;
using UnityEngine.Internal;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class SpawnAbilityRef : Reference
{
    public string[]? m_spawnPrefab;
    public string? m_maxSummonReached = "$hud_maxsummonsreached";
    public bool? m_spawnOnAwake;
    public bool? m_alertSpawnedCreature = true;
    public bool? m_passiveAggressive;
    public bool? m_spawnAtTarget = true;
    public int? m_minToSpawn = 1;
    public int? m_maxToSpawn = 1;
    public int? m_maxSpawned = 3;
    public float? m_spawnRadius = 3f;
    public bool? m_circleSpawn;
    public bool? m_snapToTerrain = true;
    [YamlMember(Description = "Used to give random Y rotations to things like AOEs that aren't circular")]
    public bool? m_randomYRotation;
    public float? m_spawnGroundOffset;
    public int? m_getSolidHeightMargin = 1000;
    public float? m_initialSpawnDelay;
    public float? m_spawnDelay;
    public float? m_preSpawnDelay;
    public bool? m_commandOnSpawn;
    public bool? m_wakeUpAnimation;
    public Skills.SkillType? m_copySkill;
    public float? m_copySkillToRandomFactor;
    public bool? m_setMaxInstancesFromWeaponLevel;
    public List<SpawnAbility.LevelUpSettings>? m_levelUpSettings;
    public SpawnAbility.TargetType? m_targetType;
    public Pathfinding.AgentType? m_targetWhenPathfindingType = Pathfinding.AgentType.Humanoid;
    public float? m_maxTargetRange = 40f;
    public EffectListRef? m_spawnEffects;
    public EffectListRef? m_preSpawnEffects;
    [YamlMember(Description = "Used for the troll summoning staff, to spawn an AOE that's friendly to the spawned creature.")]
    public string? m_aoePrefab;
    [YamlMember(Description = "Projectile")]
    public float? m_projectileVelocity = 10f;
    public float? m_projectileVelocityMax;
    public float? m_projectileAccuracy = 10f;
    public bool? m_randomDirection;
    public float? m_randomAngleMin;
    public float? m_randomAngleMax;

    public static implicit operator SpawnAbilityRef(SpawnAbility spawnAbility)
    {
        var reference = new SpawnAbilityRef();
        reference.Setup(spawnAbility);
        return reference;
    }
}