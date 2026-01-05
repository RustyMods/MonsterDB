using System.IO;
using UnityEngine;

namespace MonsterDB;

public class TextureData
{
    public string m_filePath;
    public readonly string m_name;
    public readonly byte[] m_bytes;
    private Texture2D? m_tex;

    public TextureData(string filePath)
    {
        m_filePath = filePath;
        m_name = Path.GetFileNameWithoutExtension(filePath);
        m_bytes = File.ReadAllBytes(filePath);
    }

    public Texture ToTex(Texture2D? original)
    {
        if (m_tex != null) return m_tex;
        Texture2D tex = new Texture2D(original?.width ?? 4, original?.height ?? 4, original?.format ?? TextureFormat.RGBA32, original?.mipmapCount > 1);
        tex.LoadImage(m_bytes);
        tex.Apply();
        tex.name = m_name;
        m_tex = tex;
        return tex;
    }
}