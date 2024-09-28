using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class VisualMethods
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    public static void Write(GameObject critter, string folderPath)
    {
        string visualFolderPath = folderPath + Path.DirectorySeparatorChar + "Visual";
        if (!Directory.Exists(visualFolderPath)) Directory.CreateDirectory(visualFolderPath);
        string materialFolderPath = visualFolderPath + Path.DirectorySeparatorChar + "Materials";
        if (!Directory.Exists(materialFolderPath)) Directory.CreateDirectory(materialFolderPath);
        Vector3 scale = critter.transform.localScale;
        ScaleData scaleData = new ScaleData()
        {
            x = scale.x,
            y = scale.y,
            z = scale.z
        };
        string scaleFilePath = visualFolderPath + Path.DirectorySeparatorChar + "Scale.yml";
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(scaleData);
        File.WriteAllText(scaleFilePath, serial);
        
        SkinnedMeshRenderer[] renderers = critter.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials)
            {
                string name = material.name;
                string shader = material.shader.name;
                MaterialData materialData = new();
                materialData.Name = name;
                materialData.ShaderType = shader;
                if (material.HasProperty("_Color"))
                {
                    materialData._Color = new ColorData()
                    {
                        r = material.color.r,
                        g = material.color.g,
                        b = material.color.b,
                        a = material.color.a
                    };
                }
                if (material.mainTexture) materialData._MainTex = material.mainTexture.name;
                if (material.HasColor(EmissionColor))
                {
                    var emissionColor = material.GetColor(EmissionColor);
                    materialData._EmissionColor = new ColorData()
                    {
                        r = emissionColor.r,
                        g = emissionColor.g,
                        b = emissionColor.b,
                        a = emissionColor.a
                    };
                }

                string materialFilePath = materialFolderPath + Path.DirectorySeparatorChar + name + ".yml";
                string materialSerial = serializer.Serialize(materialData);
                File.WriteAllText(materialFilePath, materialSerial);
            }
        }
        WriteArmatureStructure(critter, visualFolderPath);
    }

    private static void WriteArmatureStructure(GameObject critter, string folderPath)
    {
        string filePath = Path.Combine(folderPath, "Visual.txt");
        var visual = critter.transform.Find("Visual");
        if (!visual) return;
        List<string> output = new List<string>();
        BuildStructureRecursive(visual, output, 0);
        File.WriteAllLines(filePath, output);
    }

    private static void BuildStructureRecursive(Transform currentTransform, List<string> output, int depth)
    {
        output.Add(new string('\t', depth) + currentTransform.name);
        foreach (Transform child in currentTransform)
        {
            BuildStructureRecursive(child, output, depth + 1);
        }
    }
    
    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        var materialExport = new Dictionary<string, MaterialData>();
        Vector3 scale = critter.transform.localScale;
        creatureData.m_scale = new ScaleData()
        {
            x = scale.x,
            y = scale.y,
            z = scale.z
        };
        SkinnedMeshRenderer[] renderers = critter.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials)
            {
                string name = material.name;
                string shader = material.shader.name;
                MaterialData materialData = new();
                materialData.Name = name;
                materialData.ShaderType = shader;
                if (material.HasProperty("_Color"))
                {
                    materialData._Color = new ColorData()
                    {
                        r = material.color.r,
                        g = material.color.g,
                        b = material.color.b,
                        a = material.color.a
                    };
                }
                if (material.mainTexture) materialData._MainTex = material.mainTexture.name;
                if (material.HasColor(EmissionColor))
                {
                    var emissionColor = material.GetColor(EmissionColor);
                    materialData._EmissionColor = new ColorData()
                    {
                        r = emissionColor.r,
                        g = emissionColor.g,
                        b = emissionColor.b,
                        a = emissionColor.a
                    };
                }

                materialExport[name] = materialData;
            }
        }

        creatureData.m_materials = materialExport;
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
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
    }

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        Vector3 scale = GetScale(creatureData.m_scale);
        Dictionary<string, MaterialData> materialData = creatureData.m_materials;
        critter.transform.localScale = scale;
        SkinnedMeshRenderer[] renderers = critter.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials)
            {
                string name = material.name;
                if (!materialData.TryGetValue(name, out MaterialData data)) continue;
                material.name = name;
                material.color = GetColor(data._Color);
                if (TextureManager.RegisteredTextures.TryGetValue(data._MainTex, out Texture2D texture2D))
                {
                    material.mainTexture = texture2D;
                }
                else if (DataBase.m_textures.TryGetValue(data._MainTex, out texture2D))
                {
                    material.mainTexture = texture2D;
                }
                if (material.HasProperty(EmissionColor))
                {
                    material.SetColor(EmissionColor, GetColor(data._EmissionColor));
                }
            }

            renderer.sharedMaterials = materials.ToArray();
            renderer.materials = materials.ToArray();
        }
    }

    public static Dictionary<string, Material> CloneMaterials(GameObject critter)
    {
        Dictionary<string, Material> output = new();
        var renderers = critter.GetComponentsInChildren<SkinnedMeshRenderer>();
        int count = 0;
        foreach (var renderer in renderers)
        {
            List<Material> sharedMats = new();
            foreach (var material in renderer.sharedMaterials)
            {
                var mat = new Material(material);
                mat.name = critter.name + "_" + count;
                sharedMats.Add(mat);
                ++count;
                output[material.name] = mat;
            }

            renderer.sharedMaterials = sharedMats.ToArray();
            renderer.materials = sharedMats.ToArray();
        }

        return output;
    }

    public static void SetMaterials(GameObject ragDoll, Dictionary<string, Material> materials)
    {
        var renderers = ragDoll.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var renderer in renderers)
        {
            List<Material> sharedMats = new();
            foreach (var material in renderer.sharedMaterials)
            {
                sharedMats.Add(!materials.TryGetValue(material.name, out Material newMat) ? material : newMat);
            }
            renderer.sharedMaterials = sharedMats.ToArray();
            renderer.materials = sharedMats.ToArray();
        }
    }

    private static Color GetColor(ColorData data) => new Color(data.r, data.g, data.b, data.a);

    [Serializable]
    public class ScaleData
    {
        public float x;
        public float y;
        public float z;
    }
    
    [Serializable]
    public class MaterialData
    {
        public string ShaderType = "";
        public string Name = "";
        public ColorData _Color = new();
        public string _MainTex = "";
        public ColorData _EmissionColor = new();
    }

    [Serializable]
    public class ColorData
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }
}