using UnityEngine;

namespace MonsterDB;

public class TextureRef
{
    private static readonly Texture m_emptyTex = new Texture2D(4, 4);
    private readonly string m_name;
    public Texture? m_tex
    {
        get
        {
            if (tex != null) return tex;
            if (TextureManager.GetAllTextures().TryGetValue(m_name, out Texture2D? match))
            {
                tex = match;
                return tex;
            }
            MonsterDBPlugin.LogWarning($"Failed to find reference texture: {m_name}");
            return m_emptyTex;
        }  
    }

    private Texture? tex;

    public TextureRef(string textureName)
    {
        m_name = textureName;
    }
}