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
        _ => []
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
        BaseType.Human => ["Player"],
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

        bool isClone = CloneManager.IsClone(prefab.name, out string source);
        if (isClone)
        {
            args.Context.LogWarning($"Already a clone of {source}");
            return;
        }

        switch (baseType)
        {
            case BaseType.Character or BaseType.Humanoid or BaseType.Human:
                CreatureManager.Clone(prefab, newName, context: args.Context);
                break;
            case BaseType.Egg:
                if (!EggManager.TryClone(prefab, newName))
                {
                    args.Context.LogWarning($"Failed to clone egg {prefabName}");   
                }
                break;
            case BaseType.Item:
                if (!ItemManager.TryClone(prefab, newName, out _, context: args.Context))
                {
                    args.Context.LogWarning($"Failed to clone item {prefabName}");
                }
                break;
            case BaseType.Fish:
                if (!FishManager.TryClone(prefab, newName, context: args.Context))
                {
                    args.Context.LogWarning($"Failed to clone fish {prefabName}");   
                }
                break;
            case BaseType.Projectile:
                if (!ProjectileManager.TryClone(prefab, newName, out _, context: args.Context))
                {
                    args.Context.LogWarning($"Failed to clone projectile {prefabName}");
                }
                break;
            case BaseType.SpawnAbility:
                if (!SpawnAbilityManager.TryClone(prefab, newName, out _, context: args.Context))
                {
                    args.Context.LogWarning($"Failed to clone spawn ability {prefabName}");
                }
                break;
            case BaseType.Visual:
                if (!VisualManager.TryClone(prefab, newName, out _, context: args.Context))
                {
                    args.Context.LogWarning($"Failed to clone prefab {prefabName}");
                }
                break;
            case BaseType.CreatureSpawner:
                if (!CreatureSpawnerManager.TryClone(prefab, newName, context: args.Context))
                {
                    args.Context.LogWarning($"Failed to clone creature spawner {prefabName}");
                }
                break;
            case BaseType.Ragdoll:
                if (!RagdollManager.TryClone(prefab, newName, out _, context: args.Context))
                {
                    args.Context.LogWarning($"Failed to clone ragdoll {prefabName}");
                }
                break;
            case BaseType.SpawnArea:
                if (!SpawnAreaManager.TryClone(prefab, newName, context: args.Context))
                {
                    args.Context.LogWarning($"Failed to clone spawn area {prefab.name}");
                }
                break;
            default:
                args.Context.LogWarning("Invalid type");
                break;
        }
    }
}