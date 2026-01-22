using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public static class TexturePackage
{
    private const int MAX_COMPRESSED_CHUNK_SIZE = 64 * 1024; // 64 KB

    private static readonly Dictionary<string, ChunkBuffer> _chunkBuffers;

    static TexturePackage()
    {
        _chunkBuffers = new Dictionary<string, ChunkBuffer>();
        Harmony harmony = MonsterDBPlugin.instance._harmony;
        harmony.Patch(AccessTools.Method(typeof(ZNet), nameof(ZNet.OnNewConnection)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(TexturePackage), nameof(Patch_ZNet_OnNewConnection))));
    }

    private class ChunkBuffer
    {
        public string TransferId = "";
        public int TotalChunks;
        public byte[][] Chunks;
        public int Received;
        
        public int ReceivedBytes;
        public int LastLoggedPercent = -1;

        public void Add(int index, byte[] chunk)
        {
            if (Chunks[index] == null)
            {
                Chunks[index] = chunk;
                Received++;
                ReceivedBytes += chunk.Length;
            }
        }

        public void Assemble()
        {
            int fullSize = 0;
            for (int i = 0; i < TotalChunks; ++i)
                fullSize += Chunks[i].Length;

            byte[] fullData = new byte[fullSize];
            int offset = 0;

            for (int i = 0; i < TotalChunks; ++i)
            {
                byte[] c = Chunks[i];
                System.Buffer.BlockCopy(c, 0, fullData, offset, c.Length);
                offset += c.Length;
            }

            _chunkBuffers.Remove(TransferId);

            ZPackage compressedPkg = new ZPackage(fullData);
            ZPackage dataPkg = compressedPkg.ReadCompressedPackage();

            int count = dataPkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                string name = dataPkg.ReadString();
                byte[] bytes = dataPkg.ReadByteArray();

                TextureData data = new TextureData(name, bytes);
                TextureManager.Add(data);
                data.Write();
            }
        
            MonsterDBPlugin.LogInfo(
                $"TexturePack completed: {TotalChunks} chunks, " +
                $"{ReceivedBytes / 1024f:F1} KB received"
            );
        }
    }

    private static void Patch_ZNet_OnNewConnection(ZNet __instance, ZNetPeer peer)
    {
        peer.m_rpc.Register<ZPackage>(nameof(RPC_ReceiveTextureNames), RPC_ReceiveTextureNames);
        peer.m_rpc.Register<ZPackage>(nameof(RPC_ReceiveTexturesCompressed), RPC_ReceiveTexturesCompressed);
        peer.m_rpc.Register<ZPackage>(nameof(RPC_ReceiveTextureChunk), RPC_ReceiveTextureChunk);
        if (!__instance.IsServer())
        {
            SendTextureNames(peer.m_rpc);
        }
    }
    
    public static void SendTextureNames(ZRpc rpc)
    {
        Dictionary<string, TextureData> files = TextureManager.GetTextureData();
        ZPackage pkg = new ZPackage();
        pkg.Write(files.Count);
        foreach (string? name in files.Keys)
        {
            pkg.Write(name);
        }
        rpc.Invoke(nameof(RPC_ReceiveTextureNames), pkg);
    }
    
    private static void RPC_ReceiveTextureNames(ZRpc rpc, ZPackage pkg)
    {
        List<string> names = new();
        int count = pkg.ReadInt();
        for (int i = 0; i < count; ++i)
        {
            string name = pkg.ReadString();
            names.Add(name);
        }

        SendTextures(rpc, names);
    }
    
    private static void RPC_ReceiveTexturesCompressed(ZRpc rpc, ZPackage pkg)
    {
        pkg = pkg.ReadCompressedPackage();
        int count = pkg.ReadInt();
        for (int i = 0; i < count; ++i)
        {
            string name = pkg.ReadString();
            byte[] bytes = pkg.ReadByteArray();

            TextureData data = new TextureData(name, bytes);
            TextureManager.Add(data);
            data.Write();
        }
    }
    
    private static void SendTextures(ZRpc rpc, List<string> texturesToIgnore)
    {
        Dictionary<string, TextureData> files = TextureManager.GetTextureData();
        foreach (string name in texturesToIgnore)
        {
            files.Remove(name);
        }

        if (files.Count <= 0)
        {
            return;
        }

        // Build uncompressed payload
        ZPackage fullPkg = new ZPackage();
        fullPkg.Write(files.Count);

        foreach (KeyValuePair<string, TextureData> kvp in files)
        {
            fullPkg.Write(kvp.Key);
            fullPkg.Write(kvp.Value.m_bytes);
        }

        // Compress once
        ZPackage compressed = new ZPackage();
        compressed.WriteCompressed(fullPkg);

        byte[] data = compressed.GetArray();
        int totalSize = data.Length;

        int chunkCount = Mathf.CeilToInt((float)totalSize / MAX_COMPRESSED_CHUNK_SIZE);

        string transferId = System.Guid.NewGuid().ToString();

        MonsterDBPlugin.LogInfo(
            $"TexturePack: sending {totalSize} bytes in {chunkCount} chunks");

        for (int i = 0; i < chunkCount; ++i)
        {
            int offset = i * MAX_COMPRESSED_CHUNK_SIZE;
            int size = Mathf.Min(MAX_COMPRESSED_CHUNK_SIZE, totalSize - offset);

            byte[] chunk = new byte[size];
            System.Buffer.BlockCopy(data, offset, chunk, 0, size);

            ZPackage chunkPkg = new ZPackage();
            chunkPkg.Write(transferId);
            chunkPkg.Write(i);
            chunkPkg.Write(chunkCount);
            chunkPkg.Write(chunk);

            rpc.Invoke(nameof(RPC_ReceiveTextureChunk), chunkPkg);
        }
    }
    
    private static void RPC_ReceiveTextureChunk(ZRpc rpc, ZPackage pkg)
    {
        string transferId = pkg.ReadString();
        int index = pkg.ReadInt();
        int total = pkg.ReadInt();
        byte[] chunk = pkg.ReadByteArray();

        if (!_chunkBuffers.TryGetValue(transferId, out ChunkBuffer buffer))
        {
            buffer = new ChunkBuffer
            {
                TransferId = transferId,
                TotalChunks = total,
                Chunks = new byte[total][]
            };
            _chunkBuffers.Add(transferId, buffer);
        }
        
        buffer.Add(index, chunk);
        
        int percent = Mathf.FloorToInt((float)buffer.Received / buffer.TotalChunks * 100f);

        if (percent >= buffer.LastLoggedPercent + 10 || buffer.Received == buffer.TotalChunks)
        {
            buffer.LastLoggedPercent = percent;

            MonsterDBPlugin.LogInfo(
                string.Format(
                    "TexturePack | {0,3}/{1,-3} | {2,3}% | chunk {3,2} | {4,6} bytes | recv {5,7:F1} KB",
                    buffer.Received,
                    buffer.TotalChunks,
                    percent,
                    index + 1,
                    chunk.Length,
                    buffer.ReceivedBytes / 1024f
                )
            );
        }
        
        if (buffer.Received < buffer.TotalChunks) return;
        buffer.Assemble();
    }
}