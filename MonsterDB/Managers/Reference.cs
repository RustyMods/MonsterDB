using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MonsterDB;

public abstract class Reference
{
    public const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    protected Dictionary<string, FieldInfo> GetFields()
    {
        return GetType()
            .GetFields(FieldBindingFlags)
            .ToDictionary(f => f.Name);
    }

    public void SetFrom<V>(V source)
    {
        Dictionary<string, FieldInfo> targetFields = GetFields();
        
        FieldInfo[] sourceFields = typeof(V).GetFields(FieldBindingFlags);

        for (int i = 0; i < sourceFields.Length; ++i)
        {
            FieldInfo sourceField = sourceFields[i];
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField)) continue;
            object? SValue = sourceField.GetValue(source);
            if (SValue == null) continue;
            Set(sourceField, targetField, SValue);
        }
    }

    protected virtual void Set(FieldInfo source, FieldInfo target, object value)
    {
        try
        {
            Type TType = target.FieldType;
            Type SType = source.FieldType;
            
            if (TType.IsAssignableFrom(SType))
            {
                target.SetValue(this, value);
            }
            else switch (value)
            {
                case GameObject go when
                    TType == typeof(string):
                    target.SetValue(this, go.name);
                    break;
                case Sprite sp when 
                    TType == typeof(string):
                    target.SetValue(this, sp.name);
                    break;
                case EffectList el when
                    TType == typeof(EffectListRef):
                {
                    if (el.m_effectPrefabs != null && el.m_effectPrefabs.Length > 0)
                    {
                        target.SetValue(this, el.ToRef());
                    }
                    break;
                }
                case List<Growup.GrownEntry> ge when 
                    TType == typeof(List<GrowUpRef.GrownEntry>):
                {
                    target.SetValue(this, ge.ToRef());
                    break;
                }
                case GameObject[] goa when
                    TType == typeof(string[]):
                    target.SetValue(this, goa.ToRef());
                    break;
                case Humanoid.ItemSet[] isa when
                    TType == typeof(HumanoidRef.ItemSet[]):
                    target.SetValue(this, isa.ToRef());
                    break;
                case Humanoid.RandomItem[] ria when
                    TType == typeof(HumanoidRef.RandomItem[]):
                    target.SetValue(this, ria.ToRef());
                    break;
                case StatusEffect se when
                    TType == typeof(string):
                    target.SetValue(this, se.name);
                    break;
                case Attack att when
                    TType == typeof(AttackRef):
                    if (!string.IsNullOrEmpty(att.m_attackAnimation))
                    {
                        target.SetValue(this, att.ToRef());
                    }
                    break;
                case List<ItemDrop> items when 
                    TType == typeof(List<string>):
                    target.SetValue(this, items.ToRef());
                    break;
                case List<LevelEffects.LevelSetup> ls when 
                    TType == typeof(List<LevelSetupRef>):
                    target.SetValue(this, ls.ToRef());
                    break;
                case List<CharacterDrop.Drop> cd when 
                    TType == typeof(List<DropRef>):
                    target.SetValue(this, cd.ToRef());
                    break;
                case List<DropTable.DropData> dd when 
                    TType == typeof(List<DropTableRef>):
                    target.SetValue(this, dd.ToRef());
                    break;
                case List<Fish.BaitSetting> bs when 
                    TType == typeof(List<BaitSettingRef>):
                    target.SetValue(this, bs.ToRef());
                    break;
                case List<RandomAnimation.RandomValue> rv when 
                    TType == typeof(List<RandomAnimationRef.RandomValueRef>):
                    target.SetValue(this, rv.ToRef());
                    break;
            }
        }
        catch (Exception ex)
        {
            MonsterDBPlugin.LogDebug(ex.Message);
        }
    }
}

public static partial class Extensions
{
    public static void SetFieldsFrom<T, V>(this T target, V source, bool log = true) where V : Reference
    {
        Type T_Type = typeof(T);
        Type V_Type = typeof(V);
        
        if (log) MonsterDBPlugin.LogDebug($"Type of T: {T_Type.FullName}, Type of V: {V_Type.FullName}");
        
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
            
            if (log) MonsterDBPlugin.LogDebug($"Changing field: {targetField.Name}");
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
                targetField.SetValue(target, goNames.ToGameObjectArray());
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
            else if (TType == typeof(Sprite) &&
                     SValue is string sn)
            {
                object? defaultValue = targetField.GetValue(target);
                if (defaultValue is Sprite sprite)
                {
                    targetField.SetValue(target, TextureManager.GetSprite(sn, sprite));
                }
            }
            else if (TType == typeof(List<DropTable.DropData>) && 
                     SValue is List<DropDataRef> dd)
            {
                targetField.SetValue(target, dd.FromRef());
            }
            else if (TType == typeof(List<Fish.BaitSetting>) &&
                     SValue is List<BaitSettingRef> bs)
            {
                targetField.SetValue(target, bs.FromRef());
            }
            else if (TType == typeof(List<RandomAnimation.RandomValue>) &&
                     SValue is List<RandomAnimationRef.RandomValueRef> rv)
            {
                targetField.SetValue(target,rv.FromRef());
            }
        }
    }
}