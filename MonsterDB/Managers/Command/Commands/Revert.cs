using System;
using System.Collections.Generic;
using System.Linq;


namespace MonsterDB;

public static partial class Commands
{
    private static readonly List<string> RevertOptions = GetBaseTypes();
    
    private static void Revert(Terminal.ConsoleEventArgs args)
    {
        string type = args.GetString(2);
        if (string.IsNullOrEmpty(type))
        {
            args.Context.LogWarning("Specify type");
            return;
        }
        
        switch (type)
        {
            default:
                if (Enum.TryParse(type, true, out BaseType baseType))
                {
                    RevertBase(baseType, args);
                }
                else
                {
                    args.Context.LogWarning("Invalid type");
                }
                break;
        }
    }

    private static void RevertBase(BaseType baseType, Terminal.ConsoleEventArgs args)
    {
        string prefabName = args.GetString(3);
        if (string.IsNullOrEmpty(prefabName))
        {
            args.Context.LogWarning("Specify prefab");
            return;
        }
        
        switch (baseType)
        {
            case BaseType.None: break;
            case BaseType.All:
                LoadManager.ResetAll<Header>();
                args.Context.AddString($"Reverted All");
                break;
            default:
                LoadManager.Reset<Header>(prefabName);
                args.Context.AddString($"Reverted {prefabName}");
                break;
        }
    }

    private static List<string> GetRevertOptions(int i, string word) => i switch
    {
        2 => GetBaseTypes(),
        3 => Enum.TryParse(word, true, out BaseType type)
            ? GetCloneOptionByBase(type)
            : PrefabManager.GetAllPrefabNames(),
        _ => new List<string>()
    };

}