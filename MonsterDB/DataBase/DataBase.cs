using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.DataBase;

public static class MonsterDB
{
    private static readonly Dictionary<string, GameObject> m_items = new();
    public static readonly Dictionary<string, Texture2D> m_textures = new();
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class CacheResources_Patch
    {
        private static void Postfix() => CacheResources();
    }

    public static List<GameObject> GetCachedItems() => m_items.Values.Where(x => x != null && x.GetComponent<ItemDrop>()).ToList();

    private static CreatureItem? GetCreatureItem(string prefabName)
    {
        GameObject? prefab = TryGetGameObject(prefabName);
        if (prefab == null) return null;
        if (!prefab.TryGetComponent(out ItemDrop component)) return null;
        return MonsterManager.FormatAttack(component);
    }

    public static bool SaveCreatureItem(string prefabName)
    {
        var data = GetCreatureItem(prefabName);
        if (data == null) return false;
        string filePath = Paths.DataPath + Path.DirectorySeparatorChar + prefabName + ".yml";
        var serializer = new SerializerBuilder().Build();
        var serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);
        return true;
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
        GameObject? prefab = TryGetGameObject(itemName);
        if (prefab == null) return null;
        return prefab.TryGetComponent(out ItemDrop itemDrop) ? itemDrop : null;
    }
}