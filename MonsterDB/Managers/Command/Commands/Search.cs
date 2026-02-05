using System.Collections.Generic;
using System.Linq;

namespace MonsterDB;

public static partial class Commands
{
    private static readonly List<string> SearchTypes = new() { "item", "texture", "tex", "sprite", "shader" };
    private static List<string> GetSearchOptions(int i, string word) => i switch
    {
        2 => SearchTypes,
        3 => GetSearchTypeOptions(word),
        _ => new List<string>()
    };

    private static List<string> GetSearchTypeOptions(string word) => word switch
    {
        "item" => PrefabManager.SearchCache<ItemDrop>(""),
        "texture" or "tex" => TextureManager.GetAllTextures().Keys.ToList(),
        "shader" => ShaderRef.GetShaderNames(),
        "sprite" => TextureManager.GetSpriteNames(),
        _ => new List<string>(),
    };

    private static string GetSearchDescriptions(string[] args, string defaultValue)
    {
        if (args.Length < 3) return defaultValue;
        string type = args[2];
        if (string.IsNullOrEmpty(type))
        {
            return defaultValue;
        }

        return $"<color={HEX_Gray}>[Query]</color>: Search {type} by name";
    }

    private static void Search(Terminal.ConsoleEventArgs args)
    {
        string type = args.GetString(2);
        if (string.IsNullOrEmpty(type))
        {
            args.Context.LogWarning("Specify type");
            return;
        }

        string query = args.GetStringFrom(3);
        if (string.IsNullOrEmpty(query))
        {
            args.Context.LogWarning("Specify query");
            return;
        }

        switch (type)
        {
            case "item":
                List<string> items = PrefabManager.SearchCache<ItemDrop>(query);
                for (int i = 0; i < items.Count; ++i)
                {
                    string name = items[i];
                    args.Context.AddString("- " + name);
                    MonsterDBPlugin.LogInfo(name);
                }
                break;
            case "texture" or "tex":
                List<string> textures = TextureManager.GetAllTextures().Keys.ToList();
                for (int i = 0; i < textures.Count; ++i)
                {
                    string name = textures[i];
                    if (name.ToLower().Contains(query.ToLower()))
                    {
                        args.Context.AddString("- " + name);
                        MonsterDBPlugin.LogInfo(name);
                    }
                }
                break;
            case "shader":
                List<string> shaders = ShaderRef.GetShaderNames();
                for (var i = 0; i < shaders.Count; ++i)
                {
                    string name = shaders[i];
                    if (name.ToLower().Contains(query.ToLower()))
                    {
                        args.Context.AddString("- " + name);
                        MonsterDBPlugin.LogInfo(name);
                    }
                }
                break;
            case "sprite":
                List<string> sprites = TextureManager.GetSpriteNames();
                for (int i = 0; i < sprites.Count; ++i)
                {
                    string? name = sprites[i];
                    if (name.ToLower().Contains(query.ToLower()))
                    {
                        args.Context.AddString("- " + name);
                        MonsterDBPlugin.LogInfo(name);
                    }
                }
                break;
        }
    }
    
}