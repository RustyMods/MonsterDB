using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public static partial class Commands
{
    private static void Clone(Terminal.ConsoleEventArgs args)
    {
        string type = args.GetString(2);
        if (string.IsNullOrEmpty(type))
        {
            args.Context.LogWarning("Specify type");
            return;
        }
        if (!Enum.TryParse(type, true, out BaseType baseType))
        {
            args.Context.LogWarning("Invalid type");
            return;
        }
        CloneBase(baseType, args);
    }

    private static string GetCloneDescriptions(string[] args, string defaultValue)
    {
        if (args.Length < 3) return defaultValue;
        string type = args[2];
        if (string.IsNullOrEmpty(type))
        {
            return defaultValue;
        }

        if (!Enum.TryParse(type, true, out BaseType baseType)) return defaultValue;
        return $"<color={HEX_Gray}>[PrefabID][Name]</color>: Clone {baseType} prefab and write YML file";
    }

    private static List<string> GetCloneOptions(int i, string word) => i switch
    {
        2 => GetBaseTypes(BaseType.None, BaseType.All),
        3 => GetCloneTypeOptions(word),
        _ => new List<string>()
    };

    private static List<string> GetCloneTypeOptions(string word)
    {
        return Enum.TryParse(word, true, out BaseType type)
            ? GetCloneOptionByBase(type)
            : PrefabManager.GetAllPrefabNames();
    }
    
    private static List<string> GetCloneOptionByBase(BaseType type) => type switch
    {
        BaseType.Character => PrefabManager.GetAllPrefabNames<Character>(),
        BaseType.Humanoid => PrefabManager.GetAllPrefabNames<Humanoid>(),
        BaseType.Item or BaseType.Egg or BaseType.Fish => PrefabManager.GetAllPrefabNames<ItemDrop>(),
        BaseType.Projectile => PrefabManager.GetAllPrefabNames<Projectile>(),
        BaseType.SpawnAbility => PrefabManager.GetAllPrefabNames<SpawnAbility>(),
        BaseType.CreatureSpawner => PrefabManager.GetAllPrefabNames<CreatureSpawner>(),
        BaseType.Human => new List<string> { "Player" },
        BaseType.SpawnArea => PrefabManager.GetAllPrefabNames<SpawnArea>(),
        _ => PrefabManager.GetAllPrefabNames()
    };

    private static void CloneBase(BaseType baseType, Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(3);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Specify prefab");
            return;
        }
        string newName = args.GetString(4);
        if (string.IsNullOrEmpty(newName))
        {
            args.Context.LogWarning("Specify new name");
            return;
        }
        GameObject? prefab = PrefabManager.GetPrefab(prefabName);

        if (prefab == null)
        {
            args.Context.LogWarning($"Failed to find prefab: {prefabName}");
            return;
        }

        bool isClone = CloneManager.IsClone(prefab.name, out _);
        if (isClone)
        {
            args.Context.LogWarning("Already a clone");
            return;
        }

        switch (baseType)
        {
            case BaseType.Character or BaseType.Humanoid or BaseType.Human:
                CreatureManager.Clone(prefab, newName);
                break;
            case BaseType.Egg:
                EggManager.Clone(prefab, newName);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            case BaseType.Item:
                ItemManager.TryClone(prefab, newName, out _);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            case BaseType.Fish:
                FishManager.Clone(prefab, newName);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            case BaseType.Projectile:
                ProjectileManager.TryClone(prefab, newName, out _);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            case BaseType.SpawnAbility:
                SpawnAbilityManager.TryClone(prefab, newName, out _);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            case BaseType.Visual:
                VisualManager.TryClone(prefab, newName, out _);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            case BaseType.CreatureSpawner:
                CreatureSpawnerManager.Clone(prefab, newName);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            case BaseType.Ragdoll:
                RagdollManager.TryClone(prefab, newName, out _);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            case BaseType.SpawnArea:
                SpawnAreaManager.Clone(prefab, newName);
                args.Context.AddString($"Cloned {baseType} {prefab.name} as {newName}");
                break;
            default:
                args.Context.LogWarning("Invalid type");
                break;
        }
    }
}