using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

public static class ShaderRef
{
    public static readonly int _Color = Shader.PropertyToID("_Color");
    public static readonly int _Hue = Shader.PropertyToID("_Hue");
    public static readonly int _Saturation = Shader.PropertyToID("_Saturation");
    public static readonly int _Value = Shader.PropertyToID("_Value");
    public static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

    private static readonly Dictionary<string, Shader> m_shaders = new();

    public static Shader GetShader(string shaderName, Shader originalShader)
    {
        return !m_shaders.TryGetValue(shaderName, out Shader shader) ? originalShader : shader;
    }

    public static void CacheShaders()
    {
        AssetBundle[]? assetBundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
        foreach (AssetBundle? bundle in assetBundles)
        {
            IEnumerable<Shader>? bundleShaders;
            try
            {
                bundleShaders = bundle.isStreamedSceneAssetBundle && bundle
                    ? bundle
                        .GetAllAssetNames()
                        .Select(bundle.LoadAsset<Shader>)
                        .Where(shader => shader != null)
                    : bundle?.LoadAllAssets<Shader>();
            }
            catch (Exception)
            {
                continue;
            }

            if (bundleShaders == null) continue;
            foreach (Shader? shader in bundleShaders)
            {
                if (m_shaders.ContainsKey(shader.name)) continue;
                m_shaders[shader.name] = shader;
            }
        }
    }
}