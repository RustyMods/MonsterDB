using System.Reflection;
using BepInEx;
using HarmonyLib;
using MonsterDB.Solution;

namespace MonsterDB;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class MonsterDBPlugin : BaseUnityPlugin
{
    internal const string ModName = "MonsterDB";
    internal const string ModVersion = "0.2.4";
    internal const string Author = "RustyMods";
    public const string ModGUID = Author + "." + ModName;
    private readonly Harmony _harmony = new(ModGUID);
    public static MonsterDBPlugin instance = null!;
    public static Harmony harmony => instance._harmony;
    
    public void Awake()
    {
        instance = this;

        ConfigManager.Start();
        LocalizationManager.Start();
        
        LegacyManager.Start();
        AudioManager.Start();
        TextureManager.Start();
        FactionManager.Start();
        RaidManager.Start();
        
        CreatureManager.Setup();
        ItemManager.Setup();
        FishManager.Setup();
        EggManager.Setup();
        VisualManager.Setup();
        ProjectileManager.Setup();
        SpawnAbilityManager.Setup();
        CreatureSpawnerManager.Setup();
        
        FileManager.Start();
        PrefabManager.Start();
        SpawnManager.Setup();
        
        ProcreateText.Setup();
        GrowUpText.Setup();
        
        Snapshot.Setup();
        TexturePackage.Setup();
        
        Wiki.Write();
        VersionHandshake.Setup();
        
        Assembly assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly);
    }
    
    private void OnDestroy()
    {
        Config.Save();
    }

    public static void LogInfo(string msg)
    {
        if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Info)) return;
        instance.Logger.LogInfo(msg);
    }

    public static void LogError(string msg)
    {
        if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Error)) return;
        instance.Logger.LogError(msg);
    }

    public static void LogWarning(string msg)
    {
        if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Warning)) return;
        instance.Logger.LogWarning(msg);
    }

    public static void LogDebug(string msg)
    {
        if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Debug)) return;
        instance.Logger.LogDebug(msg);
    }

    public static void LogFatal(string msg)
    {
        instance.Logger.LogFatal(msg);
    }
}