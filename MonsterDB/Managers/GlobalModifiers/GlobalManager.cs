using System.IO;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB.GlobalModifiers;

public static class GlobalManager
{
    /// <summary>
    /// Not implemented 
    /// </summary>
    private const string FileName = "GlobalModifiers.yml";
    public static Global mods;
    
    static GlobalManager()
    {
        string filePath = Path.Combine(ConfigManager.DirectoryPath, FileName);
        mods = new Global();

        if (File.Exists(filePath)) Read(filePath);
        
        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(ObjectDB), nameof(ObjectDB.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(GlobalManager), nameof(Patch_ObjectDB_Awake))));
        harmony.Patch(AccessTools.Method(typeof(Character), nameof(Character.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(GlobalManager), nameof(Patch_Character_Awake))));
        harmony.Patch(AccessTools.Method(typeof(MonsterAI), nameof(MonsterAI.UpdateTarget)),
            new HarmonyMethod(AccessTools.Method(typeof(GlobalManager), nameof(Patch_MonsterAI_FindTarget))));

    }

    private static void Read(string filePath)
    {
        string text = File.ReadAllText(filePath);
        try
        {
            Global data = ConfigManager.Deserialize<Global>(text);
            mods = data;
            mods.Setup();
        }
        catch
        {
            MonsterDBPlugin.LogWarning($"Failed to deserialize {Path.GetFileName(filePath)}");
        }
    }
    
    private static void Patch_ObjectDB_Awake(ObjectDB __instance)
    {
        SE_GlobalModifiers? se = ScriptableObject.CreateInstance<SE_GlobalModifiers>();
        se.name = "SE_GlobalModifiers";
        __instance.m_StatusEffects.Add(se);
    }

    private static void Patch_Character_Awake(Character __instance)
    {
        __instance.GetSEMan().AddStatusEffect("SE_GlobalModifiers".GetStableHashCode());
    }

    private static void Patch_BaseAI_Awake(MonsterAI __instance)
    {
        string? prefab = Utils.GetPrefabName(__instance.name);
        if (mods.AttackPlayerObjects(prefab, out bool enable) && !enable)
        {
            __instance.m_attackPlayerObjects = false;
        }
    }

    private static void Patch_MonsterAI_FindTarget(MonsterAI __instance)
    {
        string? prefab = Utils.GetPrefabName(__instance.name);
        if (mods.AttackPlayerObjects(prefab, out bool enable) && !enable)
        {
            __instance.m_targetStatic = null;
        }
    }
}