using System;
using System.Reflection;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class RandomSpawnRef : Reference
{
    [YamlMember(Description = "Visual enabled while not spawning")]
    public string? m_OffObject;
    [YamlMember(Description = "0.0 - 100.0")]
    public float? m_chanceToSpawn = 50f;
    public Room.Theme? m_dungeonRequireTheme;
    public Heightmap.Biome? m_requireBiome;
    public bool? m_notInLava;
    [YamlMember(Description = "Elevation span (water is 30)")]
    public int? m_minElevation = -10000;
    public int? m_maxElevation = 10000;
    
    public RandomSpawnRef(){}
    public RandomSpawnRef(RandomSpawn component) => Setup(component);
    
    protected override void UpdateGameObject<T>(T target, FieldInfo targetField, string targetName, string goName,
        bool log)
    {
        if (target is not RandomSpawn component) return;
        
        if (string.IsNullOrEmpty(goName))
        {
            targetField.SetValue(target, null);
            if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: null");
        }
        else
        {
            Transform child = Utils.FindChild(component.transform, goName);
            if (child != null)
            {
                targetField.SetValue(target, child.gameObject);
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] {targetField.Name}: {child.name}");
            }
        }
    }
}