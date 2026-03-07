using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MonsterDB;

public static class RRRConverter
{
    private const string FolderName = "RRR";
    private static readonly string FolderPath;
    private static readonly Dictionary<string, RRR_Main> files;
    private static readonly Dictionary<string, TextureData> textures;
    private static readonly DropDict drops;

    private static readonly MethodInfo? JsonDeserializeMethod;
    
    static RRRConverter()
    {
        FolderPath = Path.Combine(ConfigManager.DirectoryPath, FolderName);
        files = new Dictionary<string, RRR_Main>();
        textures = new Dictionary<string, TextureData>();
        drops = new DropDict();

        JsonDeserializeMethod = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Newtonsoft.Json")
            ?.GetType("Newtonsoft.Json.JsonConvert")
            ?.GetMethod("DeserializeObject", [typeof(string), typeof(Type)]);
    }

    private static T? DeserializeObject<T>(string json) where T : class
    {
        return JsonDeserializeMethod?.Invoke(null, [json, typeof(T)]) as T;
    }
    
    public static void Read()
    {
        if (!Directory.Exists(FolderPath)) return;
        string[] paths = Directory.GetFiles(FolderPath, "*.json", SearchOption.AllDirectories);
        for (int i = 0; i < paths.Length; ++i)
        {
            string path = paths[i];
            try
            {
                string text = File.ReadAllText(path);
                RRR_Main? data = DeserializeObject<RRR_Main>(text);
                
                if (data == null) continue;
                files[path] = data;
            }
            catch (Exception ex)
            {
                MonsterDBPlugin.LogWarning(ex.Message + $" file: {Path.GetFileName(path)}");
            }
        }
        string[] texturesPath = Directory.GetFiles(FolderPath, "*.png", SearchOption.AllDirectories);
        for (int i = 0; i < texturesPath.Length; ++i)
        {
            string path = texturesPath[i];
            string? filename = Path.GetFileName(path);
            TextureData data = new(path);
            textures[filename] = data;
        }

        string[] cfgPaths = Directory.GetFiles(FolderPath, "*.cfg", SearchOption.AllDirectories);
        for (int i = 0; i < cfgPaths.Length; ++i)
        {
            string cfg = cfgPaths[i];
            string? filename = Path.GetFileNameWithoutExtension(cfg);
            if (filename.StartsWith("drop_that"))
            {
                DropThat_Parser parser = new DropThat_Parser();
                parser.Parse(cfg);
                drops.Add(parser.drops);
            }
        }
    }

    public static void ConvertAll(Terminal.ConsoleEventArgs args)
    {
        int success = 0;
        int failures = 0;
        foreach (KeyValuePair<string, RRR_Main> kvp in files)
        {
            if (Convert(kvp.Key, kvp.Value))
            {
                ++success;
            }
            else
            {
                ++failures;
            }
        }
        args.Context.Log("#FFCCCB", $"> Converted {success} RRR files ( failures: {failures} )");
    }

