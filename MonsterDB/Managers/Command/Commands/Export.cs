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
        _ => new List<string>()
    };

    private static List<string> GetExportTypeOptions(string word) =>
        word switch
        {
            "texture" => TextureManager.GetAllTextures().Keys.ToList(),
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
        BaseType.Human => new List<string> { "Player" },
        BaseType.All => new List<string>(),
        _ => PrefabManager.GetAllPrefabNames()
    };

    private static string GetExportDescriptions(string[] args, string defaultValue)
    {
        if (args.Length < 3) return defaultValue;

        string type = args[2];
        if (string.IsNullOrEmpty(type)) return defaultValue;
        
        switch (type)
        {
            case "texture":
                return $"<color={HEX_Gray}>[TextureID]</color>: Export texture png";
            case "sprite":
                return $"<color={HEX_Gray}>[SpriteID]</color>: Export sprite png";
            case "raid":
                return $"<color={HEX_Gray}>[EventName]</color>: Export raid YML file";
            case "bones":
                return $"<color={HEX_Gray}>[PrefabID]</color>: Export prefab hierarchy";
            default:
                return Enum.TryParse(type, true, out BaseType baseType) ? 
                    $"<color={HEX_Gray}>[PrefabID]</color>: Export {baseType} YML file" : 
                    defaultValue;
        }
    }

    private static void Export(Terminal.ConsoleEventArgs args)
    {
        string type = args.GetString(2);
        if (string.IsNullOrEmpty(type))
        {
            args.Context.LogWarning("Invalid parameters");
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

    private static void ExportBase(BaseType baseType, Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(3);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
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
                CreatureManager.Write(prefab, isClone, source);
                break;
            case BaseType.Egg:
                EggManager.Write(prefab, isClone, source);
                break;
            case BaseType.Item:
                ItemManager.Write(prefab, isClone, source);
                break;
            case BaseType.Fish:
                FishManager.Write(prefab, isClone, source);
                break;
            case BaseType.Projectile:
                ProjectileManager.Write(prefab, isClone, source);
                break;
            case BaseType.SpawnAbility:
                SpawnAbilityManager.Write(prefab, isClone, source);
                break;
            case BaseType.Visual:
                VisualManager.Write(prefab, isClone, source);
                break;
            case BaseType.CreatureSpawner:
                CreatureSpawnerManager.Write(prefab, isClone, source);
                break;
            case BaseType.SpawnArea:
                SpawnAreaManager.Write(prefab, isClone, source);
                break;
            case BaseType.All:
                FileManager.Export(FileManager.ImportFolder);
                args.Context.AddString("Exported all MonsterDB imported files into a single aggregated file.");
                break;
        }
    }
}