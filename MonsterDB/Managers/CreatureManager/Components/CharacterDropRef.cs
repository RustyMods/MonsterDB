using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class CharacterDropRef : Reference
{
    public List<DropRef>? m_drops;
    [DefaultValue(true)][YamlMember(Description = "Only works if Character Death Effects do not have a ragdoll")] public bool? m_dropsEnabled;

    public static implicit operator CharacterDropRef(CharacterDrop characterDrop)
    {
        CharacterDropRef reference = new CharacterDropRef();
        reference.Setup(characterDrop);
        return reference;
    }
}

[Serializable]
public class DropRef : Reference
{
    public string? m_prefab;
    [DefaultValue(1)] public int? m_amountMin;
    [DefaultValue(1)] public int? m_amountMax;
    [DefaultValue(1f)] public float? m_chance;
    [DefaultValue(false)] public bool? m_onePerPlayer;
    [DefaultValue(true)] public bool? m_levelMultiplier;
    [DefaultValue(false)] public bool? m_dontScale;
}

public static partial class Extensions
{
    public static List<DropRef> ToRef(this List<CharacterDrop.Drop> cd)
    {
        List<DropRef> drops = cd
            .Where(x => x.m_prefab != null)
            .Select(x => new DropRef()
            {
                m_prefab = x.m_prefab.name,
                m_chance = x.m_chance,
                m_amountMin =  x.m_amountMin,
                m_amountMax =  x.m_amountMax,
                m_dontScale =  x.m_dontScale,
                m_levelMultiplier = x.m_levelMultiplier,
                m_onePerPlayer = x.m_onePerPlayer,
            })
            .ToList();
        return drops;
    }

    public static List<CharacterDrop.Drop> FromRef(this List<DropRef> dr)
    {
        List<CharacterDrop.Drop> drops = new();
        foreach (DropRef? dropRef in dr)
        {
            CharacterDrop.Drop drop = new CharacterDrop.Drop();
            dropRef.UpdateFields(drop, dropRef.m_prefab ?? "Null", log: false);
            drops.Add(drop);
        }

        return drops;
    }
}