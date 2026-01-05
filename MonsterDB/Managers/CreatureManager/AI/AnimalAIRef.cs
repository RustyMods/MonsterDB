using System;
using JetBrains.Annotations;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class AnimalAIRef : BaseAIRef
{
    public float? m_timeToSafe;
}