using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class Helpers
{
    public static void SaveEffectList(EffectList list, ref List<EffectInfo> data)
    {
        if (list.m_effectPrefabs.Length <= 0) return;
        foreach (EffectList.EffectData effect in list.m_effectPrefabs)
        {
            EffectInfo effectInfo = new()
            {
                PrefabName = effect.m_prefab.name,
                Enabled = effect.m_enabled,
                Variant = effect.m_variant,
                Attach = effect.m_attach,
                Follow = effect.m_follow,
                InheritParentRotation = effect.m_inheritParentRotation,
                InheritParentScale = effect.m_inheritParentScale,
                MultiplyParentVisualScale = effect.m_multiplyParentVisualScale,
                RandomRotation = effect.m_randomRotation,
                Scale = effect.m_scale,
                ChildTransform = effect.m_childTransform
            };
            data.Add(effectInfo);
        }
    }
    
    public static void WriteEffectList(EffectList list, string filePath)
    {
        if (list.m_effectPrefabs.Length <= 0)
        {
            File.WriteAllText(filePath, "");
            return;
        }
        ISerializer serializer = new SerializerBuilder().Build();
        List<EffectInfo> data = new();
        foreach (EffectList.EffectData effect in list.m_effectPrefabs)
        {
            EffectInfo effectInfo = new()
            {
                PrefabName = effect.m_prefab.name,
                Enabled = effect.m_enabled,
                Variant = effect.m_variant,
                Attach = effect.m_attach,
                Follow = effect.m_follow,
                InheritParentRotation = effect.m_inheritParentRotation,
                InheritParentScale = effect.m_inheritParentScale,
                MultiplyParentVisualScale = effect.m_multiplyParentVisualScale,
                RandomRotation = effect.m_randomRotation,
                Scale = effect.m_scale,
                ChildTransform = effect.m_childTransform
            };
            data.Add(effectInfo);
        }
        string serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);
    }
    
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

    public static void CloneEffectList(GameObject critter, ref EffectList effectList, Dictionary<string, Material> ragDollMats)
    {
        if (effectList.m_effectPrefabs.Length <= 0) return;
        EffectList list = new();
        List<EffectList.EffectData> data = new();
        foreach (EffectList.EffectData effect in effectList.m_effectPrefabs)
        {
            GameObject prefab = effect.m_prefab;
            if (!prefab.GetComponent<Ragdoll>())
            {
                data.Add(effect);
            }
            else
            {
                GameObject clone = Object.Instantiate(prefab, MonsterDBPlugin.m_root.transform, false);
                clone.name = critter.name + "_ragdoll";
                VisualMethods.SetMaterials(clone, ragDollMats);
                RegisterToZNetScene(clone);
                data.Add(new EffectList.EffectData()
                {
                    m_prefab = clone,
                    m_enabled = effect.m_enabled,
                    m_variant = effect.m_variant,
                    m_attach = effect.m_attach,
                    m_follow = effect.m_follow,
                    m_inheritParentRotation = effect.m_inheritParentRotation,
                    m_inheritParentScale = effect.m_inheritParentScale,
                    m_multiplyParentVisualScale =  effect.m_multiplyParentVisualScale,
                    m_randomRotation = effect.m_randomRotation,
                    m_scale = effect.m_scale,
                    m_childTransform = effect.m_childTransform
                });
            }
        }

        list.m_effectPrefabs = data.ToArray();
        effectList = list;
    }

    public static Vector3 GetScale(VisualMethods.ScaleData data) => new Vector3(data.x, data.y, data.z);

    public static void UpdateEffectList(List<EffectInfo> effectInfo, ref EffectList effectList, Vector3 scale)
    {
        if (effectInfo.Count <= 0) return;
        List<EffectList.EffectData> effects = new();
        foreach (EffectInfo data in effectInfo)
        {
            GameObject? prefab = DataBase.TryGetGameObject(data.PrefabName);
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

    public static void LogParseFailure(string filePath)
    {
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Failed to parse file:");
        MonsterDBPlugin.MonsterDBLogger.LogDebug(filePath);
    }

    public static void RegisterToZNetScene(GameObject prefab)
    {
        if (!ZNetScene.instance) return;
        if (!ZNetScene.instance.m_prefabs.Contains(prefab))
        {
            ZNetScene.instance.m_prefabs.Add(prefab);
        }

        ZNetScene.instance.m_namedPrefabs[prefab.name.GetStableHashCode()] = prefab;
    }

    public static void RegisterToObjectDB(GameObject prefab)
    {
        if (!ObjectDB.instance) return;
        if (!ObjectDB.instance.m_items.Contains(prefab))
        {
            ObjectDB.instance.m_items.Add(prefab);
        }

        ObjectDB.instance.m_itemByHash[prefab.name.GetStableHashCode()] = prefab;
    }

    public static void RemoveFromZNetScene(GameObject prefab)
    {
        if (!ZNetScene.instance) return;
        ZNetScene.instance.m_prefabs.Remove(prefab);
        ZNetScene.instance.m_namedPrefabs.Remove(prefab.name.GetStableHashCode());
    }

    public static void RemoveFromObjectDB(GameObject prefab)
    {
        if (!ObjectDB.instance) return;
        ObjectDB.instance.m_items.Remove(prefab);
        ObjectDB.instance.m_itemByHash.Remove(prefab.name.GetStableHashCode());
    }
}