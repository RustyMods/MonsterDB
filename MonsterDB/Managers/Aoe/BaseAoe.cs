using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseAoe : Header
{
    [YamlMember(Order = 6)] public AoeRef? AoeData;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type =BaseType.Aoe;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        if (prefab.TryGetComponent(out Aoe component))
        {
            AoeData = new AoeRef(component);
        }
    }

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseAoe originalAoe) return;
        if (AoeData != null && originalAoe.AoeData != null)
        {
            AoeData.ResetTo(originalAoe.AoeData);
        }
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        SaveDefault(prefab);
        if (AoeData != null && prefab.TryGetComponent(out Aoe aoe))
        {
            AoeData.UpdateFields(aoe, prefab.name, true);
        }
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }
    
    protected virtual void SaveDefault(GameObject prefab)
    {
        if (AoeManager.TrySave(prefab, out BaseAoe og, IsCloned, ClonedFrom))
        {
            LoadManager.originalToModifiedList.Add(og, this);
        }
    }
}