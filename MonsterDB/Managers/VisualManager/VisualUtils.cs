using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public static class VisualUtils
{
    public static void CloneMaterials(Renderer r, ref Dictionary<string, Material> mats)
    {
        List<Material> newMats = new();
        for (int i = 0; i < r.sharedMaterials.Length; ++i)
        {
            Material mat = r.sharedMaterials[i];
            if (mat == null) continue;
            string name = mat.name.Replace("(Instance)", string.Empty);
            if (mats.TryGetValue(name, out Material? clonedMat))
            {
                newMats.Add(clonedMat);
            }
            else
            {
                clonedMat = new Material(mat);
                clonedMat.name = mat.name;
                newMats.Add(clonedMat);
                mats.Add(name, clonedMat);
            }
        }
        r.sharedMaterials = newMats.ToArray();
        r.materials = newMats.ToArray();
    }
}