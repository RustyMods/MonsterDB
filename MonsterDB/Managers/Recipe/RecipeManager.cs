using System.Collections.Generic;
using HarmonyLib;

namespace MonsterDB;

public static class RecipeManager
{
    private static readonly Dictionary<string, Recipe> recipes;
    private static readonly Dictionary<string, Recipe> recipeByItem;
    private static readonly Dictionary<string, Recipe> recipeBySharedItemName;
    private static readonly List<Recipe> recipesToRegister;

    static RecipeManager()
    {
        recipes = new  Dictionary<string, Recipe>();
        recipeByItem = new Dictionary<string, Recipe>();
        recipeBySharedItemName = new Dictionary<string, Recipe>();
        recipesToRegister = new List<Recipe>();
    }
    
    [HarmonyPriority(Priority.Last)]
    public static void MapRecipes(ObjectDB __instance)
    {
        if (__instance == null) return;
        for (int i = 0; i < recipesToRegister.Count; ++i)
        {
            Recipe recipe = recipesToRegister[i];
            if (!__instance.m_recipes.Contains(recipe))
            {
                __instance.m_recipes.Add(recipe);
            }
        }
        
        for (int i = 0; i < __instance.m_recipes.Count; ++i)
        {
            Recipe recipe =  __instance.m_recipes[i];
            if (recipe.m_item == null)
            {
                MonsterDBPlugin.LogWarning($"Mapping recipes, item is null: {recipe.name}");
                continue;
            }
            recipes[recipe.name] = recipe;
            recipeByItem[recipe.m_item.name] = recipe;
            recipeBySharedItemName[recipe.m_item.m_itemData.m_shared.m_name] = recipe;
        }
    }

    public static bool TryGetRecipe(string name, out Recipe recipe) => recipes.TryGetValue(name, out recipe);
    
    public static bool TryGetRecipeByItem(string name, out Recipe recipe) => recipeByItem.TryGetValue(name, out recipe);
    
    public static bool TryGetRecipeBySharedName(string name, out Recipe recipe) => recipeBySharedItemName.TryGetValue(name, out recipe);

    public static void Register(this Recipe recipe)
    {
        if (recipe.m_item == null) return;
        
        if (ObjectDB.instance && 
            !ObjectDB.instance.m_recipes.Contains(recipe))
        {
            ObjectDB.instance.m_recipes.Add(recipe);
        }
        if (PrefabManager._ObjectDB != null && 
            !PrefabManager._ObjectDB.m_recipes.Contains(recipe))
        {
            PrefabManager._ObjectDB.m_recipes.Add(recipe);
        }

        if (!recipesToRegister.Contains(recipe))
        {
            recipesToRegister.Add(recipe);
        }
        
        recipes[recipe.name] = recipe;
        recipeByItem[recipe.m_item.name] = recipe;
        recipeBySharedItemName[recipe.m_item.m_itemData.m_shared.m_name] = recipe;
    }
}