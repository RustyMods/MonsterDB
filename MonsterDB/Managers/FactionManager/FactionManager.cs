using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MonsterDB;

namespace Norsemen;

public static class FactionManager
{
    public static readonly Dictionary<Character.Faction, Faction> customFactions;
    private static readonly Dictionary<string, Character.Faction> factions;

    static FactionManager()
    {
        customFactions = new Dictionary<Character.Faction, Faction>();
        factions = new  Dictionary<string, Character.Faction>();
        
        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetValues)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_Enum_GetValues))));
        harmony.Patch(AccessTools.Method(typeof(Enum), nameof(Enum.GetNames)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_Enum_GetNames))));
        harmony.Patch(AccessTools.Method(typeof(BaseAI), nameof(BaseAI.IsEnemy), new Type[]{typeof(Character), typeof(Character)}),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(FactionManager), nameof(Patch_BaseAI_IsEnemy))));
    }

    private static bool IsCustom(Character.Faction faction) => customFactions.ContainsKey(faction);

    private static bool Patch_BaseAI_IsEnemy(Character a, Character b, ref bool __result)
    {
        if (!IsCustom(a.m_faction) && !IsCustom(b.m_faction)) return true;
        __result = IsEnemy(a, b);
        return false;
    }

    private static bool IsEnemy(Character a, Character b)
    {
        if (a.m_faction == b.m_faction) return false;

        if (IsCustom(a.m_faction))
        {
            return IsEnemyToCreatures(a, b);
        }

        if (IsCustom(b.m_faction))
        {
            return IsEnemyToCreatures(b, a);
        }

        return true;
    }

    private static bool IsEnemyToCreatures(Character custom, Character other)
    {
        if (!customFactions.TryGetValue(custom.GetFaction(), out Faction faction)) return true;
        return faction.IsEnemy(custom, other);
    }
    
    private static void Patch_Enum_GetValues(Type enumType, ref Array __result)
    {
        if (enumType != typeof(Character.Faction)) return;
        if (factions.Count == 0) return;
        Character.Faction[] f = new Character.Faction[__result.Length + factions.Count];
        __result.CopyTo(f, 0);
        factions.Values.CopyTo(f, __result.Length);
        __result = f;
    }

    private static void Patch_Enum_GetNames(Type enumType, ref string[] __result)
    {
        if (enumType != typeof(Character.Faction)) return;
        if (factions.Count == 0) return;
        __result = __result.AddRangeToArray(factions.Keys.ToArray());
    }


    public static Character.Faction GetFaction(string name)
    {
        if (Enum.TryParse(name, true, out Character.Faction faction))
        {
            return faction;
        }

        if (factions.TryGetValue(name, out faction))
        {
            return faction;
        }

        Dictionary<Character.Faction, string> map = GetFactionMap();
        foreach (KeyValuePair<Character.Faction, string> kvp in map)
        {
            if (kvp.Value == name)
            {
                faction = kvp.Key;
                factions[name] = faction;
                return faction;
            }
        }

        faction = (Character.Faction)name.GetStableHashCode();
        factions[name] = faction;
        return faction;

    }

    private static Dictionary<Character.Faction, string> GetFactionMap()
    {
        Array values = Enum.GetValues(typeof(Character.Faction));
        string[] names = Enum.GetNames(typeof(Character.Faction));
        Dictionary<Character.Faction, string> map = new();
        for (int i = 0; i < values.Length; ++i)
        {
            map[(Character.Faction)values.GetValue(i)] = names[i];
        }
        return map;
    }
}