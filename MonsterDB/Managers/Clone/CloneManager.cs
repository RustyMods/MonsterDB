using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public static class CloneManager
{
    private static readonly GameObject root;
    internal static readonly Dictionary<string, GameObject> clones = new();

    static CloneManager()
    {
        root = new GameObject($"{MonsterDBPlugin.ModName}_prefab_root");
        UnityEngine.Object.DontDestroyOnLoad(root);
        root.SetActive(false);
    }

    public static Transform GetRootTransform()
    {
        return root.transform;
    }
}