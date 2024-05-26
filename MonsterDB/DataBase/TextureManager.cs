using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace MonsterDB.DataBase;

public static class TextureManager
{
    public static readonly Dictionary<string, Texture2D> RegisteredTextures = new();

    public static void SaveTextureToPNG(Texture2D texture, string creatureName)
    {
        try
        {
            string FolderPath = Paths.TexturePath + Path.DirectorySeparatorChar + creatureName;
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
            string FilePath = FolderPath + Path.DirectorySeparatorChar + texture.name;
            byte[] bytes = texture.EncodeToPNG();

            File.WriteAllBytes(FilePath, bytes);
        }
        catch
        {
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Failed to save textures for " + creatureName);
        }
    }

    public static void ReadLocalTextures()
    {
        string[] directories = Directory.GetDirectories(Paths.TexturePath);
        foreach (var directory in directories)
        {
            string[] files = Directory.GetFiles(directory);
            foreach (var file in files)
            {
                RegisterTexture(file);
            }
        }
    }

    private static void RegisterTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        string? parentDir = Path.GetDirectoryName(filePath);
        if (parentDir != null)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(parentDir);
            string parent = directoryInfo.Name;
            string name = filePath.Replace(Paths.TexturePath, string.Empty).Replace(".png", string.Empty)
                .Replace(parent, string.Empty).Replace("\\", string.Empty);
            texture.name = name;
            RegisteredTextures[texture.name] = texture;
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Successfully registered custom texture: " + texture.name);
        }
    }
}