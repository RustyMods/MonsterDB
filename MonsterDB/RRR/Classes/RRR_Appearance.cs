using System;
using System.Linq;
using UnityEngine;


namespace MonsterDB;

[Serializable]
public class RRR_Appearance
{
    public RRR_Scale? vScale ;
    public RRR_Color? cTintColor ;
    public RRR_Color? cTintItems ;
    public RRR_Color? cBodySmoke ;
    public string? sCustomTexture ;

    public void Setup(VisualRef reference)
    {
        if (vScale != null) reference.m_scale = new Vector3Ref(vScale.fX ?? 1f, vScale.fY ?? 1f, vScale.fZ ?? 1f);
    }
}