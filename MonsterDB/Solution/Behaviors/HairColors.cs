using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB.Solution.Behaviors;

public static class HairColors
{
    private static readonly List<Color> m_hairColors = new List<Color>()
    {
        Color.black, new (0.98f, 0.94f, 0.75f, 1f), new (0.63f,0.36f,0f, 1f)
    };
    
    public static Vector3 GetHairColor()
    {
        Color color = m_hairColors[Random.Range(0, m_hairColors.Count)];
        return new Vector3(color.r, color.g, color.b);
    }
}