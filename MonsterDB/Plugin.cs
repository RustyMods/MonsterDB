using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MonsterDB;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class MonsterDBPlugin : BaseUnityPlugin
{
    internal const string ModName = "MonsterDB";
    internal const string ModVersion = "0.2.8";
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
        
        FileManager.Start();
        PrefabManager.Start();
        SpawnManager.Setup();
        
        ProcreateText.Setup();
        GrowUpText.Setup();
        
        TexturePackage.Setup();
        
        Wiki.Write();
        VersionHandshake.Setup();
        
        Commands.Init();
        
        Assembly assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly);
    }
    
    private void OnDestroy()
    {
        Config.Save();
    }

    public static void LogInfo(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Info)) return;
        instance.Logger.LogInfo(msg);
    }

    public static void LogError(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Error)) return;
        instance.Logger.LogError(msg);
    }

    public static void LogWarning(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Warning)) return;
        instance.Logger.LogWarning(msg);
    }

    public static void LogDebug(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Debug)) return;
        instance.Logger.LogDebug(msg);
    }

    public static void LogFatal(string msg)
    {
        if (!ConfigManager.ShouldLog(LogLevel.Fatal)) return;
        instance.Logger.LogFatal(msg);
    }
}