using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonsterDB;

public static partial class Commands
{
    private static void CleanAll(Terminal.ConsoleEventArgs args)
    {
        foreach (KeyValuePair<string, Header> kvp in LoadManager.modified)
        {
            if (kvp.Value.Clean())
            {
                string content = ConfigManager.Serialize(kvp.Value);
                string filename = kvp.Value.Prefab + ".CLEANED.yml";
                string filepath = Path.Combine(FileManager.ExportFolder, filename);
                File.WriteAllText(filepath, content);
        
                args.Context.Log(HEX_Gray, $"Cleaned up {kvp.Value}, exported to {filepath}");
            }
        }
    }
    
    private static void Clean(Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(2);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("> Specify target prefab");
            return;
        }

        if (!LoadManager.modified.TryGetValue(prefabName, out Header? modified))
        {
            args.Context.LogWarning($"> Failed to find modified prefab: {prefabName}");
            return;
        }

        if (modified.Clean())
        {
            string content = ConfigManager.Serialize(modified);
            string filename = modified.Prefab + ".CLEANED.yml";
            string filepath = Path.Combine(FileManager.ExportFolder, filename);
            File.WriteAllText(filepath, content);
        
            args.Context.Log(HEX_Gray, $"Cleaned up {prefabName}, exported to {filepath}");
        }
        else
        {
            args.Context.Log(HEX_Gray, $"> [{prefabName}] Nothing to clean up");
        }

    }

    private static List<string> GetModifiedPrefabNames(int i, string word) => i switch
    {
        2 => LoadManager.modified.Keys.ToList(),
        _ => []
    };
}