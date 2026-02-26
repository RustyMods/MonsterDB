using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

public static partial class Commands
{
    private const string HEX_Gray = "#B2BEB5";
    
    public static void Init()
    {
        _ = new Command("export",
            $"<color={HEX_Gray}>[Type][ID]</color>: Export YML file",
            Export,
            GetExportOptions,
            descriptions: GetExportDescriptions);

        _ = new Command("reload", 
            "Reloads all files in import folder", 
            LoadManager.ReloadAll, 
            adminOnly: true);
        
        _ = new Command("convert",
            $"<color={HEX_Gray}>[Name]</color>: Convert specific or all legacy file into v.0.2.x format",
            Convert,
            GetConvertOptions,
            descriptions: GetConvertDescription);

        _ = new Command("update",
            $"<color={HEX_Gray}>[FileName]</color>: Update YML file",
            Update,
            GetUpdateOptions,
            adminOnly: true);

        _ = new Command("reset",
            $"<color={HEX_Gray}>[Type][Name]</color>: Reset prefab to factory settings",
            Revert,
            GetRevertOptions,
            adminOnly: true);

        _ = new Command("clone",
            $"<color={HEX_Gray}>[Type][ID][Name]</color>: Clone prefab and export YML file",
            Clone,
            GetCloneOptions,
            descriptions: GetCloneDescriptions,
            adminOnly: true);

        _ = new Command("search",
            $"<color={HEX_Gray}>[Type][ID]</color>: Search item, texture or shader by name",
            Search,
            GetSearchOptions,
            descriptions: GetSearchDescriptions);
        
        _ = new Command("create",
            $"<color={HEX_Gray}>[ID]</color>: create item from non-item prefabs",
            ItemManager.CreateItem,
            GetZNetPrefabNames,
            adminOnly: true);
        
        _ = new Command("snapshot",
            $"<color={HEX_Gray}>[ID][lightIntensity?][x?][y?][z?][w?]</color>: Generates and exports a prefab icon",
            Snapshot.Run,
            GetAllPrefabOptions);
        
        _ = new Command("print_shader_properties",
            "[shaderName]",
            ShaderRef.PrintShaderProperties,
            ShaderRef.GetShaderOptions,
            isSecret: true);
        
#pragma warning disable CS0612 // Type or member is obsolete
        InitOldCommands();
#pragma warning restore CS0612 // Type or member is obsolete
    }

