using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public static class TextureManager
{
    private static readonly Dictionary<string, Texture2D> m_cachedTextures;
    private static readonly Dictionary<string, Sprite> m_cachedSprites;
    private static readonly Dictionary<string, TextureData> m_customs;

    static TextureManager()
    {
        m_cachedSprites =  new Dictionary<string, Sprite>();
        m_cachedTextures = new Dictionary<string, Texture2D>();
        m_customs = new Dictionary<string, TextureData>();
        
        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(ZoneSystem), nameof(ZoneSystem.Awake)),
            new HarmonyMethod(AccessTools.Method(typeof(TextureManager), nameof(WriteAll))));
    }

    public static void ExportTextureOrPrefabTextures(Terminal context, string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            context.LogWarning("Specify name");
            return;
        }

        if (GetAllTextures().TryGetValue(input, out Texture2D? texture))
        {
            Export(texture, FileManager.ExportFolder);
            context.AddString("Exported texture: " + texture.name);
        }
        else
        {
            GameObject? prefab = PrefabManager.GetPrefab(input);
            if (prefab == null)
            {
                context.LogWarning($"Failed to find prefab: {input}");
                return;
            }

            HashSet<Material> materials = prefab.GetAllMaterials();

            int count = 0;
            foreach (Material? material in materials)
            {
                if (Save(material, FileManager.ExportFolder)) ++count;
            }
            context.AddString($"Exported prefab: {prefab.name} textures (total: {count})");
        }
    }
    
    [Obsolete]
    public static void ExportMainTextures(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetStringFrom(2);
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

        HashSet<Material> materials = prefab.GetAllMaterials();

        foreach (Material? material in materials)
        {
            Save(material, FileManager.ExportFolder);
        }
    }

    [Obsolete]
    public static void ExportTexture(Terminal.ConsoleEventArgs args)
    {
        string texName = args.GetStringFrom(2);
        if (string.IsNullOrEmpty(texName))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        if (!m_cachedTextures.TryGetValue(texName, out Texture2D? texture))
        {
            args.Context.LogWarning("Failed to find texture");
            return;
        }
        Export(texture, FileManager.ExportFolder);
    }

    [Obsolete]
    public static List<string> GetTextureOptions(int i, string word) => i switch
    {
        2 => m_cachedTextures.Keys.ToList(),
        _ => new List<string>()
    };
    
    [Obsolete]
    public static void SearchTextures(Terminal.ConsoleEventArgs args)
    {
        string query = args.GetStringFrom(2);
        List<string> names = m_cachedTextures.Keys.ToList();
        for (int i = 0; i < names.Count; ++i)
        {
            string? name = names[i];
            if (name.ToLower().Contains(query.ToLower()))
            {
                args.Context.AddString("- " + name);
                MonsterDBPlugin.LogInfo(name);
            }
        }
    }

    [Obsolete]
    public static void SearchSprites(Terminal.ConsoleEventArgs args)
    {
        string query = args.GetStringFrom(2);
        List<string> names = m_cachedSprites.Keys.ToList();
        for (int i = 0; i < names.Count; ++i)
        {
            string? name = names[i];
            if (name.ToLower().Contains(query.ToLower()))
            {
                args.Context.AddString("- " + name);
                MonsterDBPlugin.LogInfo(name);
            }
        }
    }

    [Obsolete]
    public static List<string> GetSpriteOptions(int i, string word) => i switch
    {
        2 => GetSpriteNames(),
        _ => new List<string>()
    };

    public static List<string> GetSpriteNames() => m_cachedSprites.Keys.ToList();

    public static void ExportSprite(Terminal context, string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            context.LogWarning("Specify sprite name");
            return;
        }
        
        Sprite? sprite = GetSprite(input, null);
        if (sprite == null)
        {
            context.LogWarning($"Failed to find sprite: {input}");
            return;
        }
            
        ExportSprite(sprite, FileManager.ExportFolder);
    }
    
    [Obsolete]
    public static void ExportSprite(Terminal.ConsoleEventArgs args)
    {
        string name = args.GetStringFrom(2);
        if (string.IsNullOrEmpty(name))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }
        Sprite? sprite = GetSprite(name, null);
        if (sprite == null)
        {
            args.Context.LogWarning($"Failed to find sprite: {name}");
            return;
        }
            
        ExportSprite(sprite, FileManager.ExportFolder);
    }


    public static HashSet<Material> GetAllMaterials(this GameObject prefab)
    {
        Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>(true);
        HashSet<Material> materials = new HashSet<Material>();
        for (int i = 0; i < renderers.Length; ++i)
        {
            Renderer renderer = renderers[i];
            for (int y = 0; y < renderer.sharedMaterials.Length; ++y)
            {
                Material? material = renderer.sharedMaterials[y];
                if (material == null) continue;
                materials.Add(material);
            }
        }

        return materials;
    }

    public static bool TryGetTexture(string name, out Texture2D texture)
    {
        if (m_cachedTextures.TryGetValue(name, out texture)) return true;
        return false;
    }
    public static void Start()
    {
        string[] files = Directory.GetFiles(FileManager.ImportFolder, "*.png", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            ReadTexture(filePath);
        }
        MonsterDBPlugin.LogInfo($"Loaded {files.Length} PNG files");
    }

    public static void WriteAll()
    {
        GetAllTextures();
        string fileName = "Resources.Textures.Names.txt";
        string filePath = Path.Combine(ConfigManager.DirectoryPath, fileName);
        string[] text = m_cachedTextures.Keys.ToArray();
        File.WriteAllLines(filePath, text);

        GetAllSprites();
        fileName = "Resources.Sprites.Names.txt";
        filePath = Path.Combine(ConfigManager.DirectoryPath, fileName);
        text = m_cachedSprites.Keys.ToArray();
        File.WriteAllLines(filePath, text);
    }

    public static Dictionary<string, TextureData> GetTextureData() => m_customs;

    public static Texture? GetTexture(string name, Texture? defaultValue)
    {
        if (name == "null") return null;
        if (string.IsNullOrEmpty(name)) return defaultValue;
        
        if (m_cachedTextures.TryGetValue(name, out Texture2D? texture))
        {
            return texture;
        }
        if (m_customs.TryGetValue(name, out TextureData? data))
        {
            return data.ToTex(defaultValue as Texture2D);
        }
        
        MonsterDBPlugin.LogDebug("Failed to find texture: " + name);
        
        return defaultValue;
    }
    
    public static Dictionary<string, Texture2D> GetAllTextures(bool clear = false)
    {
        if (clear) m_cachedTextures.Clear();
        if (m_cachedTextures.Count > 0) return m_cachedTextures;
        foreach (Texture2D? texture in Resources.FindObjectsOfTypeAll<Texture2D>())
        {
            if (texture.GetInstanceID() < 0) continue;
            if (m_cachedTextures.ContainsKey(texture.name)) continue;
            m_cachedTextures[texture.name] = texture;
        }
        return m_cachedTextures;
    }

    public static Sprite? GetSprite(string name, Sprite? defaultValue)
    {
        if (string.IsNullOrEmpty(name)) return defaultValue;
        
        if (GetAllSprites().TryGetValue(name, out Sprite? sprite)) return sprite;

        if (m_customs.TryGetValue(name, out TextureData? data))
        {
            return data.ToSprite(defaultValue);
        }
        
        MonsterDBPlugin.LogDebug($"Failed to find sprite: {name}");
        return defaultValue;
    }

    public static Dictionary<string, Sprite> GetAllSprites(bool clear = false)
    {
        if (clear) m_cachedSprites.Clear();
        if (m_cachedSprites.Count > 0) return m_cachedSprites;
        foreach (Sprite? sprite in Resources.FindObjectsOfTypeAll<Sprite>())
        {
            if (sprite.GetInstanceID() < 0) continue;
            m_cachedSprites[sprite.name] = sprite;
        }
        return m_cachedSprites;
    }

    private static void Export(Texture texture, string path)
    {
        string fileName = texture.name;
        string filePath = Path.Combine(path, fileName + ".png");
        if (File.Exists(filePath)) return;
        
        try
        {
            
            RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
            Graphics.Blit(texture, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D newTex = new Texture2D(texture.width, texture.height);
            
            newTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            newTex.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            
            byte[]? encoded = newTex.EncodeToPNG();
            File.WriteAllBytes(filePath, encoded);
            MonsterDBPlugin.LogInfo($"Exported texture: {filePath}");
        }
        catch
        {
            MonsterDBPlugin.LogWarning("Failed to save texture: " + fileName);
        }
    }
    
    public static void ExportSprite(Sprite sprite, string path)
    {
        if (sprite == null || sprite.texture == null) return;
    
        string fileName = sprite.name;
        string filePath = Path.Combine(path, fileName + ".png");
        if (File.Exists(filePath)) return;
    
        try
        {
            Texture2D atlas = sprite.texture;
            Rect spriteRect = sprite.textureRect;
            
            bool coversFullTexture = 
                spriteRect.x == 0 && 
                spriteRect.y == 0 && 
                Mathf.Approximately(spriteRect.width, sprite.texture.width) && 
                Mathf.Approximately(spriteRect.height, sprite.texture.height);

            if (coversFullTexture)
            {
                Export(atlas, path);
                return;
            }
        
            RenderTexture tmp = RenderTexture.GetTemporary(
                atlas.width, 
                atlas.height, 
                0,
                RenderTextureFormat.Default, 
                RenderTextureReadWrite.sRGB);
        
            Graphics.Blit(atlas, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
        
            Texture2D newTex = new Texture2D(
                (int)spriteRect.width, 
                (int)spriteRect.height);
        
            newTex.ReadPixels(spriteRect, 0, 0);
            newTex.Apply();
        
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
        
            byte[] encoded = newTex.EncodeToPNG();
            File.WriteAllBytes(filePath, encoded);
            MonsterDBPlugin.LogInfo($"Exported sprite: {filePath}");
        }
        catch
        {
            MonsterDBPlugin.LogWarning("Failed to save sprite: " + fileName);
        }
    }

    private static bool Save(Material material, string path)
    {
        if (material == null) return false;
        if (material.GetInstanceID() < 0) return false;
        
        Texture? texture = material.mainTexture;
        if (texture == null) return false;

        Export(texture, path);
        return true;
    }

    private static void ReadTexture(string filePath)
    {
        if (!File.Exists(filePath)) return;
        TextureData data = new(filePath);
        m_customs[data.m_name] = data;
    }

    public static void Add(TextureData data)
    {
        m_customs[data.m_name] = data;
    }

    public static void RegisterNewIcon(Sprite icon)
    {
        m_cachedSprites[icon.name] = icon;
    }
}