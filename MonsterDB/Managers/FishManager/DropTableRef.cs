using System.Collections.Generic;

namespace MonsterDB;

public class DropTableRef : Reference
{
    public List<DropDataRef>? m_drops;
    public int? m_dropMin;
    public int? m_dropMax;
    public float? m_dropChance;
    public bool? m_oneOfEach;

    public static implicit operator DropTableRef(DropTable dropTable)
    {
        DropTableRef reference = new DropTableRef();
        reference.ReferenceFrom(dropTable);
        return reference;
    }
}