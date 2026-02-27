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
    
    public CharacterDropRef(){}

    public CharacterDropRef(CharacterDrop characterDrop) => Setup(characterDrop);
}

[Serializable]
public class DropRef : Reference
{
    public string? m_prefab;
    public int? m_amountMin;
    public int? m_amountMax;
    public float? m_chance;
    public bool? m_onePerPlayer;
    public bool? m_levelMultiplier;
    public bool? m_dontScale;
    
    public DropRef(){}

    public DropRef(CharacterDrop.Drop drop)
    {
        m_prefab = drop.m_prefab.name;
        m_chance = drop.m_chance;
        m_amountMin =  drop.m_amountMin;
        m_amountMax =  drop.m_amountMax;
        m_dontScale =  drop.m_dontScale;
        m_levelMultiplier = drop.m_levelMultiplier;
        m_onePerPlayer = drop.m_onePerPlayer;
    }
}

public static partial class Extensions
{
    public static List<DropRef> ToRef(this List<CharacterDrop.Drop> cd)
    {
        List<DropRef> drops = cd
            .Where(x => x.m_prefab != null)
            .Select(x => new DropRef(x))
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