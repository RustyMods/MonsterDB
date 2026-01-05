using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MonsterDB;

public static class Wiki
{
    private const string FolderName = "README";
    private static readonly string FolderPath;

    static Wiki()
    {
        FolderPath = Path.Combine(ConfigManager.DirectoryPath, FolderName);
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
    }

    public static void Write()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string[] resources = assembly.GetManifestResourceNames();
        string prefix = $"{MonsterDBPlugin.ModName}.Wiki.";
        List<string> paths = resources
            .Where(r => r.StartsWith(prefix) && r.EndsWith(".md"))
            .ToList();
        
        foreach (string resourceName in paths)
        {
            string relativePath = resourceName.Substring(prefix.Length);
            string fileName = Path.GetFileName(relativePath);

            string filePath = Path.Combine(FolderPath, fileName);

            if (File.Exists(filePath)) continue;
            
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;
            
            using FileStream file = File.Create(filePath);
            stream.CopyTo(file);
        }
    }
}