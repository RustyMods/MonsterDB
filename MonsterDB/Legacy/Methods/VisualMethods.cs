using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class VisualMethods
{
    public static void ReadVisuals(string folderPath, ref CreatureData creatureData)
    {
        string visualFolderPath = folderPath + Path.DirectorySeparatorChar + "Visual";
        if (!Directory.Exists(visualFolderPath)) return;
        string scaleFilePath = visualFolderPath + Path.DirectorySeparatorChar + "Scale.yml";
        if (!File.Exists(scaleFilePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string serial = File.ReadAllText(scaleFilePath);
        if (serial.IsNullOrWhiteSpace()) return;
        try
        {
            ScaleData scaleData = deserializer.Deserialize<ScaleData>(serial);
            creatureData.m_scale = scaleData;
        }
        catch
        {
            LogParseFailure(scaleFilePath);
        }

        string humanFilePath = visualFolderPath + Path.DirectorySeparatorChar + "Human.yml";
        if (File.Exists(humanFilePath))
        {
            try
            {
                var humanData = deserializer.Deserialize<HumanData>(File.ReadAllText(humanFilePath));
                creatureData.m_humanData = humanData;
            }
            catch
            {
                LogParseFailure(humanFilePath);
            }
        }

        string ragDollFilePath = visualFolderPath + Path.DirectorySeparatorChar + "RagdollScale.yml";
        if (File.Exists(ragDollFilePath))
        {
            try
            {
                creatureData.m_ragdollScale = deserializer.Deserialize<ScaleData>(File.ReadAllText(ragDollFilePath));
            }
            catch
            {
                LogParseFailure(ragDollFilePath);
            }
        }
        
        string materialFolderPath = visualFolderPath + Path.DirectorySeparatorChar + "Materials";
        if (!Directory.Exists(materialFolderPath)) return;
        string[] materialPaths = Directory.GetFiles(materialFolderPath);
        Dictionary<string, MaterialData> dataMap = new();
        foreach (string path in materialPaths)
        {
            string materialSerial = File.ReadAllText(path);
            if (materialSerial.IsNullOrWhiteSpace()) continue;
            try
            {
                MaterialData materialData = deserializer.Deserialize<MaterialData>(materialSerial);
                dataMap[materialData.Name] = materialData;
            }
            catch
            {
                LogParseFailure(path);
            }
        }

        creatureData.m_materials = dataMap;

        string particleFilePath = visualFolderPath + Path.DirectorySeparatorChar + "Particles.yml";
        if (!File.Exists(particleFilePath)) return;
        try
        {
            var particleData = deserializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(particleFilePath));
            creatureData.m_particles = particleData;
        }
        catch
        {
            LogParseFailure(particleFilePath);
        }
    }

    public static Color GetColor(ColorData data) => new Color(data.r, data.g, data.b, data.a);

    [Serializable]
    public class HumanData
    {
        public int ModelIndex = 0;
        public int BeardIndex = 0;
        public int HairIndex = 0;
    }
    
    [Serializable]
    public class ScaleData
    {
        public float x;
        public float y;
        public float z;
        
        public Vector3Ref ToRef() => new Vector3Ref(x, y, z);
    }
    
    [Serializable]
    public class MaterialData
    {
        public string ShaderType = "";
        public string Name = "";
        public ColorData _Color = new();
        public string _MainTex = "";
        public ColorData _EmissionColor = new();
        public bool Transparent;
        public float _Hue;
        public float _Value;
        public float _Saturation;

        public void Set(MaterialRef r)
        {
            r.m_shader = ShaderType;
            r.m_color = _Color.ToHex();
            r.m_emissionColor = _EmissionColor.ToHex();
            r.m_hue = _Hue;
            r.m_value = _Value;
            r.m_saturation = _Saturation;
            r.m_mainTexture = _MainTex;
        }
    }

    [Serializable]
    public class ColorData
    {
        public float r;
        public float g;
        public float b;
        public float a;
        
        public Color ToColor() => new Color(r, g, b, a);
        public string ToHex() => ToColor().ToHex();
    }
}