    private static bool Convert(string filepath, RRR_Main main)
    {
        string filename = Path.GetFileName(filepath);
        string foldername = Path.GetFileNameWithoutExtension(filename);
        List<BaseItem> items = new();
        List<string> textureNames = new();
        if (main.sOriginalPrefabName == null || string.IsNullOrEmpty(main.sOriginalPrefabName))
        {
            MonsterDBPlugin.LogWarning($"Failed RRR Conversion: {filename}");
            return false;
        }
        GameObject? prefab = PrefabManager.GetPrefab(main.sOriginalPrefabName);
        if (prefab == null)
        {
            MonsterDBPlugin.LogWarning($"Failed RRR Conversion: {filename}");
            return false;
        }

        if (!CreatureManager.TrySave(prefab, out Base data))
        {
            MonsterDBPlugin.LogWarning($"Failed RRR Conversion: {filename}");
            return false;
        }
        
        if (main.sNewPrefabName != null && !string.IsNullOrEmpty(main.sNewPrefabName))
        {
            data.ClonedFrom = prefab.name;
            data.Prefab = main.sNewPrefabName;
            data.IsCloned = true;
        }

        BaseRagdoll? ragdoll = null;

        if (main.Category_Appearance != null && data.Visuals != null)
        {
            main.Category_Appearance.Setup(data.Visuals);

            if (main.Category_Appearance.sCustomTexture != null &&
                !string.IsNullOrEmpty(main.Category_Appearance.sCustomTexture))
            {
                SkinnedMeshRenderer? skin = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skin != null)
                {
                    string textureName = "rrrtex." + main.Category_Appearance.sCustomTexture;
                    RendererRef? skinRenderer = data.Visuals.m_renderers?.FirstOrDefault(r => r.m_prefab == skin.name);
                    if (skinRenderer is { m_materials: not null })
                    {
                        foreach (MaterialRef mat in skinRenderer.m_materials)
                        {
                            mat.m_mainTexture = textureName;
                        }
                        textureNames.Add(textureName);
                    }
                    
                    if (RagdollManager.TryGetRagdoll(prefab, out GameObject ragdollPrefab))
                    {
                        ragdoll = new BaseRagdoll();
                        ragdoll.Setup(ragdollPrefab, data.IsCloned, data.IsCloned ? ragdollPrefab.name : "");
                        if (data.IsCloned)
                        {
                            ragdoll.Prefab = data.Prefab + "_ragdoll";
                        }
                        if (ragdoll.Visuals is { m_renderers.Length: > 0 })
                        {
                            var ragSkin = ragdollPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
                            if (ragSkin != null)
                            {
                                var ragSkinRenderer = ragdoll.Visuals.m_renderers.FirstOrDefault(r => r.m_prefab == ragSkin.name);
                                if (ragSkinRenderer is { m_materials: not null })
                                {
                                    foreach (MaterialRef mat in ragSkinRenderer.m_materials)
                                    {
                                        mat.m_mainTexture = textureName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (main.Category_Character != null)
        {
            if (data is BaseCharacter { Character: not null } character)
            {
                main.Category_Character.Setup(character.Character);
            }
            else if (data is BaseHumanoid { Character: not null } humanoid)
            {
                main.Category_Character.Setup(humanoid.Character);
            }
            else if (data is BaseHuman { Character: not null } human)
            {
                main.Category_Character.Setup(human.Character);
            }
        }

        if (main.Category_Humanoid != null)
        {
            if (main.sNewPrefabName != null && main.Category_Humanoid.aAdvancedCustomAttacks != null)
            {
                List<string> newItemNames = [];
                for (int i = 0; i < main.Category_Humanoid.aAdvancedCustomAttacks.Count; i++)
                {
                    RRR_CustomAttacks attack = main.Category_Humanoid.aAdvancedCustomAttacks[i];
                    if (attack.Convert(main.sNewPrefabName, out BaseItem item))
                    {
                        newItemNames.Add(item.Prefab);
                        items.Add(item);
                    }
                }

                if (newItemNames.Count > 0)
                {
                    main.Category_Humanoid.aDefaultItems = newItemNames;
                }
            }
            
            
            if (data is BaseHumanoid { Character: not null } humanoid)
            {
                main.Category_Humanoid.Setup(humanoid.Character);
            }
            else if (data is BaseHuman { Character: not null } human)
            {
                main.Category_Humanoid.Setup(human.Character);
            }
        }

        if (main.Category_BaseAI != null)
        {
            if (data is BaseCharacter { AI: not null } character)
            {
                main.Category_BaseAI.Setup(character.AI);
            }
            else if (data is BaseHumanoid { AI: not null } humanoid)
            {
                main.Category_BaseAI.Setup(humanoid.AI);
            }
            else if (data is BaseHuman { AI: not null } human)
            {
                main.Category_BaseAI.Setup(human.AI);
            }
        }

        if (main.Category_MonsterAI != null)
        {
            if (data is BaseHumanoid { AI: not null } humanoid)
            {
                main.Category_MonsterAI.Setup(humanoid.AI);
            }
            else if (data is BaseHuman { AI: not null } human)
            {
                main.Category_MonsterAI.Setup(human.AI);
            }
        }

        if (main.Category_CharacterDrops != null && data.Drops != null)
        {
            main.Category_CharacterDrops.Setup(data.Drops);
        }
        
        if (main.sNewPrefabName != null)
        {
            List<DropRef> dropThatDrops = drops.ToDropRefs(main.sNewPrefabName);
            if (dropThatDrops.Count > 0)
            {
                data.Drops ??= new CharacterDropRef();
                data.Drops.m_drops = new List<DropRef>();
                data.Drops.m_drops.AddRange(dropThatDrops);
            }
        }

        if (main.Category_Special != null)
        {
            if (data.Tameable != null) main.Category_Special.Setup(data.Tameable);
        }

        if (main.Category_NpcOnly != null && data.Visuals != null)
        {
            main.Category_NpcOnly.Setup(data.Visuals);
        }

        string exportFolderPath = Path.Combine(FolderPath, "Export");
        if (!Directory.Exists(exportFolderPath)) Directory.CreateDirectory(exportFolderPath);
        
        string folderPath = Path.Combine(exportFolderPath, foldername);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string itemFolderPath = Path.Combine(folderPath, "items");
        if (!Directory.Exists(itemFolderPath)) Directory.CreateDirectory(itemFolderPath);

        string textureFolderPath = Path.Combine(folderPath, "textures");
        if (!Directory.Exists(textureFolderPath)) Directory.CreateDirectory(textureFolderPath);
        
        foreach (BaseItem? item in items)
        {
            string itemFilename = item.Prefab + ".yml";
            string itemFilepath = Path.Combine(itemFolderPath, itemFilename);
            string content = ConfigManager.Serialize(item);
            File.WriteAllText(itemFilepath, content);
        }

        foreach (string? textureName in textureNames)
        {
            string textureFilename = textureName + ".png";
            if (textures.TryGetValue(textureFilename, out TextureData pkg))
            {
                string textureFilepath = Path.Combine(textureFolderPath, textureFilename);
                pkg.Write(textureFilepath);
            }
        }
        
        string text = ConfigManager.Serialize(data);
        filename = main.sNewPrefabName + ".yml";
        filepath = Path.Combine(folderPath, filename);
        File.WriteAllText(filepath, text);

        if (ragdoll != null)
        {
            text = ConfigManager.Serialize(ragdoll);
            filename = main.sNewPrefabName + "_ragdoll.yml";
            filepath = Path.Combine(folderPath, filename);
            File.WriteAllText(filepath, text);
        }
        
        MonsterDBPlugin.LogInfo($"Converted RRR file: {filename}");

        return true;
    }
}