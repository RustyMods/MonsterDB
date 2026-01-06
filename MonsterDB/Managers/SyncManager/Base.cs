using System;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

public enum CreatureType
{
    None, Humanoid, Character, Egg, Human, Item
}

[Serializable][UsedImplicitly]
public class Base
{
    [YamlMember(Order = 0)] public string? GameVersion;
    [YamlMember(Order = 1)] public string? ModVersion;
    [YamlMember(Order = 2)] public CreatureType Type;
    [YamlMember(Order = 3)] public string Prefab = "";
    [YamlMember(Order = 4)] public string ClonedFrom = "";
    [YamlMember(Order = 5)] public bool IsCloned;

    public virtual void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        GameVersion = Version.GetVersionString();
        ModVersion = MonsterDBPlugin.ModVersion;
    }

    public virtual void Update()
    {
        MonsterDBPlugin.LogDebug($"Updated {Prefab}");
    }
}