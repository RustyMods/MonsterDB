using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using MonsterDB.Solution.Behaviors;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class VisualMethods
{
    private static readonly Dictionary<string, Shader> m_cachedShaders = new();
    private static readonly Dictionary<string, Material> m_clonedMaterials = new();

    public static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    public static readonly int Hue = Shader.PropertyToID("_Hue");
    public static readonly int Saturation = Shader.PropertyToID("_Saturation");
    public static readonly int Value = Shader.PropertyToID("_Value");

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

        if (critter.GetComponent<Human>())
        {
            string humanFilePath = visualFolderPath + Path.DirectorySeparatorChar + "Human.yml";
            File.WriteAllText(humanFilePath, serializer.Serialize(new HumanData()));
        }

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

        Dictionary<string, bool> m_particles = new();
        foreach (var particle in critter.GetComponentsInChildren<ParticleSystem>(true))
        {
            m_particles[particle.name] = particle.gameObject.activeSelf;
        }

        foreach (var light in critter.GetComponentsInChildren<Light>(true))
        {
            m_particles[light.name] = light.gameObject.activeSelf;
        }

        WriteArmatureStructure(critter, visualFolderPath);
        if (m_particles.Count <= 0) return;
        string particleFilePath = visualFolderPath + Path.DirectorySeparatorChar + "Particles.yml";
        File.WriteAllText(particleFilePath, serializer.Serialize(m_particles));
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

                if (material.HasProperty(Hue))
                {
                    materialData._Hue = material.GetFloat(Hue);
                }

                if (material.HasProperty(Saturation))
                {
                    materialData._Saturation = material.GetFloat(Saturation);
                }

                if (material.HasProperty(Value))
                {
                    materialData._Value = material.GetFloat(Value);
                }

                materialExport[name] = materialData;
            }
        }

        creatureData.m_materials = materialExport;
        
        Dictionary<string, bool> m_particles = new();
        foreach (var particle in critter.GetComponentsInChildren<ParticleSystem>(true))
        {
            m_particles[particle.name] = particle.gameObject.activeSelf;
        }

        foreach (var light in critter.GetComponentsInChildren<Light>(true))
        {
            m_particles[light.name] = light.gameObject.activeSelf;
        }

        creatureData.m_particles = m_particles;
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

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        Vector3 scale = GetScale(creatureData.m_scale);
        Dictionary<string, MaterialData> materialData = creatureData.m_materials;
        critter.transform.localScale = scale;
        SkinnedMeshRenderer[] renderers = critter.GetComponentsInChildren<SkinnedMeshRenderer>();
        bool changedLight = false;
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials)
            {
                string name = material.name;
                if (!materialData.TryGetValue(name, out MaterialData data)) continue;
                material.name = name;
                material.color = GetColor(data._Color);
                if (TextureManager.GetTex(data._MainTex) is { } tex)
                {
                    material.mainTexture = tex;
                }
                // if (TextureManager.m_customTextures.TryGetValue(data._MainTex, out Texture2D texture2D))
                // {
                //     material.mainTexture = texture2D;
                // }
                // else if (DataBase.m_textures.TryGetValue(data._MainTex, out texture2D))
                // {
                //     material.mainTexture = texture2D;
                // }
                if (material.HasProperty(EmissionColor)) material.SetColor(EmissionColor, GetColor(data._EmissionColor));
                if (material.HasProperty(Hue)) material.SetFloat(Hue, data._Hue);
                if (material.HasProperty(Value)) material.SetFloat(Value, data._Value);
                if (material.HasProperty(Saturation)) material.SetFloat(Saturation, data._Saturation);
                
                material.shader = GetShader(material.shader, data.ShaderType);
                if (data.Transparent) SetTransparent(material);
                else SetOpaque(material);

                if (!changedLight)
                {
                    var light = critter.GetComponentInChildren<Light>();
                    if (light)
                    {
                        light.color = material.color;
                        changedLight = true;
                    }
                }
            }

            renderer.sharedMaterials = materials.ToArray();
            renderer.materials = materials.ToArray();
        }

        foreach (var kvp in creatureData.m_particles)
        {
            var particle = Utils.FindChild(critter.transform, kvp.Key);
            particle.gameObject.SetActive(kvp.Value);
        }
    }

    public static void SetOpaque(Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    public static void SetTransparent(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
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
                m_clonedMaterials[material.name] = mat;
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

    private static Shader GetShader(Shader original, string name)
    {
        if (m_cachedShaders.Count <= 0) CacheShaders();
        return m_cachedShaders.TryGetValue(name, out Shader shader) ? shader : original;
    }

    private static void CacheShaders()
    {
        var assets = Resources.FindObjectsOfTypeAll<AssetBundle>();
        foreach (var bundle in assets)
        {
            IEnumerable<Shader>? shaders;
            try
            {
                shaders = bundle.isStreamedSceneAssetBundle && bundle
                    ? bundle.GetAllAssetNames().Select(bundle.LoadAsset<Shader>).Where(shader => shader != null)
                    : bundle.LoadAllAssets<Shader>();
            }
            catch (Exception)
            {
                continue;
            }

            if (shaders == null) continue;
            foreach (var shader in shaders)
            {
                m_cachedShaders[shader.name] = shader;
            }
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