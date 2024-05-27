using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace MonsterDB.DataBase;

public static class TextureManager
{
    public static Dictionary<string, Texture2D> RegisteredTextures = new();

    public static readonly Dictionary<string, byte[]> ServerSync_Textures = new();

    public static void ClearRegisteredTextures() => RegisteredTextures.Clear();
    public static void UpdateRegisteredTextures(Dictionary<string, Texture2D> data) => RegisteredTextures = data;
    
    public static void ReadLocalTextures()
    {
        string[] files = Directory.GetFiles(Paths.TexturePath, "*.png");
        foreach (string file in files) RegisterTexture(file);
    }

    public static Texture2D LoadTexture(string name, byte[] data)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(data);
        texture.name = name;
        return texture;
    }

    private static void RegisterTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        string fileName = Path.GetFileName(filePath).Replace(".png", string.Empty);
        Texture2D texture = LoadTexture(fileName, fileData);
        RegisteredTextures[texture.name] = texture;
        ServerSync_Textures[texture.name] = fileData;
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Successfully registered custom texture: " + texture.name);
    }

    public static void WriteTextureNamesToFile()
    {
        string filePath = Paths.TexturePath + Path.DirectorySeparatorChar + "Textures.yml";
        List<string> textureNames = DataBase.MonsterDB.m_textures.Keys.ToList();
        File.WriteAllLines(filePath, textureNames);
    }
}