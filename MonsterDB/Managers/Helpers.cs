using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MonsterDB;

public static class Helpers
{
    public static void CopyFrom<T, V>(this T target, V source)
    {
        Dictionary<string, FieldInfo> targetFields = typeof(T)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ToDictionary(f => f.Name);

        FieldInfo[] sourceFields = typeof(V).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo sourceField in sourceFields)
        {
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField))
                continue;
            if (!targetField.FieldType.IsAssignableFrom(sourceField.FieldType))
                continue;

            targetField.SetValue(target, sourceField.GetValue(source));
        }
    }

    public static void SetFieldsFrom<T, V>(this T target, V source) where V : Reference
    {
        Type T_Type = typeof(T);
        Type V_Type = typeof(V);
        
        MonsterDBPlugin.LogDebug($"Type of T: {T_Type.FullName}, Type of V: {V_Type.FullName}");
        
        Dictionary<string, FieldInfo> targetFields = T_Type
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ToDictionary(f => f.Name);
        
        FieldInfo[] sourceFields = V_Type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        for (int i = 0; i < sourceFields.Length; ++i)
        {
            FieldInfo sourceField = sourceFields[i];
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField)) continue;
            
            object? SValue = sourceField.GetValue(source);
            if (SValue == null) continue;
            
            Type TType = targetField.FieldType;
            Type SType = Nullable.GetUnderlyingType(sourceField.FieldType) ?? sourceField.FieldType;
            
            MonsterDBPlugin.LogDebug($"Changing field: {targetField.Name}");
            if (TType.IsAssignableFrom(SType))
            {
                targetField.SetValue(target, SValue);
            }
            else if (TType == typeof(GameObject) && 
                     SValue is string goName)
            {
                if (string.IsNullOrEmpty(goName))
                {
                    targetField.SetValue(target, null);
                }
                else
                {
                    GameObject? prefab = PrefabManager.GetPrefab(goName);
                    if (prefab != null)
                    {
                        targetField.SetValue(target, prefab);
                    }
                }
            }
            else if (TType == typeof(GameObject[]) && 
                     SValue is string[] goNames)
            {
                targetField.SetValue(target, goNames.FromRef());
            }
            else if (SType == typeof(Humanoid.ItemSet[]) && 
                     SValue is HumanoidRef.ItemSet[] isa)
            {
                targetField.SetValue(target, isa.FromRef());
            }
            else if (TType == typeof(Humanoid.RandomItem[]) && 
                     SValue is HumanoidRef.RandomItem[] ria)
            {
                targetField.SetValue(target, ria.FromRef());
            }
            else if (TType == typeof(EffectList) && 
                     SValue is EffectListRef el)
            {
                targetField.SetValue(target, el.Effects);
            }
            else if (TType == typeof(StatusEffect) && 
                     SValue is string seName)
            {
                StatusEffect? se = PrefabManager.GetStatusEffect(seName);
                targetField.SetValue(target, se);
            }
            else if (TType == typeof(Color) &&
                     SValue is string hex)
            {
                object? TValue = targetField.GetValue(target);
                if (TValue is not Color defaultColor) continue;
                targetField.SetValue(target, hex.FromHex(defaultColor));
            }
            else if (TType == typeof(List<CharacterDrop.Drop>) && 
                     SValue is List<DropRef> dr)
            {
                targetField.SetValue(target, dr.FromRef());
            }
            else if (TType == typeof(List<ItemDrop>) && 
                     SValue is List<string> il)
            {
                targetField.SetValue(target, il.FromRef());
            }
            else if (TType == typeof(List<Growup.GrownEntry>) && 
                     SValue is List<GrowUpRef.GrownEntry> ge)
            {
                targetField.SetValue(target, ge.FromRef());
            }
            else if (TType == typeof(Sprite) && SValue is string sn)
            {
                object? defaultValue = targetField.GetValue(target);
                if (defaultValue is Sprite sprite)
                {
                    targetField.SetValue(target, TextureManager.GetSprite(sn, sprite));
                }
            }
        }
    }
    
    public static void ReferenceFrom<T, V>(this T target, V source) where T : Reference
    {
        Dictionary<string, FieldInfo> targetFields = typeof(T)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ToDictionary(f => f.Name);
        
        FieldInfo[] sourceFields = typeof(V).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (int i = 0; i < sourceFields.Length; ++i)
        {
            FieldInfo sourceField = sourceFields[i];
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField)) continue;

            try
            {
                object? SValue = sourceField.GetValue(source);
                if (SValue == null) continue;
                
                Type TType = targetField.FieldType;
                Type SType = sourceField.FieldType;
                
                if (TType.IsAssignableFrom(SType))
                {
                    targetField.SetValue(target, SValue);
                }
                else switch (SValue)
                {
                    case GameObject go when
                        TType == typeof(string):
                        targetField.SetValue(target, go.name);
                        break;
                    case Sprite sp when 
                        TType == typeof(string):
                        targetField.SetValue(target, sp.name);
                        break;
                    case EffectList el when
                        TType == typeof(EffectListRef):
                    {
                        if (el.m_effectPrefabs != null && el.m_effectPrefabs.Length > 0)
                        {
                            targetField.SetValue(target, el.ToRef());
                        }
                        break;
                    }
                    case List<Growup.GrownEntry> ge when 
                        TType == typeof(List<GrowUpRef.GrownEntry>):
                    {
                        targetField.SetValue(target, ge.ToRef());
                        break;
                    }
                    case GameObject[] goa when
                        TType == typeof(string[]):
                        targetField.SetValue(target, goa.ToRef());
                        break;
                    case Humanoid.ItemSet[] isa when
                        TType == typeof(HumanoidRef.ItemSet[]):
                        targetField.SetValue(target, isa.ToRef());
                        break;
                    case Humanoid.RandomItem[] ria when
                        TType == typeof(HumanoidRef.RandomItem[]):
                        targetField.SetValue(target, ria.ToRef());
                        break;
                    case StatusEffect se when
                        TType == typeof(string):
                        targetField.SetValue(target, se.name);
                        break;
                    case Attack att when
                        TType == typeof(AttackRef):
                        targetField.SetValue(target, att.ToRef());
                        break;
                    case List<ItemDrop> items when 
                        TType == typeof(List<string>):
                        targetField.SetValue(target, items.ToRef());
                        break;
                    case List<LevelEffects.LevelSetup> ls when 
                        TType == typeof(List<LevelSetupRef>):
                        targetField.SetValue(target, ls.ToRef());
                        break;
                    case List<CharacterDrop.Drop> cd when 
                        TType == typeof(List<DropRef>):
                        targetField.SetValue(target, cd.ToRef());
                        break;
                }
            }
            catch (Exception ex)
            {
                MonsterDBPlugin.LogError(ex.Message);
            }
        }
    }
}