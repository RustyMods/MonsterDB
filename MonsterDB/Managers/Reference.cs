using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MonsterDB;

public abstract class Reference
{
    public const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    
    public void Setup<V>(V source)
    {
        if (source == null) return;
        
        Dictionary<string, FieldInfo> targetFields = GetType()
            .GetFields(FieldBindingFlags)
            .ToDictionary(f => f.Name);
        
        FieldInfo[] sourceFields = source.GetType().GetFields(FieldBindingFlags);

        for (int i = 0; i < sourceFields.Length; ++i)
        {
            FieldInfo sourceField = sourceFields[i];
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField)) continue;
            object? sourceValue = sourceField.GetValue(source);
            if (sourceValue == null) continue;
            SetupField(sourceField, targetField, sourceValue);
        }
    }

    protected virtual void SetupField(FieldInfo source, FieldInfo target, object value)
    {
        try
        {
            Type targetType = target.FieldType;
            Type sourceType = source.FieldType;
            
            if (targetType.IsAssignableFrom(sourceType))
            {
                target.SetValue(this, value);
            }
            else switch (value)
            {
                case GameObject go when
                    targetType == typeof(string):
                    target.SetValue(this, go.name);
                    break;
                case ItemDrop id when 
                    targetType == typeof(string):
                    target.SetValue(this, id.name);
                    break;
                case Sprite sp when 
                    targetType == typeof(string):
                    target.SetValue(this, sp.name);
                    break;
                case EffectList el when
                    targetType == typeof(EffectListRef):
                {
                    if (el.m_effectPrefabs != null && el.m_effectPrefabs.Length > 0)
                    {
                        target.SetValue(this, el.ToEffectListRef());
                    }
                    break;
                }
                case Sprite[] sa when 
                    targetType == typeof(string[]):
                    target.SetValue(this, sa.ToSpriteNameArray());
                    break;
                case List<Growup.GrownEntry> ge when 
                    targetType == typeof(List<GrowUpRef.GrownEntry>):
                {
                    target.SetValue(this, ge.ToGrowUpRefEntryList());
                    break;
                }
                case GameObject[] goa when
                    targetType == typeof(string[]):
                    target.SetValue(this, goa.ToGameObjectNameArray());
                    break;
                case Humanoid.ItemSet[] isa when
                    targetType == typeof(HumanoidRef.ItemSet[]):
                    target.SetValue(this, isa.ToHumanoidRefItemSetArray());
                    break;
                case Humanoid.RandomItem[] ria when
                    targetType == typeof(HumanoidRef.RandomItem[]):
                    target.SetValue(this, ria.ToHumanoidRefRandomItemArray());
                    break;
                case StatusEffect se when
                    targetType == typeof(string):
                    target.SetValue(this, se.name);
                    break;
                case Attack att when
                    targetType == typeof(AttackRef):
                    if (!string.IsNullOrEmpty(att.m_attackAnimation))
                    {
                        target.SetValue(this, att.ToAttackRef());
                    }
                    break;
                case List<ItemDrop> items when 
                    targetType == typeof(List<string>):
                    target.SetValue(this, items.ToItemNameList());
                    break;
                case List<LevelEffects.LevelSetup> ls when 
                    targetType == typeof(List<LevelSetupRef>):
                    target.SetValue(this, ls.ToRef());
                    break;
                case List<CharacterDrop.Drop> cd when 
                    targetType == typeof(List<DropRef>):
                    target.SetValue(this, cd.ToRef());
                    break;
                case List<DropTable.DropData> dd when 
                    targetType == typeof(List<DropTableRef>):
                    target.SetValue(this, dd.ToRef());
                    break;
                case List<Fish.BaitSetting> bs when 
                    targetType == typeof(List<BaitSettingRef>):
                    target.SetValue(this, bs.ToRef());
                    break;
                case List<RandomAnimation.RandomValue> rv when 
                    targetType == typeof(List<RandomAnimationRef.RandomValueRef>):
                    target.SetValue(this, rv.ToRef());
                    break;
            }
        }
        catch (Exception ex)
        {
            MonsterDBPlugin.LogDebug(ex.Message);
        }
    }
    
    public virtual void UpdateFields<T>(T target, string targetName, bool log)
    {
        if (target == null) return;
        Type targetType = target.GetType();
        Type sourceType = GetType();

        if (string.IsNullOrEmpty(targetName)) targetName = "Unknown";

        if (log)
        {
            MonsterDBPlugin.LogDebug($"[{targetName}] Updating {targetType.FullName}");
        }

        log &= ConfigManager.ShouldLogDetails();
        
        Dictionary<string, FieldInfo> targetFields = targetType
            .GetFields(FieldBindingFlags)
            .ToDictionary(f => f.Name);
        
        FieldInfo[] sourceFields = sourceType.GetFields(FieldBindingFlags);
        
        for (int i = 0; i < sourceFields.Length; ++i)
        {
            FieldInfo sourceField = sourceFields[i];
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField)) continue;
            
            UpdateField(target, sourceField, targetField, targetName, log);
        }
    }

    private void UpdateField<T>(T target, FieldInfo sourceField, FieldInfo targetField, string targetName, bool log)
    {
        object? SValue = sourceField.GetValue(this);
        if (SValue == null) return;
        
        Type TType = targetField.FieldType;
        Type SType = Nullable.GetUnderlyingType(sourceField.FieldType) ?? sourceField.FieldType;
        
        if (string.IsNullOrEmpty(targetName))
        {
            targetName = "Unknown";
        }
        if (TType.IsAssignableFrom(SType))
        {
            targetField.SetValue(target, SValue);
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {SValue}");
        }
        else if (TType == typeof(GameObject) && 
                 SValue is string goName)
        {
            UpdateGameObject(target, targetField, targetName, goName, log);
        }
        else if (TType == typeof(Sprite[]) &&
                 SValue is string[] spn)
        {
            UpdateSpriteArray(target, targetField, spn, targetName, log);
        }
        else if (TType == typeof(Vector3) &&
                 SValue is Vector3Ref vr)
        {
            UpdateVector3(target, targetField, vr, targetName, log);
        }
        else if (TType == typeof(GameObject[]) && 
                 SValue is string[] goNames)
        {
            UpdateGameObjectArray(target, targetField, goNames, targetName, log);
        }
        else if (SType == typeof(Humanoid.ItemSet[]) && 
                 SValue is HumanoidRef.ItemSet[] isa)
        {
            UpdateItemSets(target, targetField, isa, targetName, log);
        }
        else if (TType == typeof(Humanoid.RandomItem[]) && 
                 SValue is HumanoidRef.RandomItem[] ria)
        {
            UpdateRandomItems(target, targetField, ria, targetName, log);
        }
        else if (TType == typeof(EffectList) && 
                 SValue is EffectListRef el)
        {
            UpdateEffectList(target, targetField, el, targetName, log);
        }
        else if (TType == typeof(StatusEffect) && 
                 SValue is string seName)
        {
            UpdateStatusEffect(target, targetField, seName, targetName, log);
        }
        else if (TType == typeof(Color) &&
                 SValue is string hex)
        {
            UpdateColor(target, targetField, hex, targetName, log);
        }
        else if (TType == typeof(List<CharacterDrop.Drop>) && 
                 SValue is List<DropRef> dr)
        {
            UpdateCharacterDrops(target, targetField, dr, targetName, log);
        }
        else if (TType == typeof(List<ItemDrop>) && 
                 SValue is List<string> il)
        {
            UpdateItemList(target, targetField, il, targetName, log);
        }
        else if (TType == typeof(List<Growup.GrownEntry>) && 
                 SValue is List<GrowUpRef.GrownEntry> ge)
        {
            UpdateGrownEntries(target, targetField, ge, targetName, log);
        }
        else if (TType == typeof(Sprite) &&
                 SValue is string sn)
        {
            UpdateSprite(target, targetField, sn, targetName, log);
        }
        else if (TType == typeof(List<DropTable.DropData>) && 
                 SValue is List<DropDataRef> dd)
        {
            UpdateDropTable(target, targetField, dd, targetName, log);
        }
        else if (TType == typeof(List<Fish.BaitSetting>) &&
                 SValue is List<BaitSettingRef> bs)
        {
            UpdateFishBaits(target, targetField, bs, targetName, log);
        }
        else if (TType == typeof(List<RandomAnimation.RandomValue>) &&
                 SValue is List<RandomAnimationRef.RandomValueRef> rv)
        {
            UpdateRandomAnimations(target, targetField, rv, targetName, log);
        }
        else if (TType == typeof(Attack) &&
                 SValue is AttackRef ar)
        {
            UpdateAttack(target, targetField, ar, targetName, log);
        }
        else if (TType == typeof(Aoe) &&
                 SValue is AoeRef aor)
        {
            UpdateAoe(target, targetField, aor, targetName, log);
        }
        else if (TType == typeof(ItemDrop) &&
                 SValue is string idName)
        {
            UpdateItem(target, targetField, idName, targetName, log);
        }
    }

    private void UpdateGameObject<T>(T target, FieldInfo targetField, string targetName, string goName, bool log)
    {
        if (string.IsNullOrEmpty(goName))
        {
            targetField.SetValue(target, null);
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: null");
        }
        else
        {
            GameObject? prefab = PrefabManager.GetPrefab(goName);
            if (prefab != null)
            {
                targetField.SetValue(target, prefab);
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {prefab.name}");
            }
            else
            {
                if (target is MonoBehaviour mono)
                {
                    Transform? possibleChild = Utils.FindChild(mono.transform, goName);
                    if (possibleChild != null)
                    {
                        targetField.SetValue(target, possibleChild.gameObject);
                        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {possibleChild.name} transform");
                    }
                }
            }
        }
    }

    private void UpdateSpriteArray<T>(T target, FieldInfo targetField, string[] spriteNames, string targetName, bool log)
    {
        Sprite[] sprites = spriteNames.ToSpriteArray();
        if (sprites.Length > 0)
        {
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {string.Join(", ", sprites.Select(x => x.name))}");
            targetField.SetValue(target, sprites);
        }
    }

    private void UpdateVector3<T>(T target, FieldInfo targetField, Vector3Ref vector, string targetName, bool log)
    {
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {vector.ToString()}");
        targetField.SetValue(target, vector.ToVector3());
    }

    private void UpdateGameObjectArray<T>(T target, FieldInfo targetField, string[] goNames, string targetName, bool log)
    {
        GameObject[] goArray = goNames.ToGameObjectArray();
        targetField.SetValue(target, goArray);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: GameObject[{goArray.Length}]");
    }

    private void UpdateItemSets<T>(T target, FieldInfo targetField, HumanoidRef.ItemSet[] sets, string targetName, bool log)
    {
        Humanoid.ItemSet[] itemSets = sets.FromRef();
        targetField.SetValue(target, itemSets);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: ItemSet[{itemSets.Length}]");
    }

    private void UpdateRandomItems<T>(T target, FieldInfo targetField, HumanoidRef.RandomItem[] items, string targetName, bool log)
    {
        Humanoid.RandomItem[] randomItems = items.FromRef();
        targetField.SetValue(target, randomItems);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: RandomItem[{randomItems.Length}]");
    }

    private void UpdateEffectList<T>(T target, FieldInfo targetField, EffectListRef el, string targetName, bool log)
    {
        EffectList effectList = el.Effects;
        targetField.SetValue(target, effectList);
        if (effectList.m_effectPrefabs != null)
        {
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: EffectList[{effectList.m_effectPrefabs.Length}]");
        }
    }

    private void UpdateStatusEffect<T>(T target, FieldInfo targetField, string name, string targetName, bool log)
    {
        StatusEffect? se = PrefabManager.GetStatusEffect(name);
        targetField.SetValue(target, se);
        if (se != null)
        {
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {se.name}");
        }
    }

    private void UpdateColor<T>(T target, FieldInfo targetField, string hex,  string targetName, bool log)
    {
        object? targetValue = targetField.GetValue(target);
        if (targetValue is not Color targetColor) return;
        targetField.SetValue(target, hex.FromHex(targetColor));
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {hex}");
    }

    private void UpdateCharacterDrops<T>(T target, FieldInfo targetField, List<DropRef> dropRefs, string targetName, bool log)
    {
        List<CharacterDrop.Drop> drops = dropRefs.FromRef();
        targetField.SetValue(target, drops);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: CharacterDrop[{drops.Count}]");
    }

    private void UpdateItemList<T>(T target, FieldInfo targetField, List<string> itemNames, string  targetName, bool log)
    {
        List<ItemDrop> itemList = itemNames.FromRef();
        targetField.SetValue(target, itemList);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: ItemDrop[{itemList.Count}]");
    }

    private void UpdateGrownEntries<T>(T target, FieldInfo targetField, List<GrowUpRef.GrownEntry> entries, string  targetName, bool log)
    {
        List<Growup.GrownEntry> growEntries = entries.FromRef();
        targetField.SetValue(target, growEntries);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: GrownEntry[{growEntries.Count}]");
    }

    private void UpdateSprite<T>(T target, FieldInfo targetField, string spriteName, string targetName, bool log)
    {
        object? targetValue = targetField.GetValue(target);
        Sprite? targetSprite = targetValue as Sprite;
        Sprite? sprite = TextureManager.GetSprite(spriteName, targetSprite);
        if (sprite != null)
        {
            targetField.SetValue(target, sprite);
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {sprite.name}");
        }
    }

    private void UpdateDropTable<T>(T target, FieldInfo targetField, List<DropDataRef> dropData, string targetName, bool log)
    {
        List<DropTable.DropData> drops = dropData.FromRef();
        targetField.SetValue(target, drops);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: DropTable[{drops.Count}]");
    }

    private void UpdateFishBaits<T>(T target, FieldInfo targetField, List<BaitSettingRef> baitSettings, string targetName, bool log)
    {
        List<Fish.BaitSetting> baits = baitSettings.FromRef();
        targetField.SetValue(target, baits);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: BaitSetting[{baits.Count}]");
    }

    private void UpdateRandomAnimations<T>(T target, FieldInfo targetField, List<RandomAnimationRef.RandomValueRef> rv, string targetName, bool log)
    {
        List<RandomAnimation.RandomValue> randomAnim = rv.FromRef();
        targetField.SetValue(target, randomAnim);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: RandomAnimation[{randomAnim.Count}]");
    }

    private void UpdateAttack<T>(T target, FieldInfo targetField, AttackRef attackRef, string targetName, bool log)
    {
        object? targetValue  = targetField.GetValue(target);
        if (targetValue is Attack attack)
        {
            attackRef.UpdateFields(attack, targetName, log);
        }
    }

    private void UpdateAoe<T>(T target, FieldInfo targetField, AoeRef aoeRef, string targetName, bool log)
    {
        object? targetValue  = targetField.GetValue(target);
        if (targetValue is Aoe aoe)
        {
            aoeRef.UpdateFields(aoe, targetName, log);
        }
    }

    private void UpdateItem<T>(T target, FieldInfo targetField, string name, string targetName, bool log)
    {
        GameObject? prefab = PrefabManager.GetPrefab(name);
        if (prefab != null && prefab.TryGetComponent(out ItemDrop item))
        {
            targetField.SetValue(target, item);
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {name}");
        }
    }
}