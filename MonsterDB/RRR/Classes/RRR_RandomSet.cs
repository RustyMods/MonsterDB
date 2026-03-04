using System;
using System.Collections.Generic;

namespace MonsterDB;

[Serializable]
public class RRR_RandomSet
{
    public List<string>? aSetItems ;

    public HumanoidRef.ItemSet ToItemSet() => new HumanoidRef.ItemSet
    {
        m_name = "",
        m_items = aSetItems?.ToArray() ?? Array.Empty<string>()
    };
}