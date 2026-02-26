using System;
using UnityEngine;

namespace MonsterDB;

public enum ConversionType
{
    Fermenter,
    CookingStation,
    Smelter
}

[Serializable]
public class ItemConversionRef : Reference
{
    public string? m_prefab;
    public ConversionType? m_type;
    public string? m_from;
    public string? m_to;
    public int? m_producedItems;
    public float? m_cookTime;

    public bool TryToFermenterConversion(out Fermenter.ItemConversion conversion)
    {
        conversion = new Fermenter.ItemConversion();
        if (!TryGetItems(out ItemDrop from, out ItemDrop to)) return false;
        conversion.m_from = from;
        conversion.m_to = to;
        conversion.m_producedItems = m_producedItems ?? 1;
        return true;
    }

    public bool TryToSmelterConversion(out Smelter.ItemConversion conversion)
    {
        conversion = new Smelter.ItemConversion();
        if (!TryGetItems(out ItemDrop from, out ItemDrop to)) return false;
        conversion.m_from = from;
        conversion.m_to = to;
        return true;
    }

    public bool TryToCookingConversion(out CookingStation.ItemConversion conversion)
    {
        conversion = new CookingStation.ItemConversion();
        if (!TryGetItems(out ItemDrop from, out ItemDrop to)) return false;
        conversion.m_from = from;
        conversion.m_to = to;
        conversion.m_cookTime = m_cookTime ?? 5f;
        return true;
    }

    private bool TryGetItems(out ItemDrop from, out ItemDrop to)
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        from = null;
        to = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        if (m_from == null || m_to == null) return false;
        GameObject? fromPrefab = PrefabManager.GetPrefab(m_from);
        GameObject? toPrefab = PrefabManager.GetPrefab(m_to);
        if (fromPrefab == null || toPrefab == null) return false;
        if (!fromPrefab.TryGetComponent(out from) || 
            !toPrefab.TryGetComponent(out to)) return false;
        return true;
    }
}