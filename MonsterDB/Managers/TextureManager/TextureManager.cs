using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public static class TextureManager
{
    private static readonly Dictionary<string, Texture2D> m_cachedTextures;
    private static readonly Dictionary<string, Sprite> m_cachedSprites;
    private static readonly Dictionary<string, TextureData> m_customs;
    private static readonly string TextureFolder;
    private static readonly string CustomFolder;
    private static readonly string ExportFolder;

    static TextureManager()
    {
        m_cachedSprites =  new Dictionary<string, Sprite>();
        m_cachedTextures = new Dictionary<string, Texture2D>();
        m_customs = new Dictionary<string, TextureData>();
        TextureFolder = Path.Combine(ConfigManager.DirectoryPath, "Textures");
        CustomFolder = Path.Combine(TextureFolder, "Customs");
        ExportFolder = Path.Combine(TextureFolder, "Exports");
        if (!Directory.Exists(TextureFolder)) Directory.CreateDirectory(TextureFolder);
        if (!Directory.Exists(CustomFolder)) Directory.CreateDirectory(CustomFolder);
        if (!Directory.Exists(ExportFolder)) Directory.CreateDirectory(ExportFolder);
        
        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(ZoneSystem), nameof(ZoneSystem.Awake)),
            new HarmonyMethod(AccessTools.Method(typeof(TextureManager), nameof(WriteAll))));
        
        Start();
    }
    private static void Start()
    {
        string[] files = Directory.GetFiles(CustomFolder, "*.png", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            ReadTexture(filePath);
        }
        MonsterDBPlugin.LogInfo($"Loaded {files.Length} custom textures.");
    }
    public static void Setup()
    {
        Command export = new Command("export_main_tex", "[prefabName]: export creature main texture", args =>
        {
            if (args.Length < 3) return false;

            string? prefabName = args[2];
            if (string.IsNullOrEmpty(prefabName)) return false;
            
            GameObject? prefab = PrefabManager.GetPrefab(prefabName);
            if (prefab == null) return false;

            Renderer? mainRenderer = null;
            LevelEffects? lvlEffects = prefab.GetComponentInChildren<LevelEffects>();
            if (lvlEffects != null) mainRenderer = lvlEffects.m_mainRender;
            if (mainRenderer == null)
            {
                if (prefab.TryGetComponent(out VisEquipment visEq))
                {
                    mainRenderer = visEq.m_bodyModel;
                }
            }

            if (mainRenderer == null)
            {
                mainRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (mainRenderer == null) return false;
            
            Material[]? materials = mainRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; ++i)
            {
                Material material = materials[i];
                Save(material, ExportFolder);
            }
            
            return true;
        }, PrefabManager.GetAllPrefabNames<Character>);

        Command save = new Command("export_tex", "[textureName]: export texture as png", args =>
        {   
            if (args.Length < 3) return false;

            string? texName = args[2];
            if (string.IsNullOrEmpty(texName)) return false;

            if (!m_cachedTextures.TryGetValue(texName, out Texture2D? texture))
            {
                MonsterDBPlugin.LogWarning("Failed to find texture");
                return true;
            }
            Export(texture, ExportFolder);
            return true;
        }, m_cachedTextures.Keys.ToList);
    }
    public static void WriteAll()
    {
        GetAllTextures();
        string fileName = "Resources.Textures.Names.txt";
        string filePath = Path.Combine(TextureFolder, fileName);
        string[] text = m_cachedTextures.Keys.ToArray();
        File.WriteAllLines(filePath, text);

        GetAllSprites();
        fileName = "Resources.Sprites.Names.txt";
        filePath = Path.Combine(TextureFolder, fileName);
        text = m_cachedSprites.Keys.ToArray();
        File.WriteAllLines(filePath, text);
    }

    public static Texture? GetTexture(string name, Texture? defaultValue)
    {
        if (m_cachedTextures.TryGetValue(name, out Texture2D? texture))
        {
            return texture;
        }
        if (m_customs.TryGetValue(name, out TextureData? data))
        {
            return data.ToTex(defaultValue as Texture2D);
        }
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
        if (GetAllSprites().TryGetValue(name, out Sprite? sprite)) return sprite;
        return defaultValue;
    }

    private static Dictionary<string, Sprite> GetAllSprites(bool clear = false)
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
                RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
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
    
    private static void Save(Material material, string path)
    {
        if (material == null) return;
        if (material.GetInstanceID() < 0) return;
        
        Texture? texture = material.mainTexture;
        if (texture == null) return;

        Export(texture, path);
    }

    private static void ReadTexture(string filePath)
    {
        if (!File.Exists(filePath)) return;
        TextureData data = new(filePath);
        m_customs[data.m_name] = data;
    }
}