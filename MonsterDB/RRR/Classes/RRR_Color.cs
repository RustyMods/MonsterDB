using System;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class RRR_Color
{
    public float? fRed ;
    public float? fGreen ;
    public float? fBlue ;
    
    public string ToRGBA() => new Color(fRed ?? 0f, fGreen ?? 0f, fBlue ?? 0f, 1f).ToRGBAString();
}