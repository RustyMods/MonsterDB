using System;

namespace MonsterDB;

[Serializable]
public class ItemDataRef : Reference
{
    public string? m_name;
    public string? m_description;
    public int? m_maxStackSize;
    public int? m_maxQuality;
    public float? m_scaleByQuality;
    public float? m_weight;
    public float? m_scaleWeightByQuality;
    public int? m_value;
    public bool? m_teleportable;
}