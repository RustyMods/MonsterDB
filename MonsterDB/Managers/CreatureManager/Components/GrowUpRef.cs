using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MonsterDB;

[Serializable]
public class GrowUpRef : Reference
{
    public float? m_growTime;
    public bool? m_inheritTame;
    public string? m_grownPrefab;
    public List<GrownEntry>? m_altGrownPrefabs;

    public static implicit operator GrowUpRef(Growup growUp)
    {
        GrowUpRef reference = new GrowUpRef();
        reference.SetFrom(growUp);
        return reference;
    }

    [Serializable]
    public class GrownEntry : Reference
    {
        public string m_prefab = "";
        [DefaultValue(1f)] public float m_weight;
    }
}


public static partial class Extensions
{
    public static List<GrowUpRef.GrownEntry> ToRef(this List<Growup.GrownEntry> ge)
    {
        List<GrowUpRef.GrownEntry> growEntries = ge
            .Where(x => x.m_prefab != null)
            .Select(x => new GrowUpRef.GrownEntry()
            {
                m_prefab = x.m_prefab.name,
                m_weight =  x.m_weight
            })
            .ToList();
        return growEntries;
    }

    public static List<Growup.GrownEntry> FromRef(this List<GrowUpRef.GrownEntry> ge)
    {
        List<Growup.GrownEntry> growEntries = ge
            .Select(x => new Growup.GrownEntry()
            {
                m_prefab = PrefabManager.GetPrefab(x.m_prefab),
                m_weight = x.m_weight
            })
            .Where(x => x.m_prefab != null)
            .ToList();
        return growEntries;
    }
}