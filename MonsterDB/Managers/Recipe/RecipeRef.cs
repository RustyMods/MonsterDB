using System;

namespace MonsterDB;

[Serializable]
public class RecipeRef : Reference
{
    public string? m_item;
    public int? m_amount = 1;
    public bool? m_enabled = true;
    public float? m_qualityResultAmountMultiplier = 1f;
    public int? m_listSortWeight = 100;
    public string? m_craftingStation;
    public string? m_repairStation;
    public int? m_minStationLevel = 1;
    public bool? m_requireOnlyOneIngredient;
    public RequirementRef[]? m_resources;
    
    public RecipeRef(){}

    public RecipeRef(Recipe recipe)
    {
        m_item = recipe.m_item?.name;
        m_amount = recipe.m_amount;
        m_enabled = recipe.m_enabled;
        m_qualityResultAmountMultiplier = recipe.m_qualityResultAmountMultiplier;
        m_listSortWeight = recipe.m_listSortWeight;
        m_craftingStation = recipe.m_craftingStation?.name;
        m_repairStation = recipe.m_repairStation?.name;
        m_minStationLevel = recipe.m_minStationLevel;
        m_requireOnlyOneIngredient = recipe.m_requireOnlyOneIngredient;
        m_resources = recipe.m_resources.ToRequirementRef();
    }
}