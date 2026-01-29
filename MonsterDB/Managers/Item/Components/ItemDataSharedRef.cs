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
    [Condition(icon = true)] public string? m_name;
    [Condition(icon = true)] public string? m_subtitle;
    [Condition(icon = true)] public ItemDrop.ItemData.ItemType? m_itemType;
    [Condition(icon = true)] public string[]? m_icons;
    [Condition(icon = true)] public ItemDrop.ItemData.ItemType? m_attachOverride;
    [Condition(icon = true)] public string? m_description;
    [Condition(icon = true)][DefaultValue(1)] public int? m_maxStackSize;
    [Condition(icon = true)][DefaultValue(true)] public bool? m_autoStack;
    [Condition(icon = true)][DefaultValue(1)] public int? m_maxQuality;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_scaleByQuality;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_weight;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_scaleWeightByQuality;
    [Condition(icon = true)][DefaultValue(0)] public int? m_value;
    [Condition(icon = true)] public bool? m_teleportable;
    [Condition(icon = true)][DefaultValue(false)] public bool? m_questItem;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_equipDuration;
    [Condition(icon = true)][DefaultValue(0)] public int? m_variants;
    
    [YamlMember(Description = "Set Settings")]
    [Condition(icon = true)][DefaultValue(0)] public int? m_setSize;
    [Condition(icon = true)] public string? m_setStatusEffect;
    
    [Condition(icon = true)] public string? m_equipStatusEffect;

    [YamlMember(Description = "Modifiers")] 
    [Condition(icon = true)][DefaultValue(0f)] public float? m_eitrRegenModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_movementModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_homeItemsStaminaModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_heatResistanceModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_jumpStaminaModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_attackStaminaModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_blockStaminaModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_dodgeStaminaModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_swimStaminaModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_sneakStaminaModifier;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_runStaminaModifier;

    [YamlMember(Description = "Food Settings")]
    [Condition(icon = true)][DefaultValue(0f)] public float? m_food;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_foodStamina;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_foodEitr;
    [Condition(icon = true)][DefaultValue(0f)] public float m_foodBurnTime;
    [Condition(icon = true)][DefaultValue(0f)] public float? m_foodRegen;
    [Condition(icon = true)][DefaultValue(false)] public bool? m_isDrink;

    [YamlMember(Description = "Armor Settings")]
    [Condition(icon = true)] public string? m_armorMaterial;
    [Condition(icon = true)] public ItemDrop.ItemData.HelmetHairType? m_helmetHideHair;
    [Condition(icon = true)] public ItemDrop.ItemData.HelmetHairType? m_helmetHideBeard;
    [Condition(icon = true)] public float? m_armor;
    [Condition(icon = true)] public float? m_armorPerLevel;
    [Condition(icon = true)]  public List<HitData.DamageModPair>? m_damageModifiers;
    
    [YamlMember(Description = "Shield Settings")]
    [Condition(icon = true)] public float? m_blockPower;
    [Condition(icon = true)] public float? m_blockPowerPerLevel;
    [Condition(icon = true)] public float? m_deflectionForce;
    [Condition(icon = true)] public float? m_deflectionForcePerLevel;
    [Condition(icon = true)] public float? m_timedBlockBonus;
    [Condition(icon = true)] public string? m_perfectBlockStatusEffect;
    [Condition(icon = true)] public bool? m_buildBlockCharges;
    [Condition(icon = true)] public int? m_maxBlockCharges;
    [Condition(icon = true)] public float? m_blockChargeDecayTime;
    [Condition(icon = true)] public float? m_blockChargeBlockingDecayMult;
    [Condition(icon = true)] public EffectListRef? m_blockChargeEffects;
    [Condition(icon = true)] public float? m_maxAdrenaline;
    [Condition(icon = true)] public string? m_fullAdrenalineSE;
    [Condition(icon = true)] public float? m_blockAdrenaline;
    [Condition(icon = true)] public float? m_perfectBlockAdrenaline;
    
    [YamlMember(Description = "Weapon")] 
    public ItemDrop.ItemData.AnimationState? m_animationState;
    [Condition(icon = true)] public Skills.SkillType? m_skillType;
    [DefaultValue(0)] public int? m_toolTier;
    public HitData.DamageTypes? m_damages;
    [DefaultValue(0f)] public float? m_attackForce;
    [DefaultValue(0f)] public float? m_backstabBonus;
    [DefaultValue(false)] public bool? m_dodgeable;
    [DefaultValue(false)] public bool? m_blockable;
    [Condition] public bool? m_tamedOnly;
    public string? m_attackStatusEffect;
    [DefaultValue(1f)] public float? m_attackStatusEffectChance;
    public string? m_spawnOnHit;
    public string? m_spawnOnHitTerrain;
    public string? m_ammoType;
    public AttackRef? m_attack;
    public AttackRef? m_secondaryAttack;
    [Condition(icon = true)] public float? m_useDurability;
    [Condition(icon = true)] public bool? m_destroyBroken;
    [Condition(icon = true)] public bool? m_canBeRepaired;
    [Condition(icon = true)] public float? m_maxDurability;
    [Condition(icon = true)] public float? m_durabilityPerLevel;
    [Condition(icon = true)] public float m_useDurabilityDrain;
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
    public ItemDataSharedRef(ItemDrop.ItemData.SharedData sharedData) => Setup(sharedData);

    public override bool ShouldSetupField<V>(FieldInfo targetField, V source)
    {
        if (source is ItemDrop.ItemData.SharedData sharedData)
        {
            Condition? conditions = targetField.GetCustomAttribute<Condition>();
            if (conditions == null) return true;
            return conditions.ShouldSetupField(sharedData);
        }

        return true;
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