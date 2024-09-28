using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB.Solution;

public static class DataBase
{
    private static readonly Dictionary<string, GameObject> m_items = new();
    public static readonly Dictionary<string, Texture2D> m_textures = new();
    
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class CacheResources_Patch
    {
        private static void Postfix() => CacheResources();
    }

    private static void CacheResources()
    {
        List<GameObject> allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>().ToList();
        foreach (GameObject prefab in allGameObjects)
        {
            m_items[prefab.name] = prefab;
        }

        List<Texture2D> allTextures = Resources.FindObjectsOfTypeAll<Texture2D>().ToList();
        foreach (Texture2D texture in allTextures.Where(texture => !texture.name.IsNullOrWhiteSpace()))
        {
            m_textures[texture.name] = texture;
        }
    }
    
    public static GameObject? TryGetGameObject(string prefabName)
    {
        if (!ZNetScene.instance || !ObjectDB.instance) return null;
        GameObject prefab = ObjectDB.instance.GetItemPrefab(prefabName);
        if (prefab != null) return prefab;
        prefab = ZNetScene.instance.GetPrefab(prefabName);
        if (prefab != null) return prefab;
        return !m_items.TryGetValue(prefabName, out GameObject item) ? null : item;
    }

    public static bool TryGetTexture(string textureName, out Texture2D texture)
    {
        if (TextureManager.GetRegisteredTexture(textureName, out texture))
        {
            return true;
        }

        if (m_textures.TryGetValue(textureName, out texture))
        {
            return true;
        }

        return false;
    }
}