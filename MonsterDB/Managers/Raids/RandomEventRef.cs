using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class RandomEventRef : Reference
{
    public string m_name = "";
    public bool? m_enabled;
    public bool? m_random;
    public float? m_duration;
    public bool? m_nearBaseOnly;
    public bool? m_pauseIfNoPlayerInArea;
    public float? m_eventRange;
    public float? m_standaloneInterval;
    public float? m_standaloneChance;
    public float? m_spawnerDelay;
    public Heightmap.Biome? m_biome;

    [YamlMember(Description = "( Keys required to be TRUE )")]
    public List<string>? m_requiredGlobalKeys;

    [YamlMember(Description =
        "If PlayerEvents is set, instead only spawn this event at players know who at least one of each item on each list (or player key below).")]
    public List<string>? m_altRequiredKnownItems;

    [YamlMember(Description =
        "If PlayerEvents is set, instead only spawn this event at players know who have ANY of these keys set (or any known items above).")]
    public List<string>? m_altRequiredPlayerKeysAny;

    [YamlMember(Description =
        "If PlayerEvents is set, instead only spawn this event at players know who have ALL of these keys set (or any known items above).")]
    public List<string>? m_altRequiredPlayerKeysAll;

    [YamlMember(Description = "( Keys required to be FALSE )")]
    public List<string>? m_notRequiredGlobalKeys;

    [YamlMember(Description =
        "If PlayerEvents is set, instead require ALL of the items to be unknown on this list for a player to be able to get that event. (And below not known player keys)")]
    public List<string>? m_altRequiredNotKnownItems;

    [YamlMember(Description =
        "If PlayerEvents is set, instead require ALL of the playerkeys to be unknown on this list for a player to be able to get that event. (And above not known items)")]
    public List<string>? m_altNotRequiredPlayerKeys;

    public string? m_startMessage;
    public string? m_endMessage;
    public string? m_forceMusic;
    public string? m_forceEnvironment;
    public List<SpawnDataRef>? m_spawn;
    
    public RandomEventRef(){}

    public RandomEventRef(RandomEvent randomEvent)
    {
        Setup(randomEvent);
    }
}

public static partial class Extensions
{
    public static List<SpawnDataRef> ToSpawnDataRefList(this List<SpawnSystem.SpawnData> sps)
    {
        return sps
            .Select(x => new SpawnDataRef(x))
            .ToList();
    }
}