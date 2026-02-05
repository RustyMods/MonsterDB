using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace MonsterDB;

public static class ShaderRef
{
    public static readonly int _Color = Shader.PropertyToID("_Color");
    public static readonly int _Hue = Shader.PropertyToID("_Hue");
    public static readonly int _Saturation = Shader.PropertyToID("_Saturation");
    public static readonly int _Value = Shader.PropertyToID("_Value");
    public static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");
    public static readonly int _TintColor = Shader.PropertyToID("_TintColor");
    public static readonly int _EmissionMap = Shader.PropertyToID("_EmissionMap");

    private static readonly Dictionary<string, Shader> m_shaders = new();

    public static Shader GetShader(string shaderName, Shader originalShader)
    {
        if (string.IsNullOrEmpty(shaderName)) return originalShader;
        if (m_shaders.TryGetValue(shaderName, out Shader shader)) return shader;
        MonsterDBPlugin.LogWarning("Failed to find shader: " + shaderName);
        return originalShader;
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
        
        var shaders = Resources.FindObjectsOfTypeAll<Shader>();
        foreach (Shader? shader in shaders)
        {
            if (shader == null) continue;
            if (!m_shaders.ContainsKey(shader.name))
            {
                m_shaders[shader.name] = shader;
            }
        }
    }

    public static void SearchShaders(Terminal.ConsoleEventArgs args)
    {
        string query = args.GetStringFrom(2);
        if (string.IsNullOrEmpty(query))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        foreach (string? name in m_shaders.Keys)
        {
            if (name.ToLower().Contains(query.ToLower()))
            {
                MonsterDBPlugin.LogInfo(name);
            }
        }
    }

    public static List<string> GetShaderOptions(int i, string word) => i switch
    {
        2 => GetShaderNames(),
        _ => new List<string>()
    };

    public static List<string> GetShaderNames() => m_shaders.Keys.ToList();

    public static void PrintShaderProperties(Terminal.ConsoleEventArgs args)
    {
        if (args.Length < 3)
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        string[]? line = args.Args;
        string query = string.Join(" ", line, 2, line.Length - 2);
        
        if (string.IsNullOrEmpty(query))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        if (!m_shaders.TryGetValue(query, out Shader? shader))
        {
            args.Context.LogWarning($"Failed to find shader: {query}");
            return;
        }
        
        MonsterDBPlugin.LogInfo($"Shader: {shader.name}");
        args.Context.AddString($"Shader: {shader.name}");

        int count = shader.GetPropertyCount();
        for (int i = 0; i < count; ++i)
        {
            string? prop = shader.GetPropertyName(i);
            ShaderPropertyType type = shader.GetPropertyType(i);
            if (prop != null)
            {
                args.Context.AddString($"{prop}: {type}");
                MonsterDBPlugin.LogInfo($"{prop}: {type}");
            }
        }
    }
}