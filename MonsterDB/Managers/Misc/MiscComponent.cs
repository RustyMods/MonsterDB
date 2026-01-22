using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MonsterDB.Misc;

[Serializable]
public class MiscComponent : Reference
{
    private static readonly List<Type> HandledTypes = new()
    {
        typeof(Character), typeof(Humanoid), typeof(Human),
        typeof(BaseAI), typeof(MonsterAI), typeof(AnimalAI),
        typeof(ZNetView), typeof(ZSyncTransform), typeof(ZSyncAnimation),
        typeof(FootStep), typeof(DropProjectileOverDistance),
        typeof(ItemDrop.ItemData.SharedData), typeof(Projectile),
        typeof(EggGrow), typeof(MovementDamage), typeof(Tameable),
        typeof(Procreation), typeof(Growup), typeof(NpcTalk),
        typeof(Sadle), typeof(CharacterDrop), typeof(LevelEffects),
        typeof(VisEquipment), typeof(Tail), typeof(RandomAnimation),
        typeof(CharacterTimedDestruction), typeof(CinderSpawner)
    };
    
    public Dictionary<string, List<MiscField>>? components;
    
    private bool IsUseful(FieldInfo field)
    {
        Type t = field.FieldType;

        if (t.IsPrimitive || t.IsEnum || t == typeof(string))
            return true;

        if (t == typeof(GameObject) ||
            t == typeof(EffectList) ||
            t == typeof(StatusEffect) ||
            t.IsArray)
            return true;

        if (t.IsGenericType &&
            t.GetGenericTypeDefinition() == typeof(List<>))
            return true;

        return false;
    }
    public void Setup(GameObject prefab)
    {
        List<MonoBehaviour> c = prefab.GetComponents<MonoBehaviour>().ToList();
        c.RemoveAll(x => HandledTypes.Contains(x.GetType()));
        if (c.Count == 0) return;
        components = new Dictionary<string, List<MiscField>>();
        foreach (MonoBehaviour? component in c)
        {
            Type type = component.GetType();
            string name = type.Name;

            List<FieldInfo> allFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
            allFields.RemoveAll(x => !IsUseful(x));

            List<MiscField> fields = new();
            foreach (FieldInfo field in allFields)
            {
                object? value = field.GetValue(component);
                if (value == null) continue;

                var info = new MiscField();
                info.m_name = field.Name;
                info.m_type = field.FieldType.AssemblyQualifiedName;
                
                if (value.GetType().IsPrimitive || value.GetType().IsEnum || value is string)
                {
                    info.m_value = value;
                }
                switch (value)
                {
                    case GameObject go:
                        info.m_value = go.name;
                        break;
                    case Sprite sp:
                        info.m_value = sp.name;
                        break;
                    case EffectList el when el.HasEffects():
                        info.m_value = el.ToEffectListRef();
                        break;
                    case GameObject[] goa:
                        info.m_value = goa.ToGameObjectNameArray();
                        break;
                    case StatusEffect se:
                        info.m_value = se.name;
                        break;
                    case List<string> ls:
                        info.m_value = ls;
                        break;
                    case string[] sa:
                        info.m_value = sa;
                        break;
                    case Humanoid.ItemSet[] isa:
                        info.m_value = isa.ToHumanoidRefItemSetArray();
                        break;
                    case Humanoid.RandomItem[] ria:
                        info.m_value = ria.ToHumanoidRefRandomItemArray();
                        break;
                }

                fields.Add(info);
            }
            components.Add(name, fields);
        }
    }

    public void Update(GameObject prefab)
    {
        if (components == null || components.Count <= 0) return;

        foreach (KeyValuePair<string, List<MiscField>> kvp in components)
        {
            Dictionary<string, MiscField> dict = kvp.Value.ToDictionary(f => f.m_name);
            
            Type? type = Type.GetType(kvp.Key);
            if (type == null) continue;
            Component? target = prefab.GetComponent(type);
            if (target == null) continue;
            FieldInfo[] fields = type.GetFields();

            foreach (FieldInfo targetField in fields)
            {
                if (!dict.TryGetValue(targetField.Name, out MiscField info)) continue;
                if (info.m_value == null || info.m_type == null) continue;

                Type TType = targetField.FieldType;
                Type? SType = Type.GetType(info.m_type);
                if (SType == null) continue;
                
                if (TType.IsAssignableFrom(SType))
                {
                    targetField.SetValue(target, info.m_value);
                }
                else if (TType == typeof(GameObject) && info.m_value is string goName)
                {
                    if (string.IsNullOrEmpty(goName))
                    {
                        targetField.SetValue(target, null);
                    }
                    else
                    {
                        GameObject? p = PrefabManager.GetPrefab(goName);
                        if (p != null)
                        {
                            targetField.SetValue(target, p);
                        }
                    }
                }
                else if (TType == typeof(GameObject[]) && info.m_value is string[] goNames)
                {
                    targetField.SetValue(target, goNames.ToGameObjectArray());
                }
                else if (TType == typeof(Humanoid.ItemSet[]) && info.m_value is HumanoidRef.ItemSet[] isa)
                {
                    targetField.SetValue(target, isa.ToHumanoidItemSetArray());
                }
                else if (TType == typeof(StatusEffect) && info.m_value is string seName)
                {
                    var se = PrefabManager.GetStatusEffect(seName);
                    if (se != null)
                    {
                        targetField.SetValue(target, se);
                    }
                }
                else if (TType == typeof(Color) && info.m_value is string hex)
                {
                    object? TValue = targetField.GetValue(target);
                    if (TValue is not Color defaultColor) continue;
                    targetField.SetValue(target, hex.FromHexOrRGBA(defaultColor));
                }
                else if (TType == typeof(List<CharacterDrop.Drop>) && 
                         info.m_value is List<DropRef> dr)
                {
                    targetField.SetValue(target, dr.FromRef());
                }
                else if (TType == typeof(List<ItemDrop>) && 
                         info.m_value is List<string> il)
                {
                    targetField.SetValue(target, il.FromRef());
                }
                else if (TType == typeof(List<Growup.GrownEntry>) && 
                         info.m_value is List<GrowUpRef.GrownEntry> ge)
                {
                    targetField.SetValue(target, ge.FromRef());
                }
                else if (TType == typeof(Sprite) && info.m_value is string sn)
                {
                    object? defaultValue = targetField.GetValue(target);
                    if (defaultValue is Sprite sprite)
                    {
                        targetField.SetValue(target, TextureManager.GetSprite(sn, sprite));
                    }
                }
            }
        }
    }
}

[Serializable]
public class MiscField : Reference
{
    public string m_name = "";
    public string? m_type = "";
    public object? m_value;
}