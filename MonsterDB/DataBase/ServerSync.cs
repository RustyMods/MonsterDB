using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using ServerSync;
using YamlDotNet.Serialization;

namespace MonsterDB.DataBase;

public static class ServerSync
{
    private static readonly CustomSyncedValue<string> ServerMonsterDB = new(MonsterDBPlugin.ConfigSync, "MonsterDB_ServerData", "");

    private static readonly CustomSyncedValue<string> ServerSpawnSystem = new(MonsterDBPlugin.ConfigSync, "MonsterDB_SpawnSystemData", "");

    public static Dictionary<string, MonsterData> m_serverData = new();

    public static void UpdateServerSpawnSystem(string data) => ServerSpawnSystem.Value = data;

    public static void UpdateServerMonsterDB()
    {
        if (!ZNet.instance.IsServer()) return;
        if (m_serverData.Count <= 0) return;
        var serializer = new SerializerBuilder().Build();
        var data = serializer.Serialize(m_serverData);
        ServerMonsterDB.Value = data;
        MonsterDBPlugin.MonsterDBLogger.LogInfo("Server: Updated MonsterDB to a total of " + m_serverData.Count + " files");
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    private static class ZNet_Awake_Patch
    {
        private static void Postfix(ZNet __instance)
        {
            SpawnData.ReadSpawnFiles();
            if (__instance.IsServer()) return;
            AwaitServerFiles();
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Client: Awaiting files from server");
        }
    }

    private static void AwaitServerFiles()
    {
        ServerMonsterDB.ValueChanged += () =>
        {
            if (ServerMonsterDB.Value.IsNullOrWhiteSpace()) return;
            var deserializer = new DeserializerBuilder().Build();

            foreach (var data in m_serverData.Values)
            {
                MonsterManager.ResetMonster(data.PrefabName);
            }
            
            m_serverData = deserializer.Deserialize<Dictionary<string, MonsterData>>(ServerMonsterDB.Value);

            int count = 0;

            foreach (var data in m_serverData.Values.Where(x => x.isClone))
            {
                MonsterManager.Command_CloneMonster(data.OriginalMonster, data.PrefabName, false);
            }
            
            foreach (var data in m_serverData.Values)
            {
                if (MonsterManager.Server_UpdateMonster(data))
                {
                    ++count;
                };
            }
            
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Client: Received " + count + " MonsterDB files from server") ;
        };

        ServerSpawnSystem.ValueChanged += () =>
        {
            if (ServerSpawnSystem.Value.IsNullOrWhiteSpace()) return;
            var deserializer = new DeserializerBuilder().Build();
            SpawnData.ClearSpawnData();
            var data = deserializer.Deserialize<Dictionary<string, MonsterSpawnData>>(ServerSpawnSystem.Value);
            SpawnData.AddSpawnData(data);
            SpawnData.UpdateSpawnList();
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Client: Received " + data.Count + " MonsterDB Spawn files from server");
        };
    }
    
}