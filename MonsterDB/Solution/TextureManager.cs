using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB.Solution;

public static class TextureManager
{
    public static readonly string m_texturePath = CreatureManager.m_folderPath + Path.DirectorySeparatorChar + "CustomTextures";
    public static readonly Dictionary<string, Texture2D> m_customTextures = new();
    private static readonly Dictionary<string, byte[]> m_textureBytes = new();

    // private static readonly Dictionary<ZNetPeer, int> m_peerIndex = new();
    //
    // private static readonly List<string> m_receivedTextures = new();
    //
    // [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    // private static class ZNetScene_Awake_Patch
    // {
    //     private static void Postfix()
    //     {
    //         if (ZNet.instance.IsServer())
    //         {
    //             StartCoroutine();
    //         }
    //     }
    // }
    //
    // [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    // private static class ZNet_OnNewConnection_Patch
    // {
    //     private static void Postfix(ZNetPeer peer)
    //     {
    //         Debug.LogWarning($"new connection: {peer.m_playerName}");
    //         m_peerIndex[peer] = 0;
    //         peer.m_rpc.Register<ZPackage>(nameof(RPC_ReceiveTexture),RPC_ReceiveTexture);
    //     }
    // }
    //
    // [HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
    // private static class ZNet_Disconnect_Patch
    // {
    //     private static void Postfix(ZNetPeer peer) => m_peerIndex.Remove(peer);
    // }
    //
    // private static void StartCoroutine()
    // {
    //     MonsterDBPlugin.MonsterDBLogger.LogDebug("Starting coroutine to send textures to clients");
    //     MonsterDBPlugin.m_plugin.StartCoroutine(nameof(SendTextures));
    // }
    //
    // private static IEnumerator SendTextures()
    // {
    //     while (ZNet.instance)
    //     {
    //         SendToClients();
    //         yield return new WaitForSeconds(60);
    //     }
    // }
    //
    // private static void SendToClients()
    // {
    //     if (!ZNet.instance) return;
    //     var textures = m_textureBytes.ToList();
    //     foreach (ZNetPeer peer in ZNet.instance.m_peers)
    //     {
    //         if (!m_peerIndex.TryGetValue(peer, out int index)) continue;
    //         if (index > textures.Count)
    //         {
    //             m_peerIndex[peer] = 0;
    //             continue;
    //         }
    //         var texture = textures[index];
    //         var pkg = PackageTexture(texture.Key, texture.Value);
    //         peer.m_rpc.Invoke(nameof(RPC_ReceiveTexture), pkg);
    //     }
    // }
    //
    // private static void RPC_ReceiveTexture(ZRpc rpc, ZPackage pkg)
    // {
    //     ReadPackage(pkg);
    // }

    // private static void ReadPackage(ZPackage pkg)
    // {
    //     string name = pkg.ReadString();
    //     if (m_receivedTextures.Contains(name)) return;
    //     if (m_customTextures.ContainsKey(name)) return;
    //     MonsterDBPlugin.MonsterDBLogger.LogDebug($"Received texture: {name}");
    //     byte[] data = pkg.ReadByteArray();
    //     Texture2D texture = ReadBytes(name, data);
    //     WriteTexture(texture);
    //     m_receivedTextures.Add(name);
    // }

    // private static void WriteTexture(Texture2D texture2D)
    // {
    //     var data = texture2D.EncodeToPNG();
    //     var filePath = m_texturePath + Path.DirectorySeparatorChar + texture2D.name + ".png";
    //     if (!File.Exists(filePath)) return;
    //     File.WriteAllBytes(filePath, data);
    // }
    //
    // private static ZPackage PackageTexture(string name, byte[] data)
    // {
    //     ZPackage pkg = new ZPackage();
    //     pkg.Write(name);
    //     pkg.Write(data);
    //     return pkg;
    // }
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
        // ServerSync_Textures[texture.name] = fileData;
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Registered texture: " + texture.name);
    }

    private static Texture2D ReadBytes(string name, byte[] data)
    {
        Texture2D texture = LoadTexture(name, data);
        m_customTextures[texture.name] = texture;
        m_textureBytes[texture.name] = data;
        return texture;
    }
}