using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (args.Length < 2)
            {
                return true;
            }
            if (!commands.TryGetValue(args[1], out Command data))
            {
                return true;
            }
            return data.Run(args);
        },  optionsFetcher: commands
            .Where(x => !x.Value.IsSecret())
            .Select(x => x.Key)
            .ToList);

        _ = new Command("help", "list of available commands", args =>
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, Command> command in commands)
            {
                if (command.Value.IsSecret()) continue;

                if (command.Key == "help") continue;
                
                sb.Clear();
                sb.AppendFormat("<color=yellow>{0}</color> - {1}", command.Key, command.Value.m_description);
                if (command.Value.m_adminOnly)
                {
                    sb.Append(" <color=red>(admin only)</color>");
                }
                args.Context.AddString(sb.ToString());
            }
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
        if (command.HasOptions() && strArray.Length > 2)
        {
            string option = strArray[2];
            
            List<string> list = command.FetchOptions(strArray.Length - 1, option);
            List<string> filteredList;
            string currentSearch = strArray[strArray.Length - 1];
            if (!string.IsNullOrEmpty(currentSearch))
            {
                int indexOf = list.IndexOf(currentSearch);
                filteredList = indexOf != -1 ? list.GetRange(indexOf, list.Count - indexOf) : list;
                filteredList = filteredList.FindAll(x => x.ToLower().Contains(currentSearch.ToLower()));
            }
            else
            {
                filteredList = list;
            }

            if (filteredList.Count <= 0)
            {
                __instance.m_search.text = command.GetDescription(strArray);
            }
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
        else __instance.m_search.text = command.GetDescription(strArray);
                
        return false;
    }

    private static void Patch_Terminal_TabCycle(Terminal __instance, ref List<string>? options)
    {
        if (string.IsNullOrEmpty(__instance.m_input.text))
        {
            return;
        }
        string[] strArray = __instance.m_input.text.Split(' ');
        if (strArray.Length < 2)
        {
            return;
        }
        string inputCommand = strArray[0];
        if (!string.Equals(startCommand, inputCommand, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        string type = strArray[1];
        string word = strArray.Length > 2 ? strArray[2] : "";
        
        if (commands.TryGetValue(type, out Command command))
        {
            options = command.FetchOptions(strArray.Length - 1, word);
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

    public static string GetStringFrom(this Terminal.ConsoleEventArgs args, int index, string defaultValue = "")
    {
        if (args.Length < index + 1) return defaultValue;
        return string.Join(" ", args.Args.Skip(index));
    }

    public static void LogWarning(this Terminal terminal, string msg)
    {
        terminal.Log("yellow", msg);
    }

    public static void LogError(this Terminal terminal, string msg)
    {
        terminal.Log("red", msg);
    }

    public static void Log(this Terminal terminal, string hex, string msg)
    {
        terminal.AddString($"<color={hex}>{msg}</color>");
    }
}

