using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseItem : Header
{
    [YamlMember(Order = 6)] public ItemDataSharedRef? ItemData;
    [YamlMember(Order = 7)] public VisualRef? Visuals;
    [YamlMember(Order = 8)] public RecipeRef? Recipe;
    [YamlMember(Order = 9)] public ItemConversionRef[]? Conversions;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Type = BaseType.Item;
        Prefab = prefab.name;
        ClonedFrom = source;
        IsCloned = isClone;
        SetupItem(prefab);
        SetupVisuals(prefab);
    }

    public override VisualRef? GetVisualData() => Visuals;

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseItem originalItem) return;
        if (ItemData != null && originalItem.ItemData != null) ItemData.ResetTo(originalItem.ItemData);
        if (Visuals != null && originalItem.Visuals != null) Visuals.ResetTo(originalItem.Visuals);
    }

    protected void SetupItem(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out ItemDrop component)) return;
        ItemData = new ItemDataSharedRef(component.m_itemData.m_shared);
        SetupRecipe(component);
        SetupConversions(component);
    }

    public void SetupConversions(ItemDrop item)
    {
        List<ItemConversionRef> conversions = new();
        List<GameObject> prefabs = PrefabManager.GetAllPrefabs<ZNetView>();
        for (int i = 0; i < prefabs.Count; ++i)
        {
            GameObject? go = prefabs[i];
            if (go.TryGetComponent(out Fermenter fermenter))
            {
                foreach (Fermenter.ItemConversion? conversion in fermenter.m_conversion)
                {
                    if (conversion.m_from == item || conversion.m_to == item)
                    {
                        var reference = new ItemConversionRef();
                        reference.m_prefab = fermenter.name;
                        reference.m_type = ConversionType.Fermenter;
                        reference.m_from = conversion.m_from.name;
                        reference.m_to = conversion.m_to.name;
                        reference.m_producedItems = conversion.m_producedItems;
                        conversions.Add(reference);
                    }
                }
            }

            if (go.TryGetComponent(out CookingStation cookingStation))
            {
                foreach (CookingStation.ItemConversion? conversion in cookingStation.m_conversion)
                {
                    if (conversion.m_from == item || conversion.m_to == item)
                    {
                        ItemConversionRef reference = new ItemConversionRef();
                        reference.m_type = ConversionType.CookingStation;
                        reference.m_prefab = cookingStation.name;
                        reference.m_from = conversion.m_from.name;
                        reference.m_to = conversion.m_to.name;
                        reference.m_cookTime = conversion.m_cookTime;
                        conversions.Add(reference);
                    }
                }
            }

            if (go.TryGetComponent(out Smelter smelter))
            {
                foreach (var conversion in smelter.m_conversion)
                {
                    if (conversion.m_from == item || conversion.m_to == item)
                    {
                        ItemConversionRef reference = new ItemConversionRef();
                        reference.m_type = ConversionType.Smelter;
                        reference.m_prefab = smelter.name;
                        reference.m_from = conversion.m_from.name;
                        reference.m_to = conversion.m_to.name;
                        conversions.Add(reference);
                    }
                }
            }
        }
        if (conversions.Any()) Conversions = conversions.ToArray();
    }
    
    public void SetupRecipe(ItemDrop item)
    {
        if (!RecipeManager.TryGetRecipeByItem(item.name, out Recipe recipe)) return;
        Recipe = new RecipeRef(recipe);
    }
    protected void SetupVisuals(GameObject prefab)
    {
        Visuals = new VisualRef(prefab);
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        SaveDefault(prefab);
        
        UpdatePrefab(prefab);
        
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }

    protected virtual void SaveDefault(GameObject prefab)
    {
        ItemManager.TrySave(prefab, out _, IsCloned, ClonedFrom);
    }

    protected virtual void UpdatePrefab(GameObject prefab)
    {
        UpdateItem(prefab);
        UpdateVisuals(prefab);
        UpdateRecipe(prefab);
        UpdateConversions(prefab);
    }

    protected void UpdateItem(GameObject prefab)
    {
        if (ItemData == null) return;
        ItemDrop? item = prefab.GetComponent<ItemDrop>();
        if (item == null) return;
        ItemData.UpdateFields(item.m_itemData.m_shared, prefab.name, true);
    }
    
    public void UpdateRecipe(GameObject prefab)
    {
        if (Recipe == null) return;
        if (!prefab.TryGetComponent(out ItemDrop item)) return;
        if (RecipeManager.TryGetRecipeByItem(prefab.name, out Recipe recipe))
        {
            Recipe.UpdateFields(recipe, recipe.name, true);
        }
        else
        {
            recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.name = "Recipe_" + prefab.name;
            Recipe.UpdateFields(recipe, recipe.name, true);
            if (recipe.m_item == null) recipe.m_item = item;
            recipe.Register();
        }
    }

    public void UpdateConversions(GameObject prefab)
    {
        if (Conversions == null) return;
        MonsterDBPlugin.LogInfo($"[{prefab.name}] updating conversions");
        for (int i = 0; i < Conversions.Length; i++)
        {
            ItemConversionRef conversion = Conversions[i];
            if (conversion.m_prefab == null || conversion.m_from == null || conversion.m_to == null) continue;
            GameObject? go = PrefabManager.GetPrefab(conversion.m_prefab);
            if (go == null) continue;
            switch (conversion.m_type)
            {
                case ConversionType.Fermenter:
                    if (!go.TryGetComponent(out Fermenter fermenter)) continue;
                    if (!conversion.TryToFermenterConversion(out Fermenter.ItemConversion fermenterConversion)) continue;
                    Fermenter.ItemConversion? existingFermenterConversion = fermenter.m_conversion.Find(x => 
                        x.m_from == fermenterConversion.m_from && 
                        x.m_to == fermenterConversion.m_to);
                    if (existingFermenterConversion != null)
                    {
                        existingFermenterConversion.m_producedItems = fermenterConversion.m_producedItems;
                        if (ConfigManager.ShouldLogDetails())
                        {
                            MonsterDBPlugin.LogDebug($"[{fermenter.name}] modified conversion: {existingFermenterConversion.m_from.name} produces {existingFermenterConversion.m_to.name} x{existingFermenterConversion.m_producedItems}");
                        }
                    }
                    else
                    {
                        fermenter.m_conversion.Add(fermenterConversion);
                        if (ConfigManager.ShouldLogDetails())
                        {
                            MonsterDBPlugin.LogDebug($"[{fermenter.name}] added conversion: {fermenterConversion.m_from.name} produces {fermenterConversion.m_to.name} x{fermenterConversion.m_producedItems}");
                        }
                    }
                    break;
                case ConversionType.Smelter:
                    if (!go.TryGetComponent(out Smelter smelter)) continue;
                    if (!conversion.TryToSmelterConversion(out Smelter.ItemConversion smelterConversion)) continue;
                    Smelter.ItemConversion existingSmelterConversion = smelter.m_conversion.Find(x =>
                        x.m_from == smelterConversion.m_from && 
                        x.m_to == smelterConversion.m_to);
                    if (existingSmelterConversion != null) continue;
                    smelter.m_conversion.Add(smelterConversion);
                    if (ConfigManager.ShouldLogDetails())
                    {
                        MonsterDBPlugin.LogDebug($"[{smelter.name}] added conversion: {smelterConversion.m_from.name} to {smelterConversion.m_to.name}");
                    }
                    break;
                case ConversionType.CookingStation:
                    if (!go.TryGetComponent(out CookingStation cookingStation)) continue;
                    if (!conversion.TryToCookingConversion(out CookingStation.ItemConversion cookingConversion))
                        continue;
                    var existingCook = cookingStation.m_conversion.Find(x =>
                        x.m_from == cookingConversion.m_from && 
                        x.m_to == cookingConversion.m_to);
                    if (existingCook != null)
                    {
                        existingCook.m_cookTime = cookingConversion.m_cookTime;
                        if (ConfigManager.ShouldLogDetails())
                        {
                            MonsterDBPlugin.LogDebug($"[{cookingStation.name}] modified conversion: {existingCook.m_from.name} to {existingCook.m_to.name} in {existingCook.m_cookTime}s");
                        }
                    }
                    else
                    {
                        cookingStation.m_conversion.Add(cookingConversion);
                        if (ConfigManager.ShouldLogDetails())
                        {
                            MonsterDBPlugin.LogDebug($"[{cookingStation.name}] added conversion: {cookingConversion.m_from.name} to {cookingConversion.m_to.name} in {cookingConversion.m_cookTime}s");

                        }
                    }
                    break;
            }
        }
    }

    protected void UpdateVisuals(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.Update(prefab, false, true);
        }
    }
}