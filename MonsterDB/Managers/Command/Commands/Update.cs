using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonsterDB;

public static partial class Commands
{
    private static void Update(Terminal.ConsoleEventArgs args)
    {
        string fileName = args.GetString(2);
        if (string.IsNullOrEmpty(fileName))
        {
            args.Context.LogWarning("Invalid parameter");
            return;
        }

        if (fileName.Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
            List<string> files = FileManager.GetModFileNames();
            for (int i = 0; i < files.Count; ++i)
            {
                string filePath = Path.Combine(FileManager.ImportFolder, files[i] + ".yml");
                FileManager.Read(filePath);
                args.Context.AddString($"Updated {files[i]}");
            }
        }
        else if (fileName.Equals("raids", StringComparison.CurrentCultureIgnoreCase))
        {
            RaidManager.Read();
            RaidManager.Update();
            args.Context.AddString("Updated all raids");
        }
        else
        {
            string filePath = Path.Combine(FileManager.ImportFolder, fileName + ".yml");
            FileManager.Read(filePath);
            args.Context.AddString($"Updated {fileName}");
        }
    }
    
    private static List<string> GetYMLFileOptions(int i, string word) => i switch
    {
        2 => FileManager.GetModFileNames().ToList(),
        _ => new List<string>()
    };
    
    private static List<string> GetUpdateOptions(int i, string word) => i switch
    {
        2 => FileManager.GetModFileNames().Union(new []
        {
            "all",
            "raids"
        }).ToList(),
        _ => new List<string>()
    };
}