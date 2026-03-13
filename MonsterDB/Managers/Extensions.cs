using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonsterDB;

public static partial class Extensions
{
    public static void Add<T>(this List<T> list, params T[] values) => list.AddRange(values);
    
    public static void CopyFrom<T, V>(this T target, V source) 
        where T : MonoBehaviour 
        where V : MonoBehaviour
    {
        Dictionary<string, FieldInfo> targetFields = typeof(T)
            .GetFields(Reference.FieldBindingFlags)
            .ToSafeDictionary(f => f.Name);

        FieldInfo[] sourceFields = typeof(V).GetFields(Reference.FieldBindingFlags);
        foreach (FieldInfo sourceField in sourceFields)
        {
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField))
                continue;
            if (!targetField.FieldType.IsAssignableFrom(sourceField.FieldType))
                continue;

            targetField.SetValue(target, sourceField.GetValue(source));
        }
    }
    
    public static void Remove<T>(this GameObject prefab) where T : Component
    {
        if (prefab.TryGetComponent(out T component))
        {
            Object.DestroyImmediate(component);
        }
    }

    private static void AddRange<T>(this HashSet<T> set, params T[] values)
    {
        foreach (T value in values)
        {
            set.Add(value);
        }
    }
    
    public static Dictionary<K,V> ToSafeDictionary<K, V>(this IEnumerable<V> enumerable, Func<V, K> func)
    {
        Dictionary<K, V> dict = new Dictionary<K, V>();
        foreach (V e in enumerable)
        {
            K? k = func(e);
            if (dict.ContainsKey(k)) continue;
            dict[k] = e;
        }

        return dict;
    }

    public static List<ItemDrop.ItemData> GetAvailableAttacks(this Humanoid humanoid)
    {
        return humanoid
            .GetInventory()
            .GetAllItems()
            .Where(item => item.IsWeapon() && humanoid.m_baseAI.CanUseAttack(item))
            .ToList();
    }
    
    private static readonly Dictionary<string, bool> canFlyToggleCache = new();

    public static bool CanToggleFly(this Character character)
    {
        string? prefabName = Utils.GetPrefabName(character.name);
        if (canFlyToggleCache.TryGetValue(prefabName, out bool value)) return value;

        bool canToggle = character.m_animator.parameters
            .Any(p => p.name is "fly_takeoff" or "fly_land");

        canFlyToggleCache[prefabName] = canToggle;
        return canToggle;
    }
}