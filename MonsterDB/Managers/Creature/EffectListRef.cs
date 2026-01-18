using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class EffectListRef
{
    public List<EffectDataRef> m_effectPrefabs = new();
    
    [YamlIgnore, NonSerialized] 
    private readonly List<EffectList.EffectData> data = new();
    [YamlIgnore, NonSerialized] 
    private EffectList? _effects;

    [YamlIgnore]
    public EffectList Effects
    {
        get
        {
            if (_effects != null) return _effects;
            foreach (EffectDataRef? dataRef in m_effectPrefabs)
            {
                data.Add(dataRef.ToEffectData());
            }
            _effects = new EffectList();
            _effects.m_effectPrefabs = data.ToArray();
            return _effects;
        }
    }

    public GameObject[] Create(Vector3 basePos, Quaternion baseRot, Transform? baseParent = null, float scale = 1f,
        int variant = -1) => Effects.Create(basePos, baseRot, baseParent, scale, variant);

    public EffectListRef(params string[] effects) => Add(effects);
    public EffectListRef(params EffectDataRef[] refs) => m_effectPrefabs.AddRange(refs);
    public EffectListRef(){}

    public void Add(params string[] effects)
    {
        foreach (string name in effects)
        {
            m_effectPrefabs.Add(new EffectDataRef(){m_prefab = name});
        }
    }

    public void Add(params EffectDataRef[] refs) => m_effectPrefabs.AddRange(refs);

    [Serializable]
    public class EffectDataRef : Reference
    {
        public string m_prefab = "";
        [DefaultValue(-1)] public int m_variant = -1;
        public bool m_attach;
        public bool m_follow;
        public bool m_inheritParentRotation;
        public bool m_inheritParentScale;
        public bool m_multiplyParentVisualScale;
        public bool m_randomRotation;
        public bool m_scale;
        public string m_childTransform = string.Empty;
        
        [YamlIgnore, NonSerialized] 
        private EffectList.EffectData? _data;

        public EffectList.EffectData ToEffectData()
        {
            if (_data != null) return _data;
            if (PrefabManager.GetPrefab(m_prefab) is not { } prefab)
            {
                MonsterDBPlugin.LogError("Effect Data Reference invalid: " + m_prefab);
                return new();
            }
            _data = new EffectList.EffectData()
            {
                m_prefab = prefab,
                m_variant = m_variant,
                m_attach = m_attach,
                m_follow = m_follow,
                m_inheritParentRotation = m_inheritParentRotation,
                m_inheritParentScale = m_inheritParentScale,
                m_multiplyParentVisualScale = m_multiplyParentVisualScale,
                m_randomRotation = m_randomRotation,
                m_scale = m_scale,
                m_childTransform = m_childTransform
            };
            return _data;
        }
    }
}

public static partial class Extensions
{
    public static EffectListRef ToEffectListRef(this EffectList el)
    {
        EffectListRef effectListRef = new EffectListRef();
        foreach (EffectList.EffectData? effectData in el.m_effectPrefabs)
        {
            if (effectData.m_prefab == null) continue;
            EffectListRef.EffectDataRef effectRef = new EffectListRef.EffectDataRef();
            effectRef.Setup(effectData);
            effectListRef.Add(effectRef);
        }

        return effectListRef;
    }
}