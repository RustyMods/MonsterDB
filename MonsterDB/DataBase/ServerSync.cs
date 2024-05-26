using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.DataBase;

public static class ServerSync
{
    private static readonly CustomSyncedValue<string> ServerTextures = new(MonsterDBPlugin.ConfigSync, "MonsterDB_ServerTextures", "");
    private static readonly CustomSyncedValue<string> ServerMonsterDB = new(MonsterDBPlugin.ConfigSync, "MonsterDB_ServerData", "");
    private static readonly CustomSyncedValue<string> ServerSpawnSystem = new(MonsterDBPlugin.ConfigSync, "MonsterDB_SpawnSystemData", "");
    
    public static Dictionary<string, MonsterData> m_serverData = new();

    public static bool m_initialized;
    public static void UpdateServerSpawnSystem(string data) => ServerSpawnSystem.Value = data;

    public static void UpdateServerMonsterDB()
    {
        if (!ZNet.instance.IsServer()) return;
        if (m_serverData.Count <= 0) return;
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(m_serverData);
        ServerMonsterDB.Value = data;
        MonsterDBPlugin.MonsterDBLogger.LogInfo("ServerSync: Updated MonsterDB to a total of " + m_serverData.Count + " files");
    }

    public static void UpdateServerTextures()
    {
        if (!ZNet.instance.IsServer()) return;
        if (TextureManager.ServerSync_Textures.Count <= 0) return;
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(TextureManager.ServerSync_Textures);
        ServerTextures.Value = data;
        MonsterDBPlugin.MonsterDBLogger.LogInfo("ServerSync: Updated MonsterDB with " + TextureManager.ServerSync_Textures.Count + " textures");
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static class ZNet_Awake_Patch
    {
        private static void Postfix(ZNet __instance)
        {
            SpawnData.ReadSpawnFiles();
            if (__instance.IsServer()) return;
            if (m_initialized) return;
            AwaitServerFiles();
            m_initialized = true;
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Client: Awaiting files from server");
        }
    }
    private static void UpdateMonsterData()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();

        foreach (MonsterData data in m_serverData.Values)
        {
            MonsterManager.ResetMonster(data.PrefabName);
        }
            
        m_serverData = deserializer.Deserialize<Dictionary<string, MonsterData>>(ServerMonsterDB.Value);
        
        int count = 0;

        foreach (MonsterData data in m_serverData.Values.Where(x => x.isClone))
        {
            MonsterManager.Server_CloneMonster(data);
        }
            
        foreach (var data in m_serverData.Values)
        {
            if (MonsterManager.Server_UpdateMonster(data))
            {
                ++count;
            };
        }
        
        MonsterDBPlugin.MonsterDBLogger.LogInfo("Client: Received " + count + " MonsterDB files from server") ;

    }

    private static void UpdateSpawnData()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, MonsterSpawnData> data = deserializer.Deserialize<Dictionary<string, MonsterSpawnData>>(ServerSpawnSystem.Value);
        SpawnData.ClearSpawnData();
        SpawnData.AddSpawnData(data);
        SpawnData.UpdateSpawnList();
        MonsterDBPlugin.MonsterDBLogger.LogInfo("Client: Received " + data.Count + " MonsterDB Spawn files from server");
    }

    private static void UpdateTextureData()
    {
        IDeserializer deserializer = new DeserializerBuilder().Build();
        TextureManager.ClearRegisteredTextures();
        Dictionary<string, byte[]> data = deserializer.Deserialize<Dictionary<string, byte[]>>(ServerTextures.Value);
        Dictionary<string, Texture2D> textures = new();
        foreach (var kvp in data)
        {
            textures[kvp.Key] = TextureManager.LoadTexture(kvp.Key, kvp.Value);
        }
        TextureManager.UpdateRegisteredTextures(textures);
        MonsterDBPlugin.MonsterDBLogger.LogInfo("Client: Received " + data.Count + " MonsterDB Textures from server");
    }

    private static void AwaitServerFiles()
    {
        ServerTextures.ValueChanged += () =>
        {
            if (ServerTextures.Value.IsNullOrWhiteSpace()) return;
            UpdateTextureData();
        };
        ServerMonsterDB.ValueChanged += () =>
        {
            if (ServerMonsterDB.Value.IsNullOrWhiteSpace()) return;
            UpdateMonsterData();
        };

        ServerSpawnSystem.ValueChanged += () =>
        {
            if (ServerSpawnSystem.Value.IsNullOrWhiteSpace()) return;
            UpdateSpawnData();
        };

    }
    
}