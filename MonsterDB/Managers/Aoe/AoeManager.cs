using System.IO;
using UnityEngine;

namespace MonsterDB;

public static class AoeManager
{
    public static bool Write(GameObject prefab, bool isClone = false, string source = "", string dirPath = "",
        Terminal? context = null)
    {
        if (!TrySave(prefab, out BaseAoe data, isClone, source)) return false;
        if (string.IsNullOrEmpty(dirPath)) dirPath = FileManager.ExportFolder;
        string filepath = Path.Combine(dirPath, prefab.name + ".yml");
        string text = ConfigManager.Serialize(data);
        MonsterDBPlugin.LogInfo($"Saved {prefab.name} to: {filepath}");
        context?.LogInfo($"Exported Aoe {prefab.name}");
        context?.LogInfo(filepath.RemoveRootPath());
        File.WriteAllText(filepath, text);
        return true;
    }

    public static bool TrySave(GameObject prefab, out BaseAoe data, bool isClone, string source = "")
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        data = LoadManager.GetOriginal<BaseAoe>(prefab.name);
#pragma warning restore CS8601 // Possible null reference assignment.
        if (data != null) return true;
        data = new BaseAoe();
        data.Setup(prefab, isClone, source);
        return true;
    }
    
}