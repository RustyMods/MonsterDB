using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class Helpers
{
    public static void ReadEffectInfo(string filePath, ref List<EffectInfo> effectList)
    {
        if (!File.Exists(filePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string serial = File.ReadAllText(filePath);
        if (serial.IsNullOrWhiteSpace()) return;
        try
        {
            effectList = deserializer.Deserialize<List<EffectInfo>>(serial);
        }
        catch
        {
            LogParseFailure(filePath);
        }
    }

    public static void LogParseFailure(string filePath)
    {
        MonsterDBPlugin.LogDebug("Failed to parse file:");
        MonsterDBPlugin.LogDebug(filePath);
    }
    
    public static void UpdateEffectList(List<EffectInfo> effectInfo, ref EffectList effectList, Vector3 scale)
    {
        if (effectInfo.Count <= 0) return;
        List<EffectList.EffectData> effects = new();
        foreach (EffectInfo data in effectInfo)
        {
            GameObject? prefab = PrefabManager.GetPrefab(data.PrefabName);
            if (prefab == null) continue;
            if (prefab.GetComponent<Ragdoll>())
            {
                prefab.transform.localScale = scale;
            }
            effects.Add(new EffectList.EffectData()
            {
                m_prefab = prefab,
                m_enabled = data.Enabled,
                m_attach = data.Attach,
                m_scale = data.Scale,
                m_follow = data.Follow,
                m_variant = data.Variant,
                m_childTransform = data.ChildTransform,
                m_randomRotation = data.RandomRotation,
                m_inheritParentScale = data.InheritParentScale,
                m_multiplyParentVisualScale = data.MultiplyParentVisualScale,
                m_inheritParentRotation = data.InheritParentRotation
            });
        }

        effectList = new EffectList()
        {
            m_effectPrefabs = effects.ToArray()
        };
        
    }
}