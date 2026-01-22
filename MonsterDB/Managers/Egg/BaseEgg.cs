using System;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class BaseEgg : BaseItem
{
    [YamlMember(Order = 8, Description = "If field removed, will remove component")] public EggGrowRef? EggGrow;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        SetupVersions();
        SetupItem(prefab);
        SetupVisuals(prefab);
        Type = BaseType.Egg;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        if (prefab.TryGetComponent(out EggGrow component))
        {
            EggGrow = new EggGrowRef(component);
        }
    }

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseEgg originalEgg) return;
        if (originalEgg.EggGrow == null) EggGrow = null;
        if (EggGrow != null && originalEgg.EggGrow != null) EggGrow.ResetTo(originalEgg.EggGrow);
    }

    public override void Update()
    {
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }

    protected override void SaveDefault(GameObject prefab)
    {
        EggManager.Save(prefab, IsCloned, ClonedFrom);
    }

    protected override void UpdatePrefab(GameObject prefab)
    {
        UpdateEgg(prefab);
        base.UpdatePrefab(prefab);
    }

    private void UpdateEgg(GameObject prefab)
    {
        EggGrow? component = prefab.GetComponent<EggGrow>();
        if (component == null)
        {
            if (EggGrow != null && !string.IsNullOrEmpty(EggGrow.m_grownPrefab))
            {
                component = prefab.AddComponent<EggGrow>();
                EggManager.RegisterHoverOverride(prefab.name);
            }
        }
        else
        {
            if (EggGrow == null || string.IsNullOrEmpty(EggGrow.m_grownPrefab))
            {
                component = null;
                prefab.Remove<EggGrow>();
            }
        }

        if (component == null) return;

        if (EggGrow != null)
        {
            EggGrow.UpdateFields(component, prefab.name, true);
        }
    }
}