using UnityEngine;

namespace MonsterDB;

public class TextureRef(string textureName)
{
    private static readonly Texture m_emptyTex = new Texture2D(4, 4);

    public Texture? m_tex
    {
        get
        {
            if (tex != null) return tex;
            if (TextureManager.GetAllTextures().TryGetValue(textureName, out Texture2D? match))
            {
                tex = match;
                return tex;
            }
            MonsterDBPlugin.LogWarning($"Failed to find reference texture: {textureName}");
            return m_emptyTex;
        }  
    }

    private Texture? tex;
}