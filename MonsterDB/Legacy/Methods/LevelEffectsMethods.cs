using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class LevelEffectsMethods
{
    
    public static void ReadLevelEffects(string folderPath, ref CreatureData creatureData)
    {
        var filePath = folderPath + Path.DirectorySeparatorChar + "LevelEffects.yml";
        if (!File.Exists(filePath)) return;
        var deserializer = new DeserializerBuilder().Build();
        try
        {
            var serial = File.ReadAllText(filePath);
            creatureData.m_levelEffects = deserializer.Deserialize<List<LevelEffectData>>(serial);
        }
        catch
        {
            Helpers.LogParseFailure(filePath);
        }
    }
    
    public static void Update(GameObject critter, CreatureData creatureData)
    {
        var component = critter.GetComponentInChildren<LevelEffects>();
        if (component == null) return;
        List<LevelEffects.LevelSetup> setups = new();
        foreach (var setup in creatureData.m_levelEffects)
        {
            var data = new LevelEffects.LevelSetup
            {
                m_scale = setup.Scale,
                m_hue = setup.Hue,
                m_saturation = setup.Saturation,
                m_value = setup.Value,
                m_setEmissiveColor = setup.SetEmissiveColor,
                m_emissiveColor = VisualMethods.GetColor(setup.EmissiveColor)
            };
            if (!setup.EnableObject.IsNullOrWhiteSpace())
            {
                var enableObject = Utils.FindChild(critter.transform, setup.EnableObject);
                if (enableObject != null)
                {
                    data.m_enableObject = enableObject.gameObject;
                }
            }
            setups.Add(data);
        }

        component.m_levelSetups = setups;
    }
}