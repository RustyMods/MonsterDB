using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MonsterDB.Solution.Methods;
using UnityEngine;

namespace MonsterDB.Solution;

public static class DataBase
{
    public static readonly Dictionary<string, GameObject> m_allObjects = new();
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
            m_allObjects[prefab.name] = prefab;
        }

        List<Texture2D> allTextures = Resources.FindObjectsOfTypeAll<Texture2D>().ToList();
        foreach (Texture2D texture in allTextures.Where(texture => !texture.name.IsNullOrWhiteSpace()))
        {
            m_textures[texture.name] = texture;
        }

        string filePath = TextureManager.m_texturePath + Path.DirectorySeparatorChar + "Resources.txt";
        if (File.Exists(filePath)) return;
        List<string> textureNames = m_textures.Keys.ToList();
        File.WriteAllLines(filePath, textureNames);
    }
    
    public static GameObject? TryGetGameObject(string prefabName)
    {
        if (!ZNetScene.instance || !ObjectDB.instance) return null;
        if (ObjectDB.instance.GetItemPrefab(prefabName) is { } dbPrefab) return dbPrefab;
        if (ZNetScene.instance.GetPrefab(prefabName) is { } zPrefab) return zPrefab; ;
        if (HumanMan.m_newHumans.TryGetValue(prefabName, out GameObject human)) return human;
        if (ItemDataMethods.m_clonedItems.TryGetValue(prefabName, out GameObject clone)) return clone;
        return !m_allObjects.TryGetValue(prefabName, out GameObject item) ? null : item;
    }

    public static bool TryGetTexture(string textureName, out Texture2D texture)
    {
        if (TextureManager.m_customTextures.TryGetValue(textureName, out texture)) return true;
        return m_textures.TryGetValue(textureName, out texture);
    }
}