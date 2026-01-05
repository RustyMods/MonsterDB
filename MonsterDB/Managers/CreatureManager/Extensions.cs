using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public static partial class Extensions
{
    public static void Add<T>(this List<T> list, params T[] values) => list.AddRange(values);

    public static void AddRange<T, V>(this Dictionary<T, V> dict, Dictionary<T, V> other)
    {
        foreach (KeyValuePair<T, V> kvp in other)
        {
            dict[kvp.Key] = kvp.Value;
        }
    }
    
    public static void Remove<T>(this GameObject prefab) where T : Component
    {
        if (prefab.TryGetComponent(out T component)) Object.DestroyImmediate(component);
    }

    public static void AddRange<T>(this HashSet<T> set, params T[] values)
    {
        foreach (T value in values)
        {
            set.Add(value);
        }
    }
}