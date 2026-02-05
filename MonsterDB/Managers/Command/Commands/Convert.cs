using System;
using System.Collections.Generic;
using System.Linq;

namespace MonsterDB;

public static partial class Commands
{
    private static List<string> GetConvertOptions(int i, string word) => i switch
    {
        2 => LegacyManager.creaturesToConvert.Keys.Append("all").ToList(),
        _ => new List<string>()
    };

    private static string GetConvertDescription(string[] args, string defaultValue)
    {
        if (args.Length < 3) return defaultValue;
        string type = args[2];
        if (string.IsNullOrEmpty(type))
        {
            return defaultValue;
        }

        return $"Convert {type} legacy file into v.0.2.x format";
    }

    private static void Convert(Terminal.ConsoleEventArgs args)
    {
        string input = args.GetString(2);
        if (string.IsNullOrEmpty(input))
        {
            args.Context.LogWarning("Invalid parameters");
            return;
        }

        if (input.Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
            LegacyManager.ConvertAll(args);
        }
        else
        {
            LegacyManager.Convert(args);
        }
    }
}