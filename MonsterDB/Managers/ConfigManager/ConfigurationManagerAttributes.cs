using System;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace MonsterDB;

public class ConfigurationManagerAttributes
{
    [UsedImplicitly] public int? Order = null!;
    [UsedImplicitly] public bool? Browsable = null!;
    [UsedImplicitly] public string? Category = null!;
    [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
}