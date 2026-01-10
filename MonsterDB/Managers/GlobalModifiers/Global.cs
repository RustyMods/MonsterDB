using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace MonsterDB.GlobalModifiers;

[Serializable]
public class Global
{
    [YamlMember(Description = "Define groups")]
    public Dictionary<string, string[]>? groups = new()
    {
        ["meadows"] = new [] {"Boar", "Deer", "Neck", "Greyling"},
        ["blackforest"] = new []{"Greydwarf", "Greydwarf_Elite", "Greydwarf_Shaman", "Troll", "Bjorn", "Skeleton", "Skeleton_Poison", "Ghost"},
        ["swamp"] = new[]{"Leech", "Abomination", "Blob", "BlobElite", "Surtling", "Wraith"},
        ["mountains"] = new[]{"Wolf", "Ulv", "Fenring", "FenringCultist", "Hatchling", "StoneGolem"},
        ["plains"] = new[]{"Goblin", "GoblinBrute", "GoblinShaman", "Lox", "BlobTar", "Deathsquito", "Unbjorn"},
        ["mistlands"] = new[]{"Hare", "Seeker", "SeekerBrute", "Gjall", "Tick", "SeekerBrood"},
        ["ashlands"] = new[]{"Charred_Warrior", "Charred_Archer", "Charred_Twitcher", "Morgen", "BonemawSerpent", "Asksvin", "Volture"},
        ["ocean"] = new[]{"Serpent"},
        ["bosses"] = new[]{"Eikthyr", "gd_king", "Bonemass", "Dragon", "GoblinKing", "SeekerQueen", "Fader"}
    };
    
    public Dictionary<string, GlobalModifiers> modifiers = new();
    
    public bool GetHealthModifier(string prefabName, out float modifier)
    {
        modifier = 1f;

        if (modifiers.TryGetValue(prefabName, out GlobalModifiers mods) && mods.health.HasValue)
        {
            modifier = mods.health.Value;
            return true;
        }

        if (groups != null)
        {
            foreach (KeyValuePair<string, GlobalModifiers> kvp in modifiers)
            {
                if (kvp.Value.health.HasValue && groups.TryGetValue(kvp.Key, out string[] group) && group.Contains(prefabName))
                {
                    modifier = kvp.Value.health.Value;
                    return true;
                }
            }
        }

        return false;
    }
    

    [Serializable]
    public class GlobalModifiers
    {
        public float? health;
        
    }
}