    [Obsolete]
    private static void InitOldCommands()
    {
        // obsolete
        _ = new Command("write",
            $"<color={HEX_Gray}>[PrefabID]</color>: save creature YML to {FileManager.ExportFolderName} folder",
            CreatureManager.WriteCreatureYML,
            CreatureManager.GetCreatureOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_all",
            $"save all creatures YML to {FileManager.ExportFolderName} folder",
            CreatureManager.WriteAllCreatureYML,
            isSecret: true);
        
        // obsolete
        _ = new Command("mod", 
            $"<color={HEX_Gray}>[FileName]</color>: read YML file from {FileManager.ImportFolderName} and update",
            CreatureManager.ReadCreatureYML,
            GetYMLFileOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("revert",
            "[prefabName]: revert creature to factory settings",
            CreatureManager.ResetCreature,
            GetOriginalCreatureOptions,
            adminOnly: true,
            isSecret: true);

        // obsolete
        _ = new Command("clone_character",
            "[prefabName][newName]: must be a character",
            CreatureManager.CloneCreature,
            CreatureManager.GetCreatureOptions,
            adminOnly: true,
            isSecret: true);

        // obsolete
        _ = new Command("export_bones", 
            "[prefabName]: export prefab hierarchy",
            CreatureManager.WriteHierarchy,
            GetAllPrefabOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_all_creature_spawners",
            "Export all creature spawner prefab YML files",
            CreatureSpawnerManager.WriteAllCreatureSpawners,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_creature_spawner",
            "Export specific creature spawner prefab YML file",
            CreatureSpawnerManager.WriteCreatureSpawner,
            CreatureSpawnerManager.GetCreatureSpawnersOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_egg",
            $"[prefabName]: save egg reference to {FileManager.ExportFolderName} folder",
            EggManager.WriteEggYML,
            EggManager.GetEggOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_all_egg",
            $"save all egg references to {FileManager.ExportFolderName} folder",
            EggManager.WriteAllEggYML,
            isSecret: true);
        
        // obsolete
        _ = new Command("mod_egg",
            $"[fileName]: read egg reference from {FileManager.ImportFolderName} folder",
            EggManager.ReadEgg,
            GetYMLFileOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("revert_egg", 
            "[prefabName]: revert egg to factory settings", 
            EggManager.ResetEgg,
            EggManager.GetResetOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("clone_egg",
            "[prefabName][newName]: must be an item",
            EggManager.CloneEgg,
            GetItemOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_fish", 
            $"[prefabName]: write fish YML to {FileManager.ExportFolderName} folder",
            FishManager.WriteFishYML,
            FishManager.GetFishOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_all_fish",
            $"write all fish YML to {FileManager.ExportFolderName} folder",
            FishManager.WriteAllFishYML,
            isSecret: true);
        
        // obsolete
        _ = new Command("mod_fish",
            $"[fileName]: read fish reference from {FileManager.ImportFolderName} folder",
            FishManager.UpdateFishYML,
            GetYMLFileOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("revert_fish",
            "[prefabName]: revert fish to factory settings",
            FishManager.ResetFish,
            FishManager.GetResetFishOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("clone_fish", 
            "[prefabName][newName]: must be a fish", 
            FishManager.CloneFish,
            FishManager.GetFishOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_item",
            $"[prefabName]: write item YML to {FileManager.ExportFolderName} folder",
            ItemManager.WriteItemYML,
            GetItemOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("mod_item", 
            $"[fileName]: read item YML from {FileManager.ImportFolderName} folder", 
            ItemManager.UpdateItemYML,
            GetYMLFileOptions, 
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("revert_item",
            "[prefabName]: revert item to factory settings",
            ItemManager.ResetItem,
            ItemManager.GetResetItemOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("clone_item",
            "[prefabName][newName]: must be an item",
            ItemManager.CloneItem,
            GetItemOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("search_item",
            "search item by name",
            ItemManager.SearchItem,
            ItemManager.GetSearchItemOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_projectile",
            "[prefabName]",
            ProjectileManager.WriteProjectileYML,
            ProjectileManager.GetProjectileOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("clone_projectile",
            "[prefabName][newName]: clones projectile and writes YML file",
            ProjectileManager.CloneProjectile,
            ProjectileManager.GetProjectileOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_all_raids",
            "Export all raid events as YML files",
            RaidManager.WriteAllRaidYML,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_raid",
            "Export specific raid event as YML file",
            RaidManager.WriteRaidYML,
            RaidManager.GetRaidOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("update_raids",
            $"Read all raid files from {RaidManager.FolderName} and update Random Event System",
            RaidManager.UpdateRaid,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_spawnability",
            "[prefabName]: write spawn ability YML file",
            SpawnAbilityManager.WriteSpawnAbilityYML,
            SpawnAbilityManager.GetSpawnAbilityOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("clone_spawnability",
            "[prefabName]: clone spawn ability YML file",
            SpawnAbilityManager.CloneSpawnAbility,
            SpawnAbilityManager.GetSpawnAbilityOptions,
            adminOnly: true,
            isSecret: true);
        
        // obsolete
        _ = new Command("export_main_tex",
            "[prefabName]: export creature main texture",
            TextureManager.ExportMainTextures,
            CreatureManager.GetCreatureOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("export_tex",
            "[textureName]: export texture as png",
            TextureManager.ExportTexture,
            TextureManager.GetTextureOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("search_tex",
            "search texture names",
            TextureManager.SearchTextures,
            TextureManager.GetTextureOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("search_sprite",
            "search sprite names",
            TextureManager.SearchSprites,
            TextureManager.GetSpriteOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("export_sprite",
            "[spriteName]: export sprite texture as png",
            TextureManager.ExportSprite,
            TextureManager.GetSpriteOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("search_shader",
            "[query]: returns list of shader names that contains query",
            ShaderRef.SearchShaders,
            ShaderRef.GetShaderOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("write_visual",
            "[prefabName]: write prefab visual data YML file",
            VisualManager.WriteVisualYML,
            GetAllPrefabOptions,
            isSecret: true);
        
        // obsolete
        _ = new Command("clone_visual",
            "[prefabName]: clone prefab and write visual YML file",
            VisualManager.CloneVisual,
            GetAllPrefabOptions,
            adminOnly: true,
            isSecret: true);
    }

    private static List<string> GetZNetPrefabNames(int i, string word) => i switch
    {
        2 => PrefabManager.GetAllPrefabNames<ZNetView>(),
        _ => new List<string>()
    };

    private static List<string> GetOriginalCreatureOptions(int i, string word) => i switch
    {
        2 => LoadManager.GetOriginalKeys<Base>(),
        _ => new List<string>()
    };

    private static List<string> GetAllPrefabOptions(int i, string word) => i switch
    {
        2 => PrefabManager.GetAllPrefabNames(),
        _ => new List<string>()
    };

    private static List<string> GetItemOptions(int i, string word) => i switch
    {
        2 => PrefabManager.GetAllPrefabNames<ItemDrop>(),
        _ => new List<string>()
    };

}