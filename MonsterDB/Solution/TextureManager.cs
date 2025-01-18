using System.Collections;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB.Solution;

public static class TextureManager
{
    public static readonly string m_texturePath = CreatureManager.m_folderPath + Path.DirectorySeparatorChar + "CustomTextures";
    public static readonly Dictionary<string, Texture2D> m_customTextures = new();
    private static readonly Dictionary<string, byte[]> m_textureBytes = new();

    public static Texture? GetTex(string textureName)
    {
        if (m_customTextures.TryGetValue(textureName, out Texture2D texture2D))
        {
            return texture2D;
        }
        if (DataBase.m_textures.TryGetValue(textureName, out texture2D))
        {
            return texture2D;
        }

        return null;
    }
    

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    private static class ZNet_OnNewConnection_Patch
    {
        private static void Postfix(ZNet __instance, ZNetPeer peer)
        {
            peer.m_rpc.Register<ZPackage>(nameof(RPC_ReceiveTexture),RPC_ReceiveTexture);
            peer.m_rpc.Register(nameof(RPC_ForceUpdate), RPC_ForceUpdate);
            peer.m_rpc.Register<ZPackage>(nameof(RPC_ReceiveServerTextureCount), RPC_ReceiveServerTextureCount);
            if (__instance.IsServer())
            {
                peer.m_rpc.Register<ZPackage>(nameof(RPC_ClientTextureRequest), RPC_ClientTextureRequest);
            }
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_CharacterID))]
    private static class ZNet_RPC_CharacterID_Patch
    {
        private static void Postfix(ZNet __instance, ZRpc rpc)
        {
            if (!MonsterDBPlugin.ShareTextures()) return;
            if (!__instance.IsServer()) return;
            if (m_textureBytes.Count == 0) return;
            ZNetPeer peer = __instance.GetPeer(rpc);
            if (peer == null) return;
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Server: New Connection, checking textures");
            ZPackage pkg = new ZPackage();
            pkg.Write(m_textureBytes.Count);
            foreach (var kvp in m_textureBytes)
            {
                pkg.Write(kvp.Key);
            }
            peer.m_rpc.Invoke(nameof(RPC_ReceiveServerTextureCount), pkg);
        }
    }

    private static void RPC_ClientTextureRequest(ZRpc rpc, ZPackage pkg)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(rpc);
        int count = pkg.ReadInt();
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"{peer.m_playerName} requesting {count} textures");

        Dictionary<string, byte[]> texturesToSend = new();
        for (int index = 0; index < count; ++index)
        {
            var name = pkg.ReadString();
            if (!m_textureBytes.TryGetValue(name, out byte[] data)) continue;
            texturesToSend[name] = data;
        }

        MonsterDBPlugin.m_plugin.StartCoroutine(StartSendingTextures(peer, texturesToSend));

    }

    private static void RPC_ReceiveServerTextureCount(ZRpc rpc, ZPackage pkg)
    {
        int count = pkg.ReadInt();
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Client: server has {count} textures");

        List<string> texturesToRequest = new();

        for (int index = 0; index < count; ++index)
        {
            string name = pkg.ReadString();
            if (m_customTextures.ContainsKey(name)) continue;
            texturesToRequest.Add(name);
        }
        if (texturesToRequest.Count == 0) return;
        
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Missing {texturesToRequest.Count} textures, requesting...");
        ZPackage package = new ZPackage();
        package.Write(texturesToRequest.Count);
        foreach (string name in texturesToRequest)
        {
            package.Write(name);
        }
        rpc.Invoke(nameof(RPC_ClientTextureRequest), package);
    }
    
    private static IEnumerator StartSendingTextures(ZNetPeer peer, Dictionary<string, byte[]> data)
    {
        foreach (KeyValuePair<string, byte[]> kvp in data)
        {
            ZPackage pkg = PackageTexture(kvp.Key, kvp.Value);
            peer.m_rpc.Invoke(nameof(RPC_ReceiveTexture), pkg);
            double kilobytes = kvp.Value.Length / 1024.0;
            MonsterDBPlugin.MonsterDBLogger.LogDebug($"Sent texture ({kilobytes}kb): {kvp.Key}");
            
            yield return new WaitForSeconds(1);
        }
        peer.m_rpc.Invoke(nameof(RPC_ForceUpdate));
    }

    private static void RPC_ForceUpdate(ZRpc rpc)
    {
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Received textures from server, updating...");
        Initialization.UpdateAll();
    }
    private static void RPC_ReceiveTexture(ZRpc rpc, ZPackage pkg)
    {
        string name = pkg.ReadString();
        byte[] data = pkg.ReadByteArray();
        Texture2D texture = ReadBytes(name, data);
        WriteTexture(texture);
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Received texture: {name}");
    }

    private static void WriteTexture(Texture2D texture2D)
    {
        string filePath = m_texturePath + Path.DirectorySeparatorChar + texture2D.name + ".png";
        if (File.Exists(filePath)) return;
        byte[] data = texture2D.EncodeToPNG();
        File.WriteAllBytes(filePath, data);
    }
    
    private static ZPackage PackageTexture(string name, byte[] data)
    {
        ZPackage pkg = new ZPackage();
        pkg.Write(name);
        pkg.Write(data);
        return pkg;
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
        MonsterDBPlugin.MonsterDBLogger.LogDebug($"Registered {count} textures");
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
        m_customTextures[texture.name] = texture;
        m_textureBytes[texture.name] = fileData;
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Registered texture: " + texture.name);
    }

    private static Texture2D ReadBytes(string name, byte[] data)
    {
        Texture2D texture = LoadTexture(name, data);
        m_customTextures[texture.name] = texture;
        return texture;
    }
}