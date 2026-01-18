using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace MonsterDB;

[PublicAPI]
public static class PrefabManager
{
    internal static ZNetScene? _ZNetScene;
    internal static ObjectDB? _ObjectDB;
    internal static readonly Dictionary<string, GameObject> _prefabs;
    internal static List<GameObject> PrefabsToRegister;
    internal static Dictionary<string, Clone> Clones;

    static PrefabManager()
    {
        _prefabs = new Dictionary<string, GameObject>();
        PrefabsToRegister = new List<GameObject>();
        Clones =  new Dictionary<string, Clone>();
    }

    public static void Start()
    {
        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.DeclaredMethod(typeof(FejdStartup), nameof(FejdStartup.Awake)), postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_FejdStartup))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ZNetScene), nameof(ZNetScene.Awake)),
            prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_ZNetScene_Awake))),
            postfix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(LoadManager), nameof(LoadManager.Start))));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(ObjectDB), nameof(ObjectDB.Awake)), prefix: new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), nameof(Patch_ObjectDB_Awake))));
    }
    
    public static void AddToCache(GameObject go)
    {
        if (go == null) return;
        if (_prefabs.ContainsKey(go.name) || go.GetInstanceID() < 0) return;
        _prefabs.Add(go.name, go);
    }
    
    internal static List<string> GetAllPrefabNames<T>() where T : MonoBehaviour
    {
        if (ZNetScene.instance)
        {
            return ZNetScene.instance.m_prefabs
                .Where(x => x != null)
                .Where(x => x.GetComponent<T>())
                .Select(x=>x.name)
                .ToList();
        }

        if (_ZNetScene != null)
        {
            return _ZNetScene.m_prefabs
                .Where(x => x != null)
                .Where(x => x.GetComponent<T>())
                .Select(x=>x.name)
                .ToList();
        }
        return new List<string>();
    }

    internal static List<GameObject> GetAllPrefabs<T>() where T : MonoBehaviour
    {
        if (ZNetScene.instance)
        {
            return ZNetScene.instance.m_prefabs
                .Where(x => x != null)
                .Where(x => x.GetComponent<T>())
                .ToList();
        }

        if (_ZNetScene != null)
        {
            return _ZNetScene.m_prefabs
                .Where(x => x != null)
                .Where(x => x.GetComponent<T>())
                .ToList();
        }
        return new List<GameObject>();
    }
    internal static GameObject? GetPrefab(string prefabName)
    {
        GameObject? prefab;
        
        if (ZNetScene.instance != null)
        {
            prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (prefab != null) return prefab;
        }

        if (_ZNetScene != null)
        {
            prefab = _ZNetScene.m_prefabs.Find(p => p.name == prefabName);
            if (prefab != null) return prefab;
        }

        if (CloneManager.clones.TryGetValue(prefabName, out prefab))
        {
            return prefab;
        }

        if (AudioManager.clips.TryGetValue(prefabName, out AudioRef? clip) && clip.sfx != null)
        {
            return clip.sfx;
        }

        if (_prefabs.TryGetValue(prefabName, out prefab))
        {
            return prefab;
        }
        
        CachePrefabs();
        
        if (_prefabs.TryGetValue(prefabName, out prefab))
        {
            return prefab;
        }
        MonsterDBPlugin.LogWarning($"Prefab '{prefabName}' not found");

        return null;
    }
    private static void CachePrefabs()
    {
        GameObject[]? allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allPrefabs.Length; ++i)
        {
            GameObject go =  allPrefabs[i];
            if (go == null || go.GetInstanceID() <= 0 || _prefabs.ContainsKey(go.name)) continue;
            _prefabs.Add(go.name, go);
        }
    }

    public static List<string> SearchCache<T>(string query) where T : MonoBehaviour
    {
        return _prefabs.Values
            .Where(x => x != null)
            .Where(x => x.GetComponent<T>())
            .Where(x => x.name.ToLower().Contains(query.ToLower()))
            .Select(x => x.name)
            .ToList();
    }
    
    public static StatusEffect? GetStatusEffect(string name)
    {
        if (ObjectDB.instance) return ObjectDB.instance.GetStatusEffect(name.GetStableHashCode());
        if (_ObjectDB != null) return _ObjectDB.m_StatusEffects.FirstOrDefault(x => x.name == name);
        return null;
    }

    public static void Register(this ZNetScene scene, GameObject prefab)
    {
        if (scene.m_prefabs.Contains(prefab) || !prefab.GetComponent<ZNetView>()) return;
        scene.m_prefabs.Add(prefab);
        scene.m_namedPrefabs[prefab.name.GetStableHashCode()] = prefab;
    }

    public static void Register(this ObjectDB db, GameObject prefab)
    {
        if (db.m_items.Contains(prefab) || !prefab.GetComponent<ItemDrop>()) return;
        db.m_items.Add(prefab);
        db.m_itemByHash[prefab.name.GetStableHashCode()] = prefab;
    }
    
    public static void RegisterPrefab(GameObject? prefab)
    {
        if (prefab == null) return;

        if (ZNetScene.instance)
        {
            ZNetScene.instance.Register(prefab);
        }

        if (ObjectDB.instance)
        {
            ObjectDB.instance.Register(prefab);
        }
        
        PrefabsToRegister.Add(prefab);
    }

    [HarmonyPriority(Priority.VeryHigh)]
    internal static void Patch_ZNetScene_Awake(ZNetScene __instance)
    {
        foreach (GameObject prefab in PrefabsToRegister)
        {
            if (__instance.m_prefabs.Contains(prefab) || !prefab.GetComponent<ZNetView>()) continue;
            __instance.m_prefabs.Add(prefab);
        }
    }

    internal static void Patch_ObjectDB_Awake(ObjectDB __instance)
    {
        foreach (GameObject prefab in PrefabsToRegister)
        {
            if (__instance.m_items.Contains(prefab) || !prefab.GetComponent<ItemDrop>()) continue;
            __instance.m_items.Add(prefab);
        }
    }

    [HarmonyPriority(Priority.VeryHigh)]
    internal static void Patch_FejdStartup(FejdStartup __instance)
    {
        _ZNetScene = __instance.m_objectDBPrefab.GetComponent<ZNetScene>();
        _ObjectDB = __instance.m_objectDBPrefab.GetComponent<ObjectDB>();
        ShaderRef.CacheShaders();
        foreach (Clone clone in Clones.Values)
        {
            clone.Create();
        }
        ConfigManager.SetupWatcher();
    }
}