using System;
using System.Collections.Generic;

namespace Norsemen;

[Serializable]
public class Faction
{
    public bool targetTamed = true;
    public bool targetTameables = true;
    public List<Character.Faction> allies = new();
    
    public Faction(){}

    public Faction(string name)
    {
        FactionManager.customFactions[FactionManager.GetFaction(name)] = this;
    }
    
    public bool IsEnemy(Character custom, Character other)
    {
        bool isCustomTamed = custom.IsTamed();
        bool isOtherTamed = other.IsTamed();
                
        if (isCustomTamed && isOtherTamed)
        {
            return false;
        }

        if (isOtherTamed)
        {
            return targetTamed;
        }
        
        switch (other.m_faction)
        {
            case Character.Faction.Players:
                return !allies.Contains(Character.Faction.PlayerSpawned);
            case Character.Faction.PlayerSpawned:
                return !allies.Contains(Character.Faction.Players) || !allies.Contains(Character.Faction.PlayerSpawned);
            default:
                if (allies.Contains(other.m_faction))
                {
                    return false;
                }
                
                bool isCharacterTameable = other.GetComponent<Tameable>();
                
                if (isCharacterTameable && !targetTameables)
                {
                    return false;
                }
                return true;
        }
    }
}