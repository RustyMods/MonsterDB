using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseRagdoll : Header
{
    [YamlMember(Order = 6)] public RagdollRef? Ragdoll;
    [YamlMember(Order = 7)] public VisualRef? Visuals;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        if (!prefab.TryGetComponent(out Ragdoll component)) return;
        base.Setup(prefab, isClone, source);
        Type = BaseType.Ragdoll;
        Ragdoll = new RagdollRef(component);
        Visuals = new VisualRef(prefab);
        IsCloned = isClone;
        ClonedFrom = source;
        Prefab = prefab.name;
    }
    
    public override VisualRef? GetVisualData() => Visuals;

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseRagdoll originalRagdoll) return;
        if (Ragdoll != null && originalRagdoll.Ragdoll != null) Ragdoll.ResetTo(originalRagdoll.Ragdoll);
        if (Visuals != null && originalRagdoll.Visuals != null) Visuals.ResetTo(originalRagdoll.Visuals);
    }
    
    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        UpdatePrefab(prefab);
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }

    private void UpdatePrefab(GameObject prefab)
    {
        UpdateRagdoll(prefab);
        UpdateVisuals(prefab);
    }

    private void UpdateRagdoll(GameObject prefab)
    {
        if (Ragdoll != null && prefab.TryGetComponent(out Ragdoll component))
        {
            Ragdoll.UpdateFields(component, prefab.name, true);
        }
    }

    private void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.Update(prefab, false, false);
        }
    }
}