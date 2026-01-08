using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MonsterDB;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class MonsterDBPlugin : BaseUnityPlugin
{
    internal const string ModName = "MonsterDB";
    internal const string ModDesc = "";
    internal const string ModVersion = "0.2.0";
    internal const string Author = "RustyMods";
    public const string ModGUID = Author + "." + ModName;
    internal static string ConnectionError = "";
    public readonly Harmony _harmony = new(ModGUID);
    private static readonly ManualLogSource MonsterDBLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
    public static MonsterDBPlugin instance = null!;

        
    public void Awake()
    {
        instance = this;

        AudioManager.Start();
        TextureManager.Setup();
        CreatureManager.Setup();
        ItemManager.Setup();
        FishManager.Setup();
        FileManager.Start();
        EggManager.Setup();
        PrefabManager.Setup();
        SpawnManager.Setup();
        Wiki.Write();
        LocalizationManager.Start();
        
        Assembly assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly); 
    }

    private void OnDestroy() => Config.Save();

    public static void LogInfo(string msg)
    {
        if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Info)) return;
        MonsterDBLogger.LogInfo(msg);
    }

    public static void LogError(string msg)
    {
        if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Error)) return;
        MonsterDBLogger.LogError(msg);
    }

    public static void LogWarning(string msg)
    {
        if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Warning)) return;
        MonsterDBLogger.LogWarning(msg);
    }

    public static void LogDebug(string msg)
    {
        if (!ConfigManager.ShouldLog(ConfigManager.LogLevel.Debug)) return;
        MonsterDBLogger.LogDebug(msg);
    }
}