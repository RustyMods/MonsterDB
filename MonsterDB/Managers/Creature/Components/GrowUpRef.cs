using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;

namespace MonsterDB;

[Serializable]
public class GrowUpRef : Reference
{
    public float? m_growTime;
    public bool? m_inheritTame;
    public string? m_grownPrefab;
    public List<GrownEntry>? m_altGrownPrefabs;
    
    public GrowUpRef(){}

    public GrowUpRef(Growup growUp) => Setup(growUp);

    [Serializable]
    public class GrownEntry : Reference
    {
        public string m_prefab = "";
        [DefaultValue(1f)] public float m_weight;
        
        public GrownEntry(){}

        public GrownEntry(Growup.GrownEntry entry)
        {
            m_prefab = entry.m_prefab.name;
            m_weight = entry.m_weight;
        }

        public Growup.GrownEntry ToGrowUpGrowEntry() => new Growup.GrownEntry()
        {
            m_prefab = PrefabManager.GetPrefab(m_prefab),
            m_weight = m_weight
        };
    }
}

public static partial class Extensions
{
    public static List<GrowUpRef.GrownEntry> ToGrowUpRefEntryList(this List<Growup.GrownEntry> ge)
    {
        List<GrowUpRef.GrownEntry> growEntries = ge
            .Where(x => x.m_prefab != null)
            .Select(x => new GrowUpRef.GrownEntry(x))
            .ToList();
        return growEntries;
    }

    public static List<Growup.GrownEntry> FromRef(this List<GrowUpRef.GrownEntry> ge)
    {
        List<Growup.GrownEntry> growEntries = ge
            .Select(x => x.ToGrowUpGrowEntry())
            .Where(x => x.m_prefab != null)
            .ToList();
        return growEntries;
    }
}