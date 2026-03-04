using System;

namespace MonsterDB;

[Serializable]
public class RRR_Special
{
    public bool? bCanTame ;
    public bool? bAlwaysTame ;
    public bool? bCommandableWhenTame ;
    public bool? bCanProcreateWhenTame ;
    public bool? bFxNoTame ;
    public bool? bSfxNoAlert ;
    public bool? bSfxNoIdle ;
    public bool? bSfxNoHit ;
    public bool? bSfxNoDeath ;

    public void Setup(TameableRef reference)
    {
        reference.m_commandable = bCommandableWhenTame;
    }
}