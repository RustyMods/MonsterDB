using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class RequirementRef : Reference
{
    public string? m_resItem;
    public int? m_amount;
    public int? m_extraAmountOnlyOneIngredient;
    public int? m_amountPerLevel;
    public bool? m_recover;
    
    public RequirementRef(){}

    public RequirementRef(Piece.Requirement requirement)
    {
        m_resItem = requirement.m_resItem?.name;
        m_amount = requirement.m_amount;
        m_extraAmountOnlyOneIngredient = requirement.m_extraAmountOnlyOneIngredient;
        m_amountPerLevel = requirement.m_amountPerLevel;
        m_recover = requirement.m_recover;
    }

    public bool TryGetPieceRequirement(out Piece.Requirement requirement)
    {
        requirement = new Piece.Requirement();
        if (m_resItem == null) return false;
        GameObject? itemPrefab = PrefabManager.GetPrefab(m_resItem);
        if (itemPrefab == null) return false;
        if (!itemPrefab.TryGetComponent(out ItemDrop itemDrop)) return false;
        requirement.m_resItem = itemDrop;
        requirement.m_amount = m_amount ?? 1;
        requirement.m_amountPerLevel = m_amountPerLevel ?? 1;
        requirement.m_extraAmountOnlyOneIngredient = m_extraAmountOnlyOneIngredient ?? 1;
        requirement.m_amountPerLevel = m_amountPerLevel ?? 1;
        requirement.m_recover = m_recover ?? false;
        return true;
    }
}

public static partial class Extensions
{
    public static RequirementRef[] ToRequirementRef(this Piece.Requirement[] requirements)
    {
        List<RequirementRef> reqs = new List<RequirementRef>();
        for (int i = 0; i < requirements.Length; ++i)
        {
            Piece.Requirement requirement = requirements[i];
            RequirementRef req = new RequirementRef(requirement);
            reqs.Add(req);
        }
        return reqs.ToArray();
    }

    public static Piece.Requirement[] ToPieceRequirements(this RequirementRef[] requirements)
    {
        List<Piece.Requirement> reqs = new List<Piece.Requirement>();
        for (int i = 0; i < requirements.Length; ++i)
        {
            RequirementRef requirement = requirements[i];
            if (!requirement.TryGetPieceRequirement(out Piece.Requirement res))
            {
                MonsterDBPlugin.LogWarning("Invalid requirement");
                continue;
            }
            reqs.Add(res);
        }
        return reqs.ToArray();
    }
}