using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace MonsterDB.Solution;

public static class TextureManager
{
    private static readonly string m_texturePath = CreatureManager.m_folderPath + Path.DirectorySeparatorChar + "CustomTextures";
    public static readonly Dictionary<string, Texture2D> RegisteredTextures = new();

    public static void ClearRegisteredTextures() => RegisteredTextures.Clear();

    public static bool GetRegisteredTexture(string textureName, out Texture2D texture)
    {
        return RegisteredTextures.TryGetValue(textureName, out texture);
    }

    public static void ReadLocalTextures()
    {
        if (!Directory.Exists(CreatureManager.m_folderPath)) Directory.CreateDirectory(CreatureManager.m_folderPath);
        if (!Directory.Exists(m_texturePath)) Directory.CreateDirectory(m_texturePath);
        string[] files = Directory.GetFiles(m_texturePath, "*.png");
        int count = 0;
        foreach (string file in files)
        {
            RegisterTexture(file);
            ++count;
        }
        MonsterDBPlugin.MonsterDBLogger.LogInfo($"Registered {count} textures");
    }

    private static Texture2D LoadTexture(string name, byte[] data)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(data);
        texture.name = name;
        return texture;
    }
    
    private static void RegisterTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        string name = Path.GetFileName(filePath).Replace(".png", string.Empty).Trim();
        Texture2D texture = LoadTexture(name, fileData);
        RegisteredTextures[texture.name] = texture;
        // ServerSync_Textures[texture.name] = fileData;
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Registered texture: " + texture.name);
    }

    private static Texture2D ReadBytes(string name, byte[] data)
    {
        Texture2D texture = LoadTexture(name, data);
        RegisteredTextures[texture.name] = texture;
        return texture;
    }
}