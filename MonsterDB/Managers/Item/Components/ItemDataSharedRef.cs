using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public sealed class ItemDataSharedRef : Reference
{
    public string m_prefab = "";
    [YamlConditional(icon = true)] public string? m_name;
    [YamlConditional(icon = true)] public string? m_subtitle;
    [YamlConditional(icon = true)] public ItemDrop.ItemData.ItemType? m_itemType;
    [YamlConditional(icon = true)] public string[]? m_icons;
    [YamlConditional(icon = true)] public ItemDrop.ItemData.ItemType? m_attachOverride;
    [YamlConditional(icon = true)] public string? m_description;
    [YamlConditional(icon = true)][DefaultValue(1)] public int? m_maxStackSize;
    [YamlConditional(icon = true)][DefaultValue(true)] public bool? m_autoStack;
    [YamlConditional(icon = true)][DefaultValue(1)] public int? m_maxQuality;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_scaleByQuality;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_weight;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_scaleWeightByQuality;
    [YamlConditional(icon = true)][DefaultValue(0)] public int? m_value;
    [YamlConditional(icon = true)] public bool? m_teleportable;
    [YamlConditional(icon = true)][DefaultValue(false)] public bool? m_questItem;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_equipDuration;
    [YamlConditional(icon = true)][DefaultValue(0)] public int? m_variants;
    
    [YamlMember(Description = "Set Settings")]
    [YamlConditional(icon = true)][DefaultValue(0)] public int? m_setSize;
    [YamlConditional(icon = true)] public string? m_setStatusEffect;
    
    [YamlConditional(icon = true)] public string? m_equipStatusEffect;

    [YamlMember(Description = "Modifiers")] 
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_eitrRegenModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_movementModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_homeItemsStaminaModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_heatResistanceModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_jumpStaminaModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_attackStaminaModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_blockStaminaModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_dodgeStaminaModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_swimStaminaModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_sneakStaminaModifier;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_runStaminaModifier;

    [YamlMember(Description = "Food Settings")]
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_food;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_foodStamina;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_foodEitr;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float m_foodBurnTime;
    [YamlConditional(icon = true)][DefaultValue(0f)] public float? m_foodRegen;
    [YamlConditional(icon = true)][DefaultValue(false)] public bool? m_isDrink;

    [YamlMember(Description = "Armor Settings")]
    [YamlConditional(icon = true)] public string? m_armorMaterial;
    [YamlConditional(icon = true)] public ItemDrop.ItemData.HelmetHairType? m_helmetHideHair;
    [YamlConditional(icon = true)] public ItemDrop.ItemData.HelmetHairType? m_helmetHideBeard;
    [YamlConditional(icon = true)] public float? m_armor;
    [YamlConditional(icon = true)] public float? m_armorPerLevel;
    [YamlConditional(icon = true)]  public List<HitData.DamageModPair>? m_damageModifiers;
    
    [YamlMember(Description = "Shield Settings")]
    [YamlConditional(icon = true)] public float? m_blockPower;
    [YamlConditional(icon = true)] public float? m_blockPowerPerLevel;
    [YamlConditional(icon = true)] public float? m_deflectionForce;
    [YamlConditional(icon = true)] public float? m_deflectionForcePerLevel;
    [YamlConditional(icon = true)] public float? m_timedBlockBonus;
    [YamlConditional(icon = true)] public string? m_perfectBlockStatusEffect;
    [YamlConditional(icon = true)] public bool? m_buildBlockCharges;
    [YamlConditional(icon = true)] public int? m_maxBlockCharges;
    [YamlConditional(icon = true)] public float? m_blockChargeDecayTime;
    [YamlConditional(icon = true)] public float? m_blockChargeBlockingDecayMult;
    [YamlConditional(icon = true)] public EffectListRef? m_blockChargeEffects;
    [YamlConditional(icon = true)] public float? m_maxAdrenaline;
    [YamlConditional(icon = true)] public string? m_fullAdrenalineSE;
    [YamlConditional(icon = true)] public float? m_blockAdrenaline;
    [YamlConditional(icon = true)] public float? m_perfectBlockAdrenaline;
    
    [YamlMember(Description = "Weapon")] 
    public ItemDrop.ItemData.AnimationState? m_animationState;
    [YamlConditional(icon = true)] public Skills.SkillType? m_skillType;
    [DefaultValue(0)] public int? m_toolTier;
    public HitData.DamageTypes? m_damages;
    [DefaultValue(0f)] public float? m_attackForce;
    [DefaultValue(0f)] public float? m_backstabBonus;
    [DefaultValue(false)] public bool? m_dodgeable;
    [DefaultValue(false)] public bool? m_blockable;
    [YamlConditional] public bool? m_tamedOnly;
    public string? m_attackStatusEffect;
    [DefaultValue(1f)] public float? m_attackStatusEffectChance;
    public string? m_spawnOnHit;
    public string? m_spawnOnHitTerrain;
    public string? m_ammoType;
    public AttackRef? m_attack;
    public AttackRef? m_secondaryAttack;
    [YamlConditional(icon = true)] public float? m_useDurability;
    [YamlConditional(icon = true)] public bool? m_destroyBroken;
    [YamlConditional(icon = true)] public bool? m_canBeRepaired;
    [YamlConditional(icon = true)] public float? m_maxDurability;
    [YamlConditional(icon = true)] public float? m_durabilityPerLevel;
    [YamlConditional(icon = true)] public float m_useDurabilityDrain;
    [YamlMember(Description = "AI behaviour")]
    [DefaultValue(0f)] public float? m_aiAttackRange;
    [DefaultValue(0f)] public float? m_aiAttackRangeMin;
    [DefaultValue(0f)] public float? m_aiAttackInterval;
    [DefaultValue(0f)] public float? m_aiAttackMaxAngle;
    [DefaultValue(false)] public bool? m_aiInvertAngleCheck;
    [DefaultValue(false)] public bool? m_aiWhenFlying;
    [DefaultValue(0f)] public float? m_aiWhenFlyingAltitudeMin;
    [DefaultValue(999999f)] public float? m_aiWhenFlyingAltitudeMax;
    [DefaultValue(false)] public bool? m_aiWhenWalking;
    [DefaultValue(false)] public bool? m_aiWhenSwiming;
    [DefaultValue(false)] public bool? m_aiPrioritized;
    [DefaultValue(false)] public bool? m_aiInDungeonOnly;
    [DefaultValue(false)] public bool? m_aiInMistOnly;
    [DefaultValue(1f)] public float? m_aiMaxHealthPercentage;
    [DefaultValue(0f)]public float? m_aiMinHealthPercentage;
    public ItemDrop.ItemData.AiTarget? m_aiTargetType;
    
    [YamlMember(Description = "Item Effects")]
    public EffectListRef? m_hitEffect;
    public EffectListRef? m_hitTerrainEffect;
    public EffectListRef? m_blockEffect ;
    public EffectListRef? m_startEffect ;
    public EffectListRef? m_holdStartEffect ;
    public EffectListRef? m_triggerEffect ;
    public EffectListRef? m_trailStartEffect ;
    public ItemDataSharedRef(){}
    public ItemDataSharedRef(ItemDrop.ItemData.SharedData sharedData)
    {
        Dictionary<string, FieldInfo> targetFields = GetType()
            .GetFields(FieldBindingFlags)
            .ToDictionary(f => f.Name);
        
        FieldInfo[] sourceFields = sharedData.GetType().GetFields(FieldBindingFlags);

        for (int i = 0; i < sourceFields.Length; ++i)
        {
            FieldInfo sourceField = sourceFields[i];
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField)) continue;

            if (!ShouldSetupField(targetField, sharedData)) continue;
            
            object? sourceValue = sourceField.GetValue(sharedData);
            if (sourceValue == null) continue;
            
            SetupField(sourceField, targetField, sourceValue);
        }
    }

    public bool ShouldSetupField(FieldInfo targetField, ItemDrop.ItemData.SharedData sharedData)
    {
        YamlConditional? conditions = targetField.GetCustomAttribute<YamlConditional>();
        if (conditions == null) return true;
        return conditions.ShouldSetupField(sharedData);
    }

    private static Dictionary<string, Material> _cachedArmorMaterials = new();

    protected override void UpdateMaterial<T>(T target, FieldInfo targetField, string materialName,
        string targetName, bool log)
    {
        if (_cachedArmorMaterials.TryGetValue(materialName, out Material mat))
        {
            targetField.SetValue(target, mat);
        }

        IEnumerable<Material> armorMaterials = PrefabManager.GetAllPrefabs<ItemDrop>()
            .Select(p => p.GetComponent<ItemDrop>().m_itemData.m_shared.m_armorMaterial)
            .Where(m => m != null);
        foreach (Material material in armorMaterials)
        {
            if (material.name == materialName)
            {
                targetField.SetValue(target, material);
            }
            if (_cachedArmorMaterials.ContainsKey(material.name)) continue;
            _cachedArmorMaterials.Add(material.name, material);
        }
        
        var newValue = targetField.GetValue(target);
        var matName = "null";
        if (newValue is Material newMat) matName = newMat.name;
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {matName}");
    }
}

public static partial class Extensions
{
    public static List<string> ToItemNameList(this List<ItemDrop> items)
    {
        List<string> list = items
            .Where(x => x != null)
            .Select(x => x.name)
            .ToList();
        return list;
    }

    public static List<ItemDrop> ToItemDropList(this List<string> il)
    {
        List<ItemDrop> items = il
            .Select(x => PrefabManager.GetPrefab(x)!)
            .Where(x => x != null)
            .Select(x => x.GetComponent<ItemDrop>())
            .Where(x => x != null)
            .ToList();
        return items;
    }

    public static string[] ToSpriteNameArray(this Sprite[] icons)
    {
        return icons.Select(x => x.name).ToArray();
    }

    public static Sprite[] ToSpriteArray(this string[] spriteNames)
    {
        return spriteNames
            .Select(x => TextureManager.GetSprite(x, null)!)
            .Where(x => x != null)
            .ToArray();
    }
}