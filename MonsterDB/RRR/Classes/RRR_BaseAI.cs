using System;

namespace MonsterDB;

[Serializable]
public class RRR_BaseAI
{
    public float? fViewRange ;
    public float? fViewAngle ;
    public float? fHearRange ;
    public string? sPathAgentType ;
    public float? fRandomCircleInterval ;
    public float? fRandomMoveInterval ;
    public float? fRandomMoveRange ;
    public bool? bAvoidFire ;
    public bool? bAfraidOfFire ;
    public bool? bAvoidWater ;
    public string? sSpawnMessage ;
    public string? sDeathMessage ;

    public void Setup(BaseAIRef reference)
    {
        reference.m_viewRange = fViewRange;
        reference.m_viewAngle = fViewAngle;
        reference.m_hearRange = fHearRange;
        reference.m_pathAgentType = GetPathAgentType(reference.m_pathAgentType);
        reference.m_randomCircleInterval = fRandomCircleInterval;
        reference.m_randomMoveInterval = fRandomMoveInterval;
        reference.m_randomMoveRange = fRandomMoveRange;
        reference.m_avoidFire = bAvoidFire;
        reference.m_afraidOfFire = bAfraidOfFire;
        reference.m_avoidWater = bAvoidWater;
        reference.m_spawnMessage = sSpawnMessage ?? "";
        reference.m_deathMessage = sDeathMessage ?? "";
    }

    private Pathfinding.AgentType? GetPathAgentType(Pathfinding.AgentType? defaultValue) => sPathAgentType != null
        ? Enum.TryParse(sPathAgentType, true, out Pathfinding.AgentType type) ? type : defaultValue
        : defaultValue;
}