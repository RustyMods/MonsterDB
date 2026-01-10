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

    public static implicit operator DropDataRef(DropTable.DropData dropData)
    {
        var reference = new DropDataRef();
        reference.SetFrom(dropData);
        return reference;
    }

    public static implicit operator DropTable.DropData(DropDataRef reference)
    {
        DropTable.DropData drop = new DropTable.DropData();
        drop.SetFieldsFrom(reference);
        return drop;
    }
}

public static partial class Extensions
{
    public static List<DropDataRef> ToRef(this List<DropTable.DropData> dropData)
    {
        return dropData
            .Where(x => x.m_item != null) 
            .Select(x => new DropDataRef()
            {
                m_item = x.m_item.name,
                m_stackMin = x.m_stackMin,
                m_stackMax = x.m_stackMax,
                m_weight = x.m_weight, 
                m_dontScale = x.m_dontScale
            })
            .ToList();
    }

    public static List<DropTable.DropData> FromRef(this List<DropDataRef> reference)
    {
        return reference
            .Select(x => new DropTable.DropData()
            {
                m_item = PrefabManager.GetPrefab(x.m_item),
                m_weight = x.m_weight,
                m_dontScale = x.m_dontScale,
                m_stackMin = x.m_stackMin,
                m_stackMax = x.m_stackMax
            })
            .Where(x => x.m_item != null)
            .ToList();
    }
}