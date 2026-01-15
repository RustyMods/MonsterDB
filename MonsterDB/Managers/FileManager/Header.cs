using System;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

public enum BaseType
{
    None, Humanoid, Character, Egg, Human, Item, Fish, Projectile
}

[Serializable][UsedImplicitly]
public class Header
{
    [YamlMember(Order = 0)] public string? GameVersion;
    [YamlMember(Order = 1)] public string? ModVersion;
    [YamlMember(Order = 2)] public BaseType Type;
    [YamlMember(Order = 3)] public string Prefab = "";
    [YamlMember(Order = 4)] public string ClonedFrom = "";
    [YamlMember(Order = 5)] public bool IsCloned;
    
    public virtual void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        SetupVersions();
    }

    protected void SetupVersions()
    {
        GameVersion = Version.GetVersionString();
        ModVersion = MonsterDBPlugin.ModVersion;
    }

    public virtual void Update()
    {
        MonsterDBPlugin.LogDebug($"Updated {Prefab}");
    }
}