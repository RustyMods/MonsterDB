using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MonsterDB.Solution;

public static class Tutorial
{
    public static void Write()
    {
        string filePath = CreatureManager.m_folderPath + Path.DirectorySeparatorChar + "README.md";
        if (File.Exists(filePath)) return;
        TextAsset text = GetText("Tutorial.md");
        File.WriteAllText(filePath, text.text);
    }

    private static TextAsset GetText(string fileName)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
        using Stream? stream = execAssembly.GetManifestResourceStream(resourceName);
        if (stream == null) return new TextAsset();
        using StreamReader reader = new StreamReader(stream);
        string content = reader.ReadToEnd();
        return new TextAsset(content);
    }
}