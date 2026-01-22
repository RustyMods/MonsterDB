using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseProjectile : Header
{
    [YamlMember(Order = 6)] public ProjectileRef? ProjectileData;
    [YamlMember(Order = 7)] public TeleportAbilityRef? TeleportAbility;
    [YamlMember(Order = 8)] public TriggerSpawnAbilityRef? TriggerSpawnAbility;
    [YamlMember(Order = 9)] public VisualRef? Visuals;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.Projectile;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;

        SetupProjectile(prefab);
        SetupTeleportAbility(prefab);
        SetupVisuals(prefab);
    }
    
    public override VisualRef? GetVisualData() => Visuals;

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseProjectile originalProjectile) return;
        if (ProjectileData != null && originalProjectile.ProjectileData != null)  ProjectileData.ResetTo(originalProjectile.ProjectileData);
        if (TeleportAbility != null && originalProjectile.TeleportAbility != null)  TeleportAbility.ResetTo(originalProjectile.TeleportAbility);
        if (TriggerSpawnAbility != null && originalProjectile.TriggerSpawnAbility != null)TriggerSpawnAbility.ResetTo(originalProjectile.TriggerSpawnAbility);
        if (Visuals != null && originalProjectile.Visuals != null) Visuals.ResetTo(originalProjectile.Visuals);
    }

    protected virtual void SetupTeleportAbility(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out TeleportAbility component)) return;
        TeleportAbility = new TeleportAbilityRef(component);
    }

    protected virtual void SetupTriggerSpawnAbility(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out TriggerSpawnAbility component)) return;
        TriggerSpawnAbility = new TriggerSpawnAbilityRef(component);
    }

    protected virtual void SetupProjectile(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out Projectile component)) return;
        ProjectileData = new ProjectileRef(component);
    }
    
    protected virtual void SetupVisuals(GameObject prefab)
    {
        Visuals = new VisualRef(prefab);
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        
        ProjectileManager.TrySave(prefab, out _, IsCloned, ClonedFrom);
        
        UpdatePrefab(prefab);
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }

    protected virtual void UpdatePrefab(GameObject prefab)
    {
        UpdateVisuals(prefab);
        UpdateProjectile(prefab);
        UpdateTeleportAbility(prefab);
        UpdateTriggerSpawnAbility(prefab);
    }

    protected void UpdateTriggerSpawnAbility(GameObject prefab)
    {
        if (TriggerSpawnAbility == null || !TriggerSpawnAbility.m_range.HasValue || !prefab.TryGetComponent(out TriggerSpawnAbility component)) return;
        component.m_range = TriggerSpawnAbility.m_range.Value;
        if (ConfigManager.ShouldLogDetails())
        {
            MonsterDBPlugin.LogDebug($"[{prefab.name}] TriggerSpawnAbility.m_range: {component.m_range}");
        }
    }

    protected void UpdateTeleportAbility(GameObject prefab)
    {
        if (TeleportAbility == null || !prefab.TryGetComponent(out TeleportAbility component)) return;
        TeleportAbility.Update(component, prefab.name);
    }

    protected void UpdateProjectile(GameObject prefab)
    {
        if (ProjectileData == null || !prefab.TryGetComponent(out Projectile component)) return;
        ProjectileData.UpdateFields(component, prefab.name, true);
    }
    
    protected virtual void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.Update(prefab, false, false);
        }
    }
}