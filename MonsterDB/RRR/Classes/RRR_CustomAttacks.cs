using System;
using System.Collections.Generic;

namespace MonsterDB;

[Serializable]
public class RRR_CustomAttacks
{
    public string? sOriginalPrefabName ;
    public string? sTargetAnim ;
    public float? fAIAttackInterval ;
    public bool? bAIPrioritized ;
    public RRR_Damages? dtAttackDamageOverride ;
    public float? fAttackDamageTotalOverride ;
    public string? sAttackProjectileOverride ;
    public List<string>? aStartEffects ;
    public List<string>? aHitEffects ;
    public List<string>? aTriggerEffects ;

    public bool Convert(string prefabId, out BaseItem item)
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        item = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        if (sOriginalPrefabName == null || string.IsNullOrEmpty(sOriginalPrefabName)) return false;
        var prefab = PrefabManager.GetPrefab(sOriginalPrefabName);
        if (prefab == null) return false;
        if (!ItemManager.TrySave(prefab, out item)) return false;
        item.IsCloned = true;
        item.ClonedFrom = prefab.name;
        item.Prefab = $"{prefabId}_{prefab.name}";
        if (item.ItemData != null && item.ItemData.m_attack != null)
        {
            item.ItemData.m_attack.m_attackAnimation = sTargetAnim;
            item.ItemData.m_aiAttackInterval = fAIAttackInterval;
            item.ItemData.m_aiPrioritized = bAIPrioritized;
            if (dtAttackDamageOverride != null) item.ItemData.m_damages = dtAttackDamageOverride.ToDamages();
            if (fAttackDamageTotalOverride != null && item.ItemData.m_damages.HasValue)
            {
                HitData.DamageTypes damages = item.ItemData.m_damages.Value;
                damages.Modify(fAttackDamageTotalOverride.Value);
                item.ItemData.m_damages = damages;
            }
            item.ItemData.m_attack.m_attackProjectile = sAttackProjectileOverride;
            if (aStartEffects != null) item.ItemData.m_startEffect = new EffectListRef(aStartEffects.ToArray());
            if (aHitEffects != null) item.ItemData.m_hitEffect = new EffectListRef(aHitEffects.ToArray());
            if (aTriggerEffects != null) item.ItemData.m_triggerEffect = new EffectListRef(aTriggerEffects.ToArray());
        }
        return true;
    }
}