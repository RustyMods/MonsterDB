using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public class TextureData
{
    public string m_filePath;
    public readonly string m_name;
    public readonly byte[] m_bytes;
    private Texture2D? m_tex;
    private Sprite? m_sprite;

    public TextureData(string filePath)
    {
        m_filePath = filePath;
        m_name = Path.GetFileNameWithoutExtension(filePath);
        m_bytes = File.ReadAllBytes(filePath);
    }

    public TextureData(string fileName, byte[] bytes)
    {
        m_filePath = "Server";
        m_name = fileName;
        m_bytes = bytes;
    }

    public Texture ToTex(Texture2D? original)
    {
        if (m_tex != null) return m_tex;
        Texture2D tex = new Texture2D(original?.width ?? 4, original?.height ?? 4, original?.format ?? TextureFormat.RGBA32, original?.mipmapCount > 1);
        tex.LoadImage4x(m_bytes);
        tex.Apply();
        tex.name = m_name;
        m_tex = tex;
        return tex;
    }

    public Sprite? ToSprite(Sprite? original)
    {
        if (m_sprite != null) return m_sprite;
        Texture2D? tex = ToTex(original?.texture) as Texture2D;
        if (tex == null) return original;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        sprite.name = m_name;
        m_sprite = sprite;
        return sprite;
    }

    public void Write()
    {
        var folderPath = Path.Combine(FileManager.ImportFolder, "textures");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        string filePath = Path.Combine(folderPath, m_name + ".png");
        File.WriteAllBytes(filePath, m_bytes);
    }
}

public static partial class Extensions
{
    private static readonly MethodInfo LoadImage = AccessTools.Method(typeof(ImageConversion), nameof(ImageConversion.LoadImage), new [] { typeof(Texture2D), typeof(byte[]) });
    public static bool LoadImage4x(this Texture2D tex, byte[] data)
    {
        return (bool)LoadImage.Invoke(null, new object[] { tex , data});
    }
}