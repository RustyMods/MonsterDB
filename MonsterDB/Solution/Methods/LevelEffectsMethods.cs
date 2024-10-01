using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class LevelEffectsMethods
{
    private static readonly int Hue = Shader.PropertyToID("_Hue");
    private static readonly int Saturation = Shader.PropertyToID("_Saturation");
    private static readonly int Value = Shader.PropertyToID("_Value");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        var component = critter.GetComponentInChildren<LevelEffects>();
        if (component == null) return;
        foreach (var setup in component.m_levelSetups)
        {
            creatureData.m_levelEffects.Add(Record(setup));
        }
    }

    private static LevelEffectData Record(LevelEffects.LevelSetup setup)
    {
        return new LevelEffectData
        {
            Scale = setup.m_scale,
            Hue = setup.m_hue,
            Saturation = setup.m_saturation,
            Value = setup.m_value,
            EmissiveColor = new VisualMethods.ColorData()
            {
                r = setup.m_emissiveColor.r,
                g = setup.m_emissiveColor.g,
                b = setup.m_emissiveColor.b,
                a = setup.m_emissiveColor.a
            },
            EnableObject = setup.m_enableObject ? setup.m_enableObject.name : ""
        };
    }

    public static void Write(GameObject critter, string folderPath)
    {
        var component = critter.GetComponentInChildren<LevelEffects>();
        if (component == null) return;
        List<LevelEffectData> levelSetups = new();
        foreach (var setup in component.m_levelSetups)
        {
            levelSetups.Add(Record(setup));
        }

        var serializer = new SerializerBuilder().Build();
        var serial = serializer.Serialize(levelSetups);
        var filePath = folderPath + Path.DirectorySeparatorChar + "LevelEffects.yml";
        File.WriteAllText(filePath, serial);
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
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

    [HarmonyPatch(typeof(LevelEffects), nameof(LevelEffects.SetupLevelVisualization))]
    private static class LevelEffects_SetupLevelVisualization_Patch
    {
        private static bool Prefix(LevelEffects __instance, int level)
        {
            string name = __instance.m_character.name.Replace("(Clone)", string.Empty);
            if (!CreatureManager.m_data.TryGetValue(name, out CreatureData data)) return true;
            SetupLevelVisual(__instance, level, __instance.m_character.gameObject, data);
            return false;
        }

        private static void SetupLevelVisual(LevelEffects __instance, int level, GameObject critter, CreatureData data)
        {
            if (level <= 1 || __instance.m_levelSetups.Count < level - 1) return;
            LevelEffects.LevelSetup levelSetup = __instance.m_levelSetups[level - 2];
            critter.transform.localScale = Helpers.GetScale(data.m_scale) * levelSetup.m_scale;
            if (__instance.m_mainRender)
            {
                string key = Utils.GetPrefabName(__instance.m_character.gameObject) + level;
                if (LevelEffects.m_materials.TryGetValue(key, out Material material))
                {
                    Material[] sharedMaterials = __instance.m_mainRender.sharedMaterials;
                    sharedMaterials[0] = material;
                    __instance.m_mainRender.sharedMaterials = sharedMaterials;
                }
                else
                {
                    Material[] sharedMaterials = __instance.m_mainRender.sharedMaterials;
                    sharedMaterials[0] = new Material(sharedMaterials[0]);
                    sharedMaterials[0].SetFloat(Hue, levelSetup.m_hue);
                    sharedMaterials[0].SetFloat(Saturation, levelSetup.m_saturation);
                    sharedMaterials[0].SetFloat(Value, levelSetup.m_value);
                    if (levelSetup.m_setEmissiveColor)
                    {
                        sharedMaterials[0].SetColor(EmissionColor, levelSetup.m_emissiveColor);
                    }

                    __instance.m_mainRender.sharedMaterials = sharedMaterials;
                    LevelEffects.m_materials[key] = sharedMaterials[0];
                }
            }

            if (__instance.m_baseEnableObject)
            {
                __instance.m_baseEnableObject.SetActive(false);
            }

            if (!levelSetup.m_enableObject) return;
            levelSetup.m_enableObject.SetActive(true);
        }
    }
}