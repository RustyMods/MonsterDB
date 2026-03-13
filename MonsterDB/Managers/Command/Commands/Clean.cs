using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonsterDB;

public static partial class Commands
{
    private static void CleanAll(Terminal.ConsoleEventArgs args)
    {
        int count = 0;
        foreach (KeyValuePair<string, Header> kvp in LoadManager.modified)
        {
            if (kvp.Value.Clean())
            {
                string content = ConfigManager.Serialize(kvp.Value);
                string filename = kvp.Value.Prefab + ".CLEANED.yml";
                string filepath = Path.Combine(FileManager.ExportFolder, filename);
                File.WriteAllText(filepath, content);
                ++count;
            }
        }
        args.Context.LogInfo($"Cleaned up {count} files");
    }
    
    private static void Clean(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("> Specify target prefab");
            return;
        }

        if (prefabName.Equals("all", StringComparison.InvariantCultureIgnoreCase))
        {
            CleanAll(args);
            return;
        }

        if (!LoadManager.modified.TryGetValue(prefabName, out Header? modified))
        {
            args.Context.LogWarning($"Failed to find modified prefab: {prefabName}");
            return;
        }

        if (modified.Clean())
        {
            string content = ConfigManager.Serialize(modified);
            string filename = modified.Prefab + ".CLEANED.yml";
            string filepath = Path.Combine(FileManager.ExportFolder, filename);
            File.WriteAllText(filepath, content);
        
            args.Context.LogInfo($"Cleaned up {prefabName}");
            args.Context.LogInfo($"{filepath.RemoveRootPath()}");
        }
        else
        {
            args.Context.LogDebug($"[{prefabName}] Nothing to clean up");
        }

    }

    private static List<string> GetModifiedPrefabNames(int i, string word) => i switch
    {
        2 => LoadManager.modified.Keys.Union(["all"]).ToList(),
        _ => []
    };
}