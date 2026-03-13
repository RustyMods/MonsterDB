using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public abstract class Reference
{
    public const BindingFlags FieldBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Resets any fields with values to original value
    /// </summary>
    /// <param name="source"></param>
    public void ResetTo(Reference source)
    {
        if (source.GetType() != GetType())
        {
            MonsterDBPlugin.LogWarning($"[ResetTo] Type mismatch: target={GetType().Name} != source={source.GetType().Name}");
            return;
        }
        
        FieldInfo[] fields = GetType().GetFields(FieldBindingFlags);
        
        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            
            object? currentValue = field.GetValue(this);
            if (currentValue == null) continue;
            object? sourceValue = field.GetValue(source);
            if (sourceValue == null) continue;

            if (currentValue is Reference reference && 
                sourceValue is Reference otherReference)
            {
                reference.ResetTo(otherReference);
            }
            else if (currentValue is RendererRef[] rendererArray && sourceValue is RendererRef[] otherRendererArray)
            {
                Dictionary<(string m_prefab, string? m_parent, int? m_index), RendererRef> exactMatchLookup = otherRendererArray
                        .GroupBy(x => (x.m_prefab, x.m_parent, x.m_index))
                        .ToDictionary(x => x.Key, x => x.First());
                
                for (int y = 0; y < rendererArray.Length; ++y)
                {
                    RendererRef currentRenderer = rendererArray[y];
                    (string m_prefab, string? m_parent, int? m_index) key = (currentRenderer.m_prefab, currentRenderer.m_parent, currentRenderer.m_index);
                    if (exactMatchLookup.TryGetValue(key, out RendererRef otherRenderer))
                    {
                        currentRenderer.ResetTo(otherRenderer);
                    }
                }
            }
            else if (currentValue is LightRef[] lightArray && sourceValue is LightRef[] otherLightArray)
            {
                
                Dictionary<(string m_prefab, string? m_parent, int? m_index), LightRef> exactMatchLookup = otherLightArray
                    .GroupBy(x => (x.m_prefab, x.m_parent, x.m_index))
                    .ToDictionary(x => x.Key, x => x.First());
                
                for (int y = 0; y < lightArray.Length; ++y)
                {
                    LightRef currentLight = lightArray[y];
                    (string m_prefab, string? m_parent, int? m_index) key = (currentLight.m_prefab, currentLight.m_parent, currentLight.m_index);
                    if (exactMatchLookup.TryGetValue(key, out LightRef otherLight))
                    {
                        currentLight.ResetTo(otherLight);
                    }
                }
            }
            else if (currentValue is ParticleSystemRef[] particleSystemArray &&
                     sourceValue is ParticleSystemRef[] otherParticleSystemArray)
            {
                Dictionary<(string m_prefab, string? m_parent, int? m_index), ParticleSystemRef> exactMatchLookup = otherParticleSystemArray
                    .GroupBy(x => (x.m_prefab, x.m_parent, x.m_index))
                    .ToDictionary(x => x.Key, x => x.First());
                
                for (int y = 0; y < particleSystemArray.Length; ++y)
                {
                    ParticleSystemRef currentParticleSystem = particleSystemArray[y];
                    (string m_prefab, string? m_parent, int? m_index) key = (currentParticleSystem.m_prefab, currentParticleSystem.m_parent, currentParticleSystem.m_index);
                    if (exactMatchLookup.TryGetValue(key, out ParticleSystemRef otherParticleSystem))
                    {
                        currentParticleSystem.ResetTo(otherParticleSystem);
                    }
                }
            }
            else
            {
                field.SetValue(this, sourceValue);
            }
        }
    }
    
    /// <summary>
    /// this reference will compare against target reference, if values match, will set to null
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetName"></param>
    /// <param name="log"></param>
    /// <returns>number of fields set to null</returns>
    public int Clean(Reference target, string targetName, bool log)
    {
        int count = 0;
        if (target.GetType() != GetType())
        {
            MonsterDBPlugin.LogWarning($"[Clean] Type mismatch: target={target.GetType().Name} != source={GetType().Name}");
            return count;
        }
        if (string.IsNullOrEmpty(targetName)) targetName = "Unknown";
        FieldInfo[] fields = GetType().GetFields(FieldBindingFlags);
        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            object? t_value = field.GetValue(target);
            object? m_value = field.GetValue(this);

            if (t_value is TransformRef t_transform && m_value is TransformRef m_transform)
            {
                if (t_transform.m_position == m_transform.m_position) continue;
                if (!t_transform.m_position.HasValue || !m_transform.m_position.HasValue) continue;
                if (t_transform.m_position.Value.x.Equals(m_transform.m_position.Value.x)) continue;
                if (t_transform.m_position.Value.y.Equals(m_transform.m_position.Value.y)) continue;
                if (t_transform.m_position.Value.z.Equals(m_transform.m_position.Value.z)) continue;
                t_transform.m_position = null;
                ++count;
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{targetName}] {field.Name}.m_position: [ CLEAN ]");
                }
            }
            else if (t_value is Reference tr && m_value is Reference mr)
            {
                count += mr.Clean(tr, targetName, log);
            }
            else
            {
                if (m_value == null || t_value == null) continue;
                if (field.GetCustomAttribute<Persistent>() != null) continue;
                if (CleanField(target, field, m_value, t_value, targetName, log))
                {
                    ++count;
                }
            }
        }

        return count;
    }

    private bool CleanField(Reference target, FieldInfo field, object originalValue, object targetValue, string targetName, bool log)
    {
        Type type = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
                
        if (type.IsArray)
        {
            if (originalValue is not Array m_array || targetValue is not Array t_array) return false;
            if (m_array.Length != t_array.Length) return false;
            
            if (m_array.Length > 0)
            {
                if (m_array.GetValue(0) is Reference)
                {
                    for (int i = 0; i < m_array.Length; ++i)
                    {
                        Reference? m_arrayValue = m_array.GetValue(i) as Reference;
                        Reference? t_arrayValue = t_array.GetValue(i) as Reference;
                        if (m_arrayValue == null || t_arrayValue == null) continue;
                        m_arrayValue.Clean(t_arrayValue, targetName, log);
                    }
                }
                else
                {
                    bool isSame = true;
                    for (int j = 0; j < m_array.Length - 1; ++j)
                    {
                        object? m_arrayValue = m_array.GetValue(j);
                        object? t_arrayValue = t_array.GetValue(j);
                        
                        if (t_arrayValue != m_arrayValue)
                        {
                            isSame = false;
                            break;
                        }
                    }

                    if (isSame)
                    {
                        field.SetValue(target, null);
                        if (log)
                        {
                            MonsterDBPlugin.LogDebug($"[{targetName}] {field.Name}");
                        }

                        return true;
                    }
                }
            }
        }
        else if (targetValue is EffectListRef t_effects && originalValue is EffectListRef m_effects)
        {
            List<string> t_effectNames = t_effects.m_effectPrefabs.Select(t => t.m_prefab).ToList();
            List<string> m_effectNames = m_effects.m_effectPrefabs.Select(m => m.m_prefab).ToList();
            if (t_effectNames.Count != m_effectNames.Count) return false;
            bool isSame = true;
            for (int j = 0; j < t_effectNames.Count; ++j)
            {
                string? t_name = t_effectNames[j];
                string? m_name = m_effectNames[j];
                if (t_name.Equals(m_name)) continue;
                isSame = false;
                break;
            }

            if (isSame)
            {
                field.SetValue(target, null);
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{targetName}] {field.Name}");
                }

                return true;
            }
        }
        else if (targetValue is List<StepEffectRef> t_steps && originalValue is List<StepEffectRef> m_steps)
        {
            if (t_steps.Count != m_steps.Count) return false;
            bool isSame = true;
            for (int i = 0; i < m_steps.Count; ++i)
            {
                StepEffectRef t_step = t_steps[i];
                StepEffectRef m_step = m_steps[i];
                if (t_step.m_effectPrefabs == null || m_step.m_effectPrefabs == null) continue;
                if (t_step.m_effectPrefabs.Length != m_step.m_effectPrefabs.Length) continue;
                
                for (int j = 0; j < m_step.m_effectPrefabs.Length; ++j)
                {
                    var t_effectName = t_step.m_effectPrefabs[j];
                    var m_effectName = m_step.m_effectPrefabs[j];
                    if (t_effectName.Equals(m_effectName)) continue;
                    isSame = false;
                    break;
                }

                if (!isSame) break;

                if (t_step.m_name.Equals(m_step.m_name) && t_step.m_motionType.Equals(m_step.m_motionType) &&
                    t_step.m_material.Equals(m_step.m_material)) continue;
                
                isSame = false;
                break;
            }

            if (isSame)
            {
                field.SetValue(target, null);
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{targetName}] {field.Name}");
                }

                return true;
            }
        }
        else if (targetValue is List<LevelSetupRef> t_lvls && originalValue is List<LevelSetupRef> m_lvls)
        {
            if (t_lvls.Count != m_lvls.Count) return false;
            bool isSame = true;
            for (int i = 0; i < m_lvls.Count; ++i)
            {
                LevelSetupRef t_lvl = m_lvls[i];
                LevelSetupRef m_lvl = m_lvls[i];
                if (t_lvl.Equals(m_lvl)) continue;
                isSame = false;
                break;
            }

            if (isSame)
            {
                field.SetValue(target, null);
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{targetName}] {field.Name}");
                }

                return true;
            }
        }
        else
        {
            if (targetValue.Equals(originalValue))
            {
                field.SetValue(target, null);
                if (log)
                {
                    MonsterDBPlugin.LogDebug($"[{targetName}] {field.Name}");
                }

                return true;
            }

            if (targetValue is float t_float && originalValue is float m_float)
            {
                if (Mathf.Approximately(t_float, m_float))
                {
                    field.SetValue(target, null);
                    if (log)
                    {
                        MonsterDBPlugin.LogDebug($"[{targetName}] {field.Name}");
                    }

                    return true;
                }
            }
        }

        return false;
    }

    public virtual bool Equals<T>(T other) where T : Reference => other == this;

    public virtual bool IsNull()
    {
        FieldInfo[] fields = GetType().GetFields(FieldBindingFlags);
        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];
            object? value = field.GetValue(this);
            if (value != null) return false;
        }
        return true;
    }
    /// <summary>
    /// Grabs all values that matches reference fields and sets value
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="V"></typeparam>
    public void Setup<V>(V source)
    {
        if (source == null) return;
        
        Dictionary<string, FieldInfo> targetFields = GetType()
            .GetFields(FieldBindingFlags)
            .ToSafeDictionary(f => f.Name);
        
        FieldInfo[] sourceFields = source.GetType().GetFields(FieldBindingFlags);

        for (int i = 0; i < sourceFields.Length; ++i)
        {
            FieldInfo sourceField = sourceFields[i];
            if (!targetFields.TryGetValue(sourceField.Name, out FieldInfo targetField)) continue;
            if (!ShouldSetupField(targetField, source)) continue;
            object? sourceValue = sourceField.GetValue(source);
            if (sourceValue == null) continue;
            SetupField(sourceField, targetField, sourceValue);
        }
    }

    protected virtual void SetupField(FieldInfo source, FieldInfo target, object value)
    {
        try
        {
            Type targetType = Nullable.GetUnderlyingType(target.FieldType) ?? target.FieldType;
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
                    target.SetValue(this, ls.ToLevelSetupRefList());
                    break;
                case List<CharacterDrop.Drop> cd when 
                    targetType == typeof(List<DropRef>):
                    target.SetValue(this, cd.ToRef());
                    break;
                case List<DropTable.DropData> dd when 
                    targetType == typeof(List<DropTableRef>):
                    target.SetValue(this, dd.ToDropDataRefList());
                    break;
                case List<Fish.BaitSetting> bs when 
                    targetType == typeof(List<BaitSettingRef>):
                    target.SetValue(this, bs.ToBaitSettingsRefList());
                    break;
                case List<RandomAnimation.RandomValue> rv when 
                    targetType == typeof(List<RandomAnimationRef.RandomValueRef>):
                    target.SetValue(this, rv.ToRef());
                    break;
                case List<SpawnSystem.SpawnData> sps when
                    targetType == typeof(List<SpawnDataRef>):
                    target.SetValue(this, sps.ToSpawnDataRefList());
                    break;
                case Material mat when
                    targetType == typeof(string):
                    target.SetValue(this, mat.name);
                    break;
                case List<SpawnArea.SpawnData> spawns when
                    targetType == typeof(List<SpawnAreaRef.SpawnDataRef>):
                    target.SetValue(this, spawns.ToSpawnDataRefList());
                    break;
                case List<FootStep.StepEffect> stepEffects when
                    targetType == typeof(List<StepEffectRef>):
                    target.SetValue(this, stepEffects.ToStepEffectRefList());
                    break;
            }
        }
        catch (Exception ex)
        {
            MonsterDBPlugin.LogDebug(ex.Message);
        }
    }

    public virtual bool ShouldSetupField<V>(FieldInfo targetField, V source) => true;
    
    public virtual void UpdateFields<T>(T target, string targetName, bool log)
    {
        if (target == null) return;
        Type targetType = target.GetType();
        Type sourceType = GetType();

        if (string.IsNullOrEmpty(targetName)) targetName = "Unknown";

        if (log)
        {
            if (LoadManager.resetting)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}] Resetting {targetType.FullName}");
            }
            else
            {
                MonsterDBPlugin.LogDebug($"[{targetName}] Updating {targetType.FullName}");
            }
        }

        log &= ConfigManager.ShouldLogDetails();
        
        Dictionary<string, FieldInfo> targetFields = targetType
            .GetFields(FieldBindingFlags)
            .ToSafeDictionary(f => f.Name);
        
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
        object? newValue = sourceField.GetValue(this);
        if (newValue == null) return;
        
        Type targetType = targetField.FieldType;
        Type sourceType = Nullable.GetUnderlyingType(sourceField.FieldType) ?? sourceField.FieldType;
        
        if (string.IsNullOrEmpty(targetName)) targetName = "Unknown";
        
        if (targetType.IsAssignableFrom(sourceType))
        {
            UpdateAssignableType(target, targetField, targetType, newValue, targetName, log);
        }
        else if (targetType == typeof(GameObject) && 
                 newValue is string goName)
        {
            UpdateGameObject(target, targetField, targetName, goName, log);
        }
        else if (targetType == typeof(Sprite[]) &&
                 newValue is string[] spn)
        {
            UpdateSpriteArray(target, targetField, spn, targetName, log);
        }
        else if (targetType == typeof(Vector3) &&
                 newValue is Vector3Ref vr)
        {
            UpdateVector3(target, targetField, vr, targetName, log);
        }
        else if (targetType == typeof(GameObject[]) && 
                 newValue is string[] goNames)
        {
            UpdateGameObjectArray(target, targetField, goNames, targetName, log);
        }
        else if (targetType == typeof(Humanoid.ItemSet[]) && 
                 newValue is HumanoidRef.ItemSet[] isa)
        {
            UpdateItemSets(target, targetField, isa, targetName, log);
        }
        else if (targetType == typeof(Humanoid.RandomItem[]) && 
                 newValue is HumanoidRef.RandomItem[] ria)
        {
            UpdateRandomItems(target, targetField, ria, targetName, log);
        }
        else if (targetType == typeof(EffectList) && 
                 newValue is EffectListRef el)
        {
            UpdateEffectList(target, targetField, el, targetName, log);
        }
        else if (targetType == typeof(StatusEffect) && 
                 newValue is string seName)
        {
            UpdateStatusEffect(target, targetField, seName, targetName, log);
        }
        else if (targetType == typeof(Color) &&
                 newValue is string hex)
        {
            UpdateColor(target, targetField, hex, targetName, log);
        }
        else if (targetType == typeof(List<CharacterDrop.Drop>) && 
                 newValue is List<DropRef> dr)
        {
            UpdateCharacterDrops(target, targetField, dr, targetName, log);
        }
        else if (targetType == typeof(List<ItemDrop>) && 
                 newValue is List<string> il)
        {
            UpdateItemList(target, targetField, il, targetName, log);
        }
        else if (targetType == typeof(List<Growup.GrownEntry>) && 
                 newValue is List<GrowUpRef.GrownEntry> ge)
        {
            UpdateGrownEntries(target, targetField, ge, targetName, log);
        }
        else if (targetType == typeof(Sprite) &&
                 newValue is string sn)
        {
            UpdateSprite(target, targetField, sn, targetName, log);
        }
        else if (targetType == typeof(List<DropTable.DropData>) && 
                 newValue is List<DropDataRef> dd)
        {
            UpdateDropTable(target, targetField, dd, targetName, log);
        }
        else if (targetType == typeof(List<Fish.BaitSetting>) &&
                 newValue is List<BaitSettingRef> bs)
        {
            UpdateFishBaits(target, targetField, bs, targetName, log);
        }
        else if (targetType == typeof(List<RandomAnimation.RandomValue>) &&
                 newValue is List<RandomAnimationRef.RandomValueRef> rv)
        {
            UpdateRandomAnimations(target, targetField, rv, targetName, log);
        }
        else if (targetType == typeof(Attack) &&
                 newValue is AttackRef ar)
        {
            UpdateAttack(target, targetField, ar, targetName, log);
        }
        else if (targetType == typeof(Aoe) &&
                 newValue is AoeRef aor)
        {
            UpdateAoe(target, targetField, aor, targetName, log);
        }
        else if (targetType == typeof(ItemDrop) &&
                 newValue is string idName)
        {
            UpdateItem(target, targetField, idName, targetName, log);
        }
        else if (targetType == typeof(List<SpawnSystem.SpawnData>) &&
                 newValue is List<SpawnDataRef> sdr)
        {
            UpdateSpawnDataList(target, targetField, sdr, targetName, log);
        }
        else if (targetType == typeof(DropTable) &&
                 newValue is DropTableRef dt)
        {
            UpdateDropTable(target, targetField, dt, targetName, log);
        }
        else if (targetType == typeof(Material) && 
                 newValue is string matName)
        {
            UpdateMaterial(target, targetField, matName, targetName, log);
        }
        else if (targetType == typeof(List<SpawnArea.SpawnData>) &&
                 newValue is List<SpawnAreaRef.SpawnDataRef> spawns)
        {
            UpdateSpawnDataList(target, targetField, spawns, targetName, log);
        }
        else if (targetType == typeof(Piece.Requirement[]) &&
                 newValue is RequirementRef[] requirements)
        {
            UpdateRequirements(target, targetField, requirements, targetName, log);
        }
        else if (targetType == typeof(CraftingStation) &&
                 newValue is string craftingStationName)
        {
            UpdateCraftingStation(target, targetField, craftingStationName, targetName, log);
        }
        else if (targetType == typeof(List<FootStep.StepEffect>) &&
                 newValue is List<StepEffectRef> stepEffects)
        {
            UpdateStepEffect(target, targetField, stepEffects, targetName, log);
        }
    }

    protected virtual void UpdateAssignableType<T>(
        T target, 
        FieldInfo targetField, 
        Type targetType, 
        object value, 
        string targetName, 
        bool log)
    {
        if (target is Character character && character.name.Contains("(Clone)") && targetField.Name == "m_boss")
        {
            MonsterDBPlugin.LogDebug("Skipping m_boss, prefab is an instance");
            return;
        }
        
        targetField.SetValue(target, value);
        if (log)
        {
            if (targetType.IsArray)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {targetType.Name}[{((Array)value).Length}]");
            }
            else if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listType = targetType.GetGenericArguments()[0];
                int count = ((System.Collections.ICollection)value).Count;
                MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {listType.Name}[{count}]");
            }
            else if (targetType == typeof(HitData.DamageModifiers))
            {
                HitData.DamageModifiers modifiers = (HitData.DamageModifiers)value;
                FieldInfo[] fields = modifiers.GetType().GetFields();
                IEnumerable<string> parts = fields.Select(f => $"{f.Name}={f.GetValue(modifiers)}");
                MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {string.Join(", ", parts)}");
            }
            else
            {
                MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {value}");
            }
        }
    }

    protected virtual void UpdateMaterial<T>(
        T target, 
        FieldInfo targetField, 
        string materialName,
        string targetName, 
        bool log)
    {
        if (string.IsNullOrEmpty(materialName))
        {
            targetField.SetValue(target, null);
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: null");
        }
        else if (MaterialManager.TryGetMaterial(materialName, out Material material))
        {
            targetField.SetValue(target, material);
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {material.name}");
        }
        else
        {
            MonsterDBPlugin.LogWarning($"Failed to find material: {materialName}");
        }
    }

    public static void UpdateCraftingStation<T>(T target, FieldInfo targetField, string name, string targetName,
        bool log)
    {
        var prefab = PrefabManager.GetPrefab(name);
        if (prefab == null) return;
        if (!prefab.TryGetComponent(out CraftingStation craftingStation))
        {
            MonsterDBPlugin.LogWarning($"[{targetName}] {targetField.Name}: {name} missing CraftingStation component");
            return;
        }
        targetField.SetValue(target, craftingStation);
        if (log)
        {
            MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {name}");
        }
    }

    public void UpdateStepEffect<T>(T target, FieldInfo targetField, List<StepEffectRef> effects, string targetName,
        bool log)
    {
        if (targetField.GetValue(target) is not List<FootStep.StepEffect> defaultValues) return;
        List<FootStep.StepEffect> stepEffects = effects.ToFootStepEffects(defaultValues);
        targetField.SetValue(target, stepEffects);
        if (log)
        {
            MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: FootStep.StepEffect[{stepEffects.Count}]");
            for (int i = 0; i < stepEffects.Count; ++i)
            {
                FootStep.StepEffect? effect = stepEffects[i];
                string joined = string.Join(", ", effect.m_effectPrefabs.Select(e => e.name));
                MonsterDBPlugin.LogDebug($"[{targetName}][{targetField.Name}] {effect.m_name}: {joined}");
            }
        }
    }
    protected void UpdateRequirements<T>(T target, FieldInfo targetField, RequirementRef[] requirements,
        string targetName, bool log)
    {
        Piece.Requirement[] reqs = requirements.ToPieceRequirements();
        targetField.SetValue(target, reqs);
        if (log)
        {
            MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: Piece.Requirements[{reqs.Length}]");
        }
    }
    protected virtual void UpdateDropTable<T>(T target, FieldInfo targetField, DropTableRef dt, string targetName,
        bool log)
    {
        DropTable table = dt.ToDropTable(targetName);
        targetField.SetValue(target, table);
    }

    protected virtual void UpdateGameObject<T>(T target, FieldInfo targetField, string targetName, string goName, bool log)
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
        }
    }

    private void UpdateSpawnDataList<T>(T target, FieldInfo targetField, List<SpawnDataRef> r, string targetName, bool log)
    {
        log &= ConfigManager.ShouldLogDetails();
        if (string.IsNullOrEmpty(targetName)) targetName = "Unknown";
        List<SpawnSystem.SpawnData> list = new List<SpawnSystem.SpawnData>();
        for (int i = 0; i < r.Count; ++i)
        {
            SpawnDataRef? reference = r[i];
            SpawnSystem.SpawnData data = new SpawnSystem.SpawnData();
            reference.UpdateFields(data, reference.m_name, false);
            if (data.m_prefab == null) continue;
            list.Add(data);
        }
        targetField.SetValue(target, list);
        if (log)
        {
            MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: SpawnSystem.SpawnData[{list.Count}]");
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

    private void UpdateSpawnDataList<T>(T target, FieldInfo targetField, List<SpawnAreaRef.SpawnDataRef> spawns,
        string targetName, bool log)
    {
        var output = spawns.ToSpawnAreaSpawnDataList();
        targetField.SetValue(target, output);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: SpawnArea.SpawnData[{spawns.Count}]");
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
        Humanoid.ItemSet[] itemSets = sets.ToHumanoidItemSetArray();
        targetField.SetValue(target, itemSets);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: ItemSet[{itemSets.Length}]");
    }

    private void UpdateRandomItems<T>(T target, FieldInfo targetField, HumanoidRef.RandomItem[] items, string targetName, bool log)
    {
        Humanoid.RandomItem[] randomItems = items.ToHumanoidRandomItemArray();
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
        object? currentValue = targetField.GetValue(target);
        if (currentValue is not Color currentColor) return;
        Color color = hex.FromHexOrRGBA(currentColor);
        targetField.SetValue(target, color);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {color.ToString()}");
    }

    private void UpdateCharacterDrops<T>(T target, FieldInfo targetField, List<DropRef> dropRefs, string targetName, bool log)
    {
        List<CharacterDrop.Drop> drops = dropRefs.FromRef();
        targetField.SetValue(target, drops);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: CharacterDrop[{drops.Count}]");
    }

    private void UpdateItemList<T>(T target, FieldInfo targetField, List<string> itemNames, string  targetName, bool log)
    {
        List<ItemDrop> itemList = itemNames.ToItemDropList();
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
        List<DropTable.DropData> drops = dropData.ToDropDataList();
        targetField.SetValue(target, drops);
        if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: DropTable[{drops.Count}]");
    }

    private void UpdateFishBaits<T>(T target, FieldInfo targetField, List<BaitSettingRef> baitSettings, string targetName, bool log)
    {
        List<Fish.BaitSetting> baits = baitSettings.ToBaitSettingsList();
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
        object? currentValue = targetField.GetValue(target);
        if (currentValue is Attack attack)
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