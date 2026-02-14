using System;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseSpawnData : Header
{
    [YamlMember(Order = 6)] public SpawnDataRef? SpawnData;

    public void SetupData(SpawnSystem.SpawnData data)
    {
        SetupVersions();
        Type = BaseType.SpawnData;
        Prefab = data.m_name;
        SpawnData = new SpawnDataRef(data);
    }

    public override void Update()
    {
        if (SpawnData == null) return;

        if (SpawnManager.TryGetSpawnData(Prefab, out SpawnSystem.SpawnData data))
        {
            SpawnData.UpdateFields(data, data.m_name, true);
            MonsterDBPlugin.LogInfo($"Updated SpawnData: {Prefab}");
        }
        else
        {
            SpawnManager.QueueUpdate(this);
            MonsterDBPlugin.LogInfo($"Queued Update SpawnData: {Prefab}");
        }
        
        if (LoadManager.loadList.Exists(x => x.Prefab == Prefab)) return;
        LoadManager.loadList.Add(this);
    }

    public void UpdateData(SpawnSystem.SpawnData data)
    {
        if (SpawnData == null) return;
        SpawnData.UpdateFields(data, data.m_name, true);
        base.Update();
    }
}