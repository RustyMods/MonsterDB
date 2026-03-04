using System;
using System.Collections.Generic;
using System.Linq;

namespace MonsterDB;

[Serializable]
public class Faction
{
    public string name = "";
    public bool targetTamed = true;
    public bool targetTameables = true;
    public List<Character.Faction> allies = new();

    public void Setup()
    {
        if (string.IsNullOrEmpty(name)) return;
        FactionManager.GetFaction(name, this);
        MonsterDBPlugin.LogInfo("Loading faction: " + name);
        if (ConfigManager.ShouldLogDetails())
        {
            MonsterDBPlugin.LogDebug($"[ Faction: {name} ] m_targetTamed: {targetTamed}, m_targetTameables: {targetTameables}");
            MonsterDBPlugin.LogDebug($"[ Faction: {name} ] m_allies: {string.Join(", ", allies.Select(s => s.ToString()))}");
        }
    }
    
    public bool IsEnemy(Character custom, Character other)
    {
        if (allies.Contains(other.m_faction))
        {
            return false;
        }
        
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
                
        bool isCharacterTameable = other.GetComponent<Tameable>();
                
        if (isCharacterTameable && !targetTameables)
        {
            return false;
        }
        return true;
    }
}