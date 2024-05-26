using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB.DataBase;

public static class DataBase
{
    private static readonly Dictionary<string, GameObject> m_items = new();
    public static readonly Dictionary<string, Texture2D> m_textures = new();
    
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
        foreach (var texture in allTextures)
        {
            if (texture.name.IsNullOrWhiteSpace()) continue;
            m_textures[texture.name] = texture;
        }
        
        MonsterManager.CreateBaseHuman();
    }

    public static GameObject? TryGetGameObject(string prefabName)
    {
        if (!ZNetScene.instance || !ObjectDB.instance) return null;
        GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
        if (prefab != null) return prefab;
        prefab = ObjectDB.instance.GetItemPrefab(prefabName);
        if (prefab != null) return prefab;
        return !m_items.TryGetValue(prefabName, out GameObject item) ? null : item;
    }

    public static bool TryGetItemDrop(string itemName, out ItemDrop? output)
    {
        output = TryGetItemDrop(itemName);
        return output != null;
    }

    private static ItemDrop? TryGetItemDrop(string itemName)
    {
        if (!ZNetScene.instance || !ObjectDB.instance) return null;
        GameObject prefab = ZNetScene.instance.GetPrefab(itemName);
        if (prefab != null)
        {
            if (prefab.TryGetComponent(out ItemDrop itemDrop)) return itemDrop;
        }
        prefab = ObjectDB.instance.GetItemPrefab(itemName);
        if (prefab != null)
        {
            if (prefab.TryGetComponent(out ItemDrop itemDrop)) return itemDrop;
        }

        if (m_items.TryGetValue(itemName, out GameObject item))
        {
            if (item.TryGetComponent(out ItemDrop itemDrop)) return itemDrop;
        }

        return null;
    }
}