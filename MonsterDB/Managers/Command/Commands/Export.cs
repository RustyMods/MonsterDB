using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

public static partial class Commands
{
    private static List<string> GetBaseTypes(params BaseType[] ignoreTypes) => 
        (from BaseType type in Enum.GetValues(typeof(BaseType)) 
            where !ignoreTypes.Contains(type) 
            select type.ToString())
        .ToList();
    
    private static readonly List<string> ExportOptions =
        GetBaseTypes(BaseType.None, BaseType.Ragdoll).Union(new List<string> 
        { 
            "texture", 
            "sprite", 
            "raid",
            "bones"
        }).ToList();
    
    private static List<string> GetExportOptions(int i, string word) => i switch
    {
        2 => ExportOptions,
        3 => GetExportTypeOptions(word),
        _ => []
    };

    private static List<string> GetExportTypeOptions(string word) =>
        word switch
        {
            "texture" => TextureManager.GetAllTextures().Keys.Union(PrefabManager.GetAllPrefabNames()).ToList(),
            "sprite" => TextureManager.GetAllSprites().Keys.ToList(),
            "raid" => RaidManager.GetRaidNames().Append("all").ToList(),
            _ => Enum.TryParse(word, true, out BaseType type)
                ? GetOptionsByBase(type)
                : PrefabManager.GetAllPrefabNames()
        };

    private static List<string> GetOptionsByBase(BaseType type) => type switch
    {
        BaseType.Character => PrefabManager.GetAllPrefabNames<Character>().Append("all").ToList(),
        BaseType.Humanoid => PrefabManager.GetAllPrefabNames<Humanoid>().Append("all").ToList(),
        BaseType.Egg => PrefabManager.GetAllPrefabNames<EggGrow>().Append("all").ToList(),
        BaseType.Item => PrefabManager.GetAllPrefabNames<ItemDrop>(),
        BaseType.Fish => PrefabManager.GetAllPrefabNames<Fish>().Append("all").ToList(),
        BaseType.Projectile => PrefabManager.GetAllPrefabNames<Projectile>(),
        BaseType.SpawnAbility => PrefabManager.GetAllPrefabNames<SpawnAbility>(),
        BaseType.CreatureSpawner => PrefabManager.GetAllPrefabNames<CreatureSpawner>().Append("all")
            .ToList(),
        BaseType.SpawnArea => PrefabManager.GetAllPrefabNames<SpawnArea>().Append("all").ToList(),
        BaseType.Human => ["Player"],
        BaseType.All => [],
        BaseType.SpawnData => SpawnManager.GetCachedSpawnDataIds(),
        _ => PrefabManager.GetAllPrefabNames()
    };

    private static string GetExportDescriptions(string[] args, string defaultValue)
    {
        if (args.Length < 3) return defaultValue;

        string type = args[2];
        if (string.IsNullOrEmpty(type)) return defaultValue;

        return type switch
        {
            "texture" => $"<color={HEX_Gray}>[TextureID]</color>: Export texture png",
            "sprite" => $"<color={HEX_Gray}>[SpriteID]</color>: Export sprite png",
            "raid" => $"<color={HEX_Gray}>[EventName]</color>: Export raid YML file",
            "bones" => $"<color={HEX_Gray}>[PrefabID]</color>: Export prefab hierarchy",
            _ => Enum.TryParse(type, true, out BaseType baseType)
                ? baseType == BaseType.All ? $"<color={HEX_Gray}>Export Aggregate YML file of all imported YML files</color>" : $"<color={HEX_Gray}>[PrefabID]</color>: Export {baseType} YML file"
                : defaultValue
        };
    }

    private static void Export(Terminal.ConsoleEventArgs args)
    {
        string type = args.GetString(2);
        if (string.IsNullOrEmpty(type))
        {
            args.Context.LogWarning("Specify type");
            return;
        }

        if (!Enum.TryParse(type, true, out BaseType baseType))
        {
            switch (type)
            {
                case "texture":
                    TextureManager.ExportTextureOrPrefabTextures(args.Context, args.GetStringFrom(3));
                    break;
                case "sprite":
                    TextureManager.ExportSprite(args.Context, args.GetStringFrom(3));
                    break;
                case "raid":
                    RaidManager.WriteRaidYML(args.Context, args.GetString(3));
                    break;
                case "bones":
                    CreatureManager.WriteHierarchy(args.Context, args.GetString(3));
                    break;
            }
        }
        else
        {
            ExportBase(baseType, args);
        }
    }

    private static void ExportAllCharacters(Terminal? context = null)
    {
        List<GameObject> prefabs = PrefabManager.GetAllPrefabs<Character>();
        for (int i = 0; i < prefabs.Count; ++i)
        {
            GameObject prefab = prefabs[i];
            bool isClone = CloneManager.IsClone(prefab.name, out string source);
            if (!CreatureManager.Write(prefab, isClone, source, context))
            {
                context?.LogError($"Failed to export character {prefab.name}");
            }
        }
    }

    private static void ExportAllEggs(Terminal? context = null)
    {
        var prefabs = PrefabManager.GetAllPrefabs<EggGrow>();
        for (int i = 0; i < prefabs.Count; ++i)
        {
            var prefab = prefabs[i];
            bool isClone = CloneManager.IsClone(prefab.name, out string source);
            if (!EggManager.Write(prefab, isClone, source, context))
            {
                context?.LogWarning($"Failed to export egg {prefab.name}");
            }
        }
    }

    private static void ExportAllItems(Terminal? context = null)
    {
        var prefabs = PrefabManager.GetAllPrefabs<ItemDrop>();
        for (int i = 0; i < prefabs.Count; ++i)
        {
            var prefab = prefabs[i];
            bool isClone = CloneManager.IsClone(prefab.name, out string source);
            if (!ItemManager.Write(prefab, isClone, source, context: context))
            {
                context?.LogWarning($"Failed to export item {prefab.name}");
            }
        }
    }

    private static void ExportAllFish(Terminal? context = null)
    {
        var prefabs = PrefabManager.GetAllPrefabs<Fish>();
        for (int i = 0; i < prefabs.Count; ++i)
        {
            var prefab = prefabs[i];
            bool isClone = CloneManager.IsClone(prefab.name, out string source);
            if (!FishManager.Write(prefab, isClone, source, context))
            {
                context?.LogWarning($"Failed to export fish {prefab.name}");
            }
        }
    }

    private static void ExportBase(BaseType baseType, Terminal.ConsoleEventArgs args)
    {
        if (baseType == BaseType.All)
        {
            FileManager.Export(FileManager.ImportFolder);
            args.Context.LogInfo("Exported all MonsterDB imported files into a single aggregated file.");
            return;
        }
        
        string prefabName = args.GetString(3);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Specify prefab");
            return;
        }

        if (baseType == BaseType.SpawnData)
        {
            prefabName = args.GetStringFrom(3);
            if (SpawnManager.Export(prefabName, out string filepath))
            {
                args.Context.LogInfo($"Exported {prefabName} Spawn Data");
                args.Context.LogInfo(filepath.RemoveRootPath());
            }
            else
            {
                args.Context.LogError($"Failed to export {prefabName} spawn data");
            }
            return;
        }

        if (prefabName == "all")
        {
            switch (baseType)
            {
                case BaseType.Character or BaseType.Human or BaseType.Humanoid:
                    ExportAllCharacters(args.Context);
                    return;
                case BaseType.Egg:
                    ExportAllEggs(args.Context);
                    return;
                case BaseType.Item:
                    ExportAllItems(args.Context);
                    return;
                case BaseType.Fish:
                    ExportAllFish(args.Context);
                    return;
                default:
                    args.Context.LogWarning($"Export {baseType} all not supported");
                    return;
            }
        }
                
        GameObject? prefab = PrefabManager.GetPrefab(prefabName);

        if (prefab == null)
        {
            args.Context.LogWarning($"Failed to find prefab: {prefabName}");
            return;
        }

        bool isClone = CloneManager.IsClone(prefab.name, out string source);
        
        switch (baseType)
        {
            case BaseType.Character or BaseType.Human or BaseType.Humanoid:
                if (!CreatureManager.Write(prefab, isClone, source, args.Context))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
            case BaseType.Egg:
                if (!EggManager.Write(prefab, isClone, source, args.Context))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
            case BaseType.Item:
                if (!ItemManager.Write(prefab, isClone, source, context: args.Context))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
            case BaseType.Fish:
                if (!FishManager.Write(prefab, isClone, source, args.Context))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
            case BaseType.Projectile:
                if (!ProjectileManager.Write(prefab, isClone, source))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
            case BaseType.SpawnAbility:
                if (!SpawnAbilityManager.Write(prefab, isClone, source, context: args.Context))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
            case BaseType.Visual:
                if (!VisualManager.Write(prefab, isClone, source, context: args.Context))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
            case BaseType.CreatureSpawner:
                if (!CreatureSpawnerManager.Write(prefab, isClone, source, args.Context))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
            case BaseType.SpawnArea:
                if (!SpawnAreaManager.Write(prefab, isClone, source, context: args.Context))
                {
                    args.Context.LogError($"Failed to export {baseType} {prefab.name}");
                }
                break;
        }
    }
}