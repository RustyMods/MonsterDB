using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class AnimalAIRef : BaseAIRef
{
    public float? m_timeToSafe;
    // Extra fields to update Animal Tameable
    public List<string>? m_consumeItems;
    public float? m_consumeRange;
    public float? m_consumeSearchRange;
    public float? m_consumeSearchInterval;
    
    public AnimalAIRef(){}

    public AnimalAIRef(AnimalAI ai) => Setup(ai);
}