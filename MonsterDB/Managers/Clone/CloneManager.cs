using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public static class CloneManager
{
    private static readonly GameObject root;
    internal static readonly Dictionary<string, GameObject> prefabs;
    internal static readonly Dictionary<string, Clone> clones;


    static CloneManager()
    {
        prefabs = new Dictionary<string, GameObject>();
        clones =  new Dictionary<string, Clone>();
        root = new GameObject($"{MonsterDBPlugin.ModName}_prefab_root");
        UnityEngine.Object.DontDestroyOnLoad(root);
        root.SetActive(false);
    }

    public static Transform GetRootTransform()
    {
        return root.transform;
    }

    public static void Clear()
    {
        for (int i = 0; i < root.transform.childCount; ++i)
        {
            GameObject child = root.transform.GetChild(i).gameObject;
            int hash = child.name.GetStableHashCode();
            if (ZNetScene.instance)
            {
                ZNetScene.instance.m_prefabs.Remove(child);
                ZNetScene.instance.m_namedPrefabs.Remove(hash);
            }
            if (ObjectDB.instance)
            {
                ObjectDB.instance.m_items.Remove(child);
                ObjectDB.instance.m_itemByHash.Remove(hash);
            }
            PrefabManager.PrefabsToRegister.Remove(child);
            PrefabManager._prefabs.Remove(child.name);
            MonsterDBPlugin.LogDebug($"Destroyed {child.name}");
            Object.Destroy(child);
        }

        prefabs.Clear();
        clones.Clear();
    }
}