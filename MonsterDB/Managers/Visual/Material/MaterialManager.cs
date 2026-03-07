using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public static class MaterialManager
{
    private static readonly Dictionary<string, Material> _materials;
    private static readonly List<string> _invalidQueries;
    private static float _lastSearchTime;
    
    static MaterialManager()
    {
        _materials = new Dictionary<string, Material>();
        _invalidQueries = [];
    }

    public static bool TryGetMaterial(string name, out Material material)
    {
        if (!_materials.TryGetValue(name, out material))
        {
            if (Time.time - _lastSearchTime < 10f && _invalidQueries.Contains(name))
            {
                return false;
            }
            CacheMaterials();
        }

        bool result = _materials.TryGetValue(name, out material);
        if (!result && !_invalidQueries.Contains(name)) _invalidQueries.Add(name);
        else _invalidQueries.Remove(name);
        return result;
    }

    private static void CacheMaterials()
    {
        _lastSearchTime = Time.time;
        
        Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
        for (int i = 0; i < materials.Length; ++i)
        {
            Material material = materials[i];
            if (material.GetInstanceID() < 0 || _materials.ContainsKey(material.name)) continue;
            _materials.Add(material.name, material);
        }
    }
}