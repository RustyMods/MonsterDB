using System.IO;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB.GlobalModifiers;

public static class GlobalManager
{
    private const string FileName = "GlobalModifiers.yml";
    public static Global mods;
    
    static GlobalManager()
    {
        string filePath = Path.Combine(ConfigManager.DirectoryPath, FileName);
        mods = new Global();

        if (File.Exists(filePath)) Read(filePath);
        
        var harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(ObjectDB), nameof(ObjectDB.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(GlobalManager), nameof(Patch_ObjectDB_Awake))));
        harmony.Patch(AccessTools.Method(typeof(Character), nameof(Character.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(GlobalManager), nameof(Patch_Character_Awake))));
    }

    private static void Read(string filePath)
    {
        string text = File.ReadAllText(filePath);
        try
        {
            Global data = ConfigManager.Deserialize<Global>(text);
            mods = data;
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
}