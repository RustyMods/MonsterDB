using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class Header
{
    [YamlMember(Order = 0)] public string? GameVersion;
    [YamlMember(Order = 1)] public string? ModVersion;
    [YamlMember(Order = 2)] public BaseType Type;
    [YamlMember(Order = 3)] public string Prefab = "";
    [YamlMember(Order = 4)] public string ClonedFrom = "";
    [YamlMember(Order = 5)] public bool IsCloned;
    
    public virtual void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        SetupVersions();
    }

    protected void SetupVersions()
    {
        GameVersion = Version.GetVersionString();
        ModVersion = MonsterDBPlugin.ModVersion;
    }

    public virtual void Update()
    {
        MonsterDBPlugin.LogInfo(LoadManager.resetting ? $"Reset {Prefab}" : $"Updated {Prefab}");
        if (LoadManager.modified.ContainsKey(Prefab)) return;
        LoadManager.modified.Add(this);
    }
    
    public virtual void CopyFields(Header original)
    {
        
    }

    public virtual VisualRef? GetVisualData() => null;

    public virtual bool Clean()
    {
        if (!LoadManager.TryGetOriginal(Prefab, out Header original)) return false;

        if (original.GetType() != GetType()) return false;

        int count = 0;
        FieldInfo[] fields = GetType().GetFields(Reference.FieldBindingFlags);
        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            object? m_value = field.GetValue(this);
            object? o_value = field.GetValue(original);
            if (m_value is not Reference m_ref || o_value is not Reference o_ref) continue;
            count += o_ref.Clean(m_ref, Prefab, true);
        }
        MonsterDBPlugin.LogInfo($"[{Prefab}] cleaned {count} fields");
        return count > 0;
    }
}