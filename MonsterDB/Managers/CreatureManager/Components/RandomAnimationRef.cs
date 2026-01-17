using System;
using System.Collections.Generic;
using System.Linq;

namespace MonsterDB;

[Serializable]
public class RandomAnimationRef : Reference
{
    public List<RandomValueRef>? m_values;

    [Serializable]
    public class RandomValueRef : Reference
    {
        public string m_name = "";
        public int m_values;
        public float m_interval;
        public bool m_floatValue;
        public float m_floatTransition = 1f;
    }
}

public static partial class Extensions
{
    public static List<RandomAnimationRef.RandomValueRef> ToRef(this List<RandomAnimation.RandomValue> list)
    {
        return list.Select(x => new RandomAnimationRef.RandomValueRef()
        {
            m_name = x.m_name,
            m_values = x.m_values,
            m_interval = x.m_interval,
            m_floatValue = x.m_floatValue,
            m_floatTransition = x.m_floatTransition,
        }).ToList();
    }

    public static List<RandomAnimation.RandomValue> FromRef(this List<RandomAnimationRef.RandomValueRef> list)
    {
        return list.Select(x => new RandomAnimation.RandomValue()
        {
            m_name = x.m_name,
            m_values = x.m_values,
            m_interval = x.m_interval,
            m_floatValue = x.m_floatValue,
            m_floatTransition = x.m_floatTransition,
        }).ToList();
    }
}

