using System;
using System.Collections.Generic;

namespace MonsterDB;

[Serializable]
public class RRR_NPC
{
    public bool? bIsFemale ;
    public bool? bNoBeardIfFemale ;
    public bool? bRandomGender ;
    public List<string>? aBeardIds ;
    public List<string>? aHairIds ;
    public RRR_Color? cSkinColorMin ;
    public RRR_Color? cSkinColorMax ;
    public RRR_Color? cHairColorMin ;
    public RRR_Color? cHairColorMax ;
    public float? fHairValueMin ;
    public float? fHairValueMax ;

    public void Setup(VisualRef reference)
    {
        if (bRandomGender.HasValue)
        {
            reference.m_modelIndex = bIsFemale.HasValue && bIsFemale.Value ? new[] { 1 } :  new[] { 0 };
            if (bIsFemale.HasValue && bIsFemale.Value && bNoBeardIfFemale.HasValue && bNoBeardIfFemale.Value) reference.m_beards = Array.Empty<string>();
        }
        else
        {
            reference.m_beards = aBeardIds?.ToArray();
            reference.m_hairs = aHairIds?.ToArray();
        }
        
        reference.m_skinColors = GetSkinColors();
        reference.m_hairColors = GetHairColors();
    }

    public string[] GetHairColors()
    {
        List<string> colors = new();
        if (cHairColorMin != null) colors.Add(cHairColorMin.ToRGBA());
        if (cHairColorMax != null) colors.Add(cHairColorMax.ToRGBA());
        return colors.ToArray();
    }

    public string[] GetSkinColors()
    {
        List<string> colors = new();
        if (cSkinColorMin != null) colors.Add(cSkinColorMin.ToRGBA());
        if (cSkinColorMax != null) colors.Add(cSkinColorMax.ToRGBA());
        return colors.ToArray();
    }
}