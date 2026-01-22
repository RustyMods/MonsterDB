using System;
using System.Collections.Generic;

namespace MonsterDB;

[Serializable]
public class DropTableRef : Reference
{
    public List<DropDataRef>? m_drops;
    public int? m_dropMin;
    public int? m_dropMax;
    public float? m_dropChance;
    public bool? m_oneOfEach;

    public DropTable ToDropTable(string targetName = "")
    {
        DropTable table = new DropTable();
        UpdateFields(table, targetName, false);
        return table;
    }
}