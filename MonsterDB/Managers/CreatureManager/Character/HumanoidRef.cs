using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class HumanoidRef : CharacterRef
{
    [YamlMember(Order = 100)] public string[]? m_defaultItems;
    [YamlMember(Order = 101)] public string[]? m_randomWeapon;
    [YamlMember(Order = 102)] public string[]? m_randomArmor;
    [YamlMember(Order = 103)] public string[]? m_randomShield;
    [YamlMember(Order = 104)] public ItemSet[]? m_randomSets;
    [YamlMember(Order = 105)] public RandomItem[]? m_randomItems;
    [YamlMember(Order = 106)] public EffectListRef? m_consumeItemEffects;
    [YamlMember(Order = 107)] public EffectListRef? m_equipEffects;
    [YamlMember(Order = 108)] public EffectListRef? m_perfectBlockEffect;
    [YamlMember(Order = 109)] public ItemDataSharedRef[]? m_attacks;
    
    public override ItemDataSharedRef[]? GetAttacks() => m_attacks;

    [Serializable]
    public class ItemSet
    {
        public string m_name = "";
        public string[] m_items = Array.Empty<string>();
    }

    [Serializable]
    public class RandomItem
    {
        public string m_prefab = "";
        public float m_chance = 0.5f;
    }
}

public static partial class Extensions
{
    public static HashSet<GameObject> GetAttacks(this Humanoid humanoid)
    {
        HashSet<GameObject> attacks = new();
        if (humanoid.m_defaultItems != null) attacks.AddRange(humanoid.m_defaultItems);
        if (humanoid.m_randomWeapon != null) attacks.AddRange(humanoid.m_randomWeapon);
        if (humanoid.m_randomSets != null)
            attacks.AddRange(humanoid.m_randomSets
                .SelectMany(x => x.m_items)
                .ToArray());
            
        attacks.RemoveWhere(x => x == null);
        return attacks;
    }

    public static ItemDataSharedRef[] ToRef(this HashSet<GameObject> items)
    {
        List<ItemDataSharedRef> attackRefs = new();
            
        foreach (GameObject? attack in items)
        {
            PrefabManager.AddToCache(attack);
            ItemDrop? itemDrop = attack.GetComponent<ItemDrop>();
            if (itemDrop == null) continue;
            ItemDataSharedRef itemDataRef = new ItemDataSharedRef();
            itemDataRef.ReferenceFrom(itemDrop.m_itemData.m_shared);
            itemDataRef.m_prefab = attack.name;
            attackRefs.Add(itemDataRef);
        }

        return attackRefs.ToArray();
    }
    
    public static HumanoidRef.RandomItem[] ToRef(this Humanoid.RandomItem[] ria)
    {
        HumanoidRef.RandomItem[] randomItems = ria
            .Where(x => x != null && x.m_prefab != null)
            .Select(x => new HumanoidRef.RandomItem()
            {
                m_prefab = x.m_prefab.name,
                m_chance = x.m_chance
            })
            .ToArray();
        return randomItems;
    }

    public static Humanoid.RandomItem[] FromRef(this HumanoidRef.RandomItem[] ria)
    {
        Humanoid.RandomItem[] randomItems = ria
            .Where(x => !string.IsNullOrEmpty(x.m_prefab))
            .Select(x => new Humanoid.RandomItem()
            {
                m_prefab = PrefabManager.GetPrefab(x.m_prefab),
                m_chance = x.m_chance
            })
            .Where(x => x.m_prefab != null)
            .ToArray();
        return randomItems;
    }
    
    public static HumanoidRef.ItemSet[] ToRef(this Humanoid.ItemSet[] isa)
    {
        HumanoidRef.ItemSet[] itemSets = isa
            .Where(x => x != null)
            .Select(x => new HumanoidRef.ItemSet()
            {
                m_name = x.m_name,
                m_items = x.m_items
                    .Where(it => it != null)
                    .Select(it => it.name)
                    .ToArray()
            })
            .ToArray();
        return itemSets;
    }

    public static Humanoid.ItemSet[] FromRef(this HumanoidRef.ItemSet[] isa)
    {
        Humanoid.ItemSet[] itemSets = isa
            .Where(x => x.m_items.Length > 0)
            .Select(x => new Humanoid.ItemSet()
            {
                m_name = x.m_name,
                m_items = x.m_items
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Select(p => PrefabManager.GetPrefab(p)!)
                    .Where(p => p != null)
                    .ToArray()
            })
            .ToArray();
        return itemSets;
    }
    public static string[] ToRef(this GameObject[] goa)
    {
        string[] prefabs = goa
            .Where(x => x != null)
            .Select(x => x.name)
            .ToArray();
        return prefabs;
    }

    public static GameObject[] FromRef(this string[] goNames)
    {
        GameObject[] prefabs = goNames
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => PrefabManager.GetPrefab(x)!)
            .Where(x => x != null)
            .ToArray();
        return prefabs;
    }
}