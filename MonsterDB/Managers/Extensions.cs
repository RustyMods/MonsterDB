using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    
    public static void CopyFrom<T, V>(this T target, V source)
    {
        Dictionary<string, FieldInfo> targetFields = typeof(T)
            .GetFields(Reference.FieldBindingFlags)
            .ToDictionary(f => f.Name);

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
}