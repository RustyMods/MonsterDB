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
    
    public static Dictionary<TKey,SValue> ToDict<TKey, SValue>(this IEnumerable<SValue> enumerable, Func<SValue, TKey> func)
    {
        Dictionary<TKey, SValue> dict = new Dictionary<TKey, SValue>();
        foreach (SValue e in enumerable)
        {
            TKey? k = func(e);
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

    public static bool CanToggleFly(this Character character)
    {
        AnimatorControllerParameter[]? animParams = character.m_animator.parameters;
        for (int i = 0; i < animParams.Length; ++i)
        {
            AnimatorControllerParameter? param = animParams[i];
            if (param.name == "fly_takeoff" || param.name == "fly_land") return true;
        }

        return false;
    }
}