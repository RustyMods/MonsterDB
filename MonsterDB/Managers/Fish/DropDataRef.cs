using System;
using System.Collections.Generic;
using System.Linq;

namespace MonsterDB;

[Serializable]
public class DropDataRef : Reference
{
    public string m_item = "";
    public int m_stackMin = 1;
    public int m_stackMax = 1;
    public float m_weight = 1f;
    public bool m_dontScale;
    
    public DropDataRef(){}
    public DropDataRef(DropTable.DropData data) => Setup(data);

    public DropTable.DropData ToDropData()
    {
        return new DropTable.DropData()
        {
            m_item = PrefabManager.GetPrefab(m_item),
            m_weight = m_weight,
            m_dontScale = m_dontScale,
            m_stackMin = m_stackMin,
            m_stackMax = m_stackMax
        };
    }
}

public static partial class Extensions
{
    public static List<DropDataRef> ToDropDataRefList(this List<DropTable.DropData> dropData)
    {
        return dropData
            .Where(x => x.m_item != null) 
            .Select(x => new DropDataRef(x))
            .ToList();
    }

    public static List<DropTable.DropData> ToDropDataList(this List<DropDataRef> reference)
    {
        return reference
            .Select(x => x.ToDropData())
            .Where(x => x.m_item != null)
            .ToList();
    }
}