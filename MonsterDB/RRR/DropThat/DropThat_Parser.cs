using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MonsterDB;

public static partial class Extensions
{
    public static int GetInt(this string[] parts, int index, int defaultValue = 0)
    {
        if (parts.Length - 1 < index) return defaultValue;
        return int.TryParse(parts[index].Trim(), out int x) ? x : 0;
    }
    
    public static float GetFloat(this string[] parts, int index, float defaultValue = 0f)
    {
        if (parts.Length - 1 < index) return defaultValue;
        return float.TryParse(parts[index].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
    }

    public static bool GetBool(this string[] parts, int index, bool defaultValue = false)
    {
        if (parts.Length - 1 < index) return defaultValue;
        return bool.TryParse(parts[index].Trim(), out bool result) ? result : defaultValue;
    }
    
    public static string GetString(this string[] parts, int index, string defaultValue = "")
    {
        if (parts.Length - 1 < index) return defaultValue;
        return parts[index].Trim();
    }
}

public class DropDict : Dictionary<string, List<DropThat>>
{
    public void Add(DropThat drop)
    {
        if (!ContainsKey(drop.creature)) this[drop.creature] = new List<DropThat>();
        this[drop.creature].Add(drop);
    }

    public void Add(DropDict other)
    {
        foreach (List<DropThat> drop in other.Values)
        {
            Add(drop);
        }
    }

    public void Add(List<DropThat> drops)
    {
        for (int i = 0; i < drops.Count; ++i)
        {
            Add(drops[i]);
        }
    }

    public List<DropRef> ToDropRefs(string key) => TryGetValue(key, out List<DropThat>? list)
        ? list
            .Where(d => d.enabled)
            .Select(d => d.ToDropRef())
            .ToList()
        : new List<DropRef>();
}

public class DropThat_Parser
{
    public readonly DropDict drops = new DropDict();

    public void Parse(string filepath)
    {
        string[] lines = File.ReadAllLines(filepath);
        
        bool isBlock = false;
        DropThat block = new DropThat();
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                isBlock = true;
                
                string[] parts = line.Replace("[", string.Empty).Replace("]", string.Empty).Split('.');
                string creature = parts.GetString(0).Replace("RRRM_", string.Empty);
                int index = parts.GetInt(1);
                block.creature = creature;
                block.index = index;
                continue;
            }

            if (string.IsNullOrEmpty(line))
            {
                isBlock = false;
                if (block.isValid)
                {
                    drops.Add(block);
                    MonsterDBPlugin.LogDebug($"[ DropThat ]: {block.creature}, {block.prefab} ( {block.min} - {block.max} )");
                }
                block = new DropThat();
                continue;
            }

            if (isBlock)
            {
                string[] parts = line.Split('=');
                string key = parts.GetString(0);
                switch (key)
                {
                    case "PrefabName":
                        block.prefab = parts.GetString(1);
                        break;
                    case "SetAmountMin":
                        block.min = parts.GetInt(1, 1);
                        break;
                    case "SetAmountMax":
                        block.max = parts.GetInt(1, 1);
                        break;
                    case "SetChanceToDrop":
                        block.chance = parts.GetFloat(1, 100f);
                        break;
                    case "SetDropOnePerPlayer":
                        block.onePerPlayer = parts.GetBool(1, false);
                        break;
                    case "SetScaleByLevel":
                        block.scaleByLevel = parts.GetBool(1, true);
                        break;
                    case "EnableConfig":
                        block.enabled = parts.GetBool(1, true);
                        break;
                }
            }
        }
    }
}