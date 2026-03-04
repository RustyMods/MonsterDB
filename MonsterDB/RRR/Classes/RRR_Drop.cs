using System;

namespace MonsterDB;

[Serializable]
public class RRR_Drop
{
    public string? sPrefabName ;
    public int? iAmountMin ;
    public int? iAmountMax ;
    public float? fChance ;
    public bool? bOnePerPlayer ;
    public bool? bLevelMultiplier ;

    public DropRef ToDropRef() => new DropRef
    {
        m_prefab = sPrefabName,
        m_amountMin = iAmountMin,
        m_amountMax = iAmountMax,
        m_chance = fChance,
        m_onePerPlayer = bOnePerPlayer,
        m_levelMultiplier = bLevelMultiplier
    };
}