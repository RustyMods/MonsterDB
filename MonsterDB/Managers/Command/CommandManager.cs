using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;

namespace MonsterDB;

public static class CommandManager
{
    private static readonly string startCommand;
    public static readonly Dictionary<string, Command> commands;

    static CommandManager()
    {
        commands = new Dictionary<string, Command>();
        Harmony harmony = MonsterDBPlugin.harmony;
        startCommand = MonsterDBPlugin.ModName.ToLower();

        harmony.Patch(AccessTools.Method(typeof(Terminal), nameof(Terminal.Awake)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(CommandManager), 
                nameof(Patch_Terminal_Awake))));
        harmony.Patch(AccessTools.Method(typeof(Terminal), nameof(Terminal.updateSearch)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(CommandManager),
                nameof(Patch_Terminal_UpdateSearch))));
        harmony.Patch(AccessTools.Method(typeof(Terminal), nameof(Terminal.tabCycle)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(CommandManager),
                nameof(Patch_Terminal_TabCycle))));
    }

    private static void Patch_Terminal_Awake()
    {
        _ = new Terminal.ConsoleCommand(startCommand, "use help to find available commands", args =>
        {
            if (args.Length < 2) return false;
            if (!commands.TryGetValue(args[1], out Command data)) return false;
            return data.Run(args);
        },  optionsFetcher: commands
            .Where(x => !x.Value.IsSecret())
            .Select(x => x.Key)
            .ToList);

        _ = new Command("help", "list of available commands", _ =>
        {
            StringBuilder sb = new StringBuilder();
            foreach (var command in commands)
            {
                if (command.Value.IsSecret()) continue;

                sb.Clear();
                sb.AppendFormat("{0}: {1}", command.Key, command.Value.m_description);
                if (command.Value.m_adminOnly)
                {
                    sb.Append(" (admin only)");
                }
                MonsterDBPlugin.LogInfo(sb.ToString());
            }
            return true;
        });
    }

    private static bool Patch_Terminal_UpdateSearch(Terminal __instance, string word)
    {
        if (__instance.m_search == null) return true;
        string[] strArray = __instance.m_input.text.Split(' ');
        if (strArray.Length < 3) return true;
        if (strArray[0] != startCommand) return true;
        return HandleSearch(__instance, word, strArray);
    }
    
    private static bool HandleSearch(Terminal __instance, string word, string[] strArray)   
    {
        if (!commands.TryGetValue(strArray[1], out Command command)) return true;
        if (command.HasOptions() && strArray.Length == 3)
        {
            List<string> list = command.FetchOptions();
            List<string> filteredList;
            string currentSearch = strArray[2];
            if (!currentSearch.IsNullOrWhiteSpace())
            {
                int indexOf = list.IndexOf(currentSearch);
                filteredList = indexOf != -1 ? list.GetRange(indexOf, list.Count - indexOf) : list;
                filteredList = filteredList.FindAll(x => x.ToLower().Contains(currentSearch.ToLower()));
            }
            else filteredList = list;
            if (filteredList.Count <= 0) __instance.m_search.text = command.m_description;
            else
            {
                __instance.m_lastSearch.Clear();
                __instance.m_lastSearch.AddRange(filteredList);
                __instance.m_lastSearch.Remove(word);
                __instance.m_search.text = "";
                int maxShown = 10;
                int count = Math.Min(__instance.m_lastSearch.Count, maxShown);
                for (int index = 0; index < count; ++index)
                {
                    string text = __instance.m_lastSearch[index];
                    __instance.m_search.text += text + " ";
                }
    
                if (__instance.m_lastSearch.Count <= maxShown) return false;
                int remainder = __instance.m_lastSearch.Count - maxShown;
                __instance.m_search.text += $"... {remainder} more.";
            }
        }
        else __instance.m_search.text = command.m_description;
                
        return false;
    }

    private static void Patch_Terminal_TabCycle(string word, ref List<string>? options)
    {
        if (commands.TryGetValue(word, out Command? command))
        {
            options = command.FetchOptions();
        }
    }
}

public static partial class Extensions
{
    public static string GetString(this Terminal.ConsoleEventArgs args, int index, string defaultValue = "")
    {
        if (args.Length < index + 1) return defaultValue;
        return args[index];
    }

    public static float GetFloat(this Terminal.ConsoleEventArgs args, int index, float defaultValue = 0f)
    {
        if (args.Length < index + 1) return defaultValue;
        string? arg = args[index];
        return float.TryParse(arg, out float result) ? result : defaultValue;
    }

    public static int GetInt(this Terminal.ConsoleEventArgs args, int index, int defaultValue = 0)
    {
        if (args.Length < index + 1) return defaultValue;
        string? arg = args[index];
        return int.TryParse(arg, out int result) ? result : defaultValue;
    }
}

