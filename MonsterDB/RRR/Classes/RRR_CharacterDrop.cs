using System;
using System.Collections.Generic;
using System.Linq;

namespace MonsterDB;

[Serializable]
public class RRR_CharacterDrop
{
    public bool bReplaceOriginalDrops ;
    public List<RRR_Drop>? drDrops ;

    public void Setup(CharacterDropRef reference)
    {
        if (!bReplaceOriginalDrops) return;
        reference.m_drops = drDrops?.Select(d => d.ToDropRef()).ToList();
    }
}