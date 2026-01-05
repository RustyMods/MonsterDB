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
    public static readonly Dictionary<string, Command> commands = new();

    static CommandManager()
    {
        Harmony harmony = MonsterDBPlugin.instance._harmony;
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

    private static bool Patch_Terminal_TabCycle(Terminal __instance, string word, List<string>? options, bool usePrefix)
    {
        if (options == null || options.Count == 0) return true;
        usePrefix = usePrefix && __instance.m_tabPrefix > char.MinValue;
        if (usePrefix)
        {
            if (word.Length < 1 || word[0] != __instance.m_tabPrefix) return true;
            word = word.Substring(1);
        }
        return HandleTabCycle(__instance, word, options, usePrefix);
    }
    
    private static bool HandleTabCycle(Terminal __instance, string word, List<string> options, bool usePrefix)
    {
        string currentInput = __instance.m_input.text;
        string[] inputParts = currentInput.Split(' ');

        if (!(inputParts.Length >= 2 && 
              String.Equals(inputParts[0], startCommand, StringComparison.CurrentCultureIgnoreCase) &&
              commands.ContainsKey(inputParts[1].ToLower())))
        {
            return true; // Let original method handle it
        }
        
        if (__instance.m_tabCaretPosition == -1)
        {
            __instance.m_tabOptions.Clear();
            __instance.m_tabCaretPosition = __instance.m_input.caretPosition;
            word = word.ToLower();
            __instance.m_tabLength = word.Length;
            
            if (__instance.m_tabLength == 0)
            {
                __instance.m_tabOptions.AddRange(options);
            }
            else
            {
                foreach (string option in options)
                {
                    if (option != null && option.Length > __instance.m_tabLength && 
                        option.Substring(0, __instance.m_tabLength).ToLower() == word)
                    {
                        __instance.m_tabOptions.Add(option);
                    }
                }
            }
            __instance.m_tabOptions.Sort();
            __instance.m_tabIndex = -1;
        }
        
        if (__instance.m_tabOptions.Count == 0)
            __instance.m_tabOptions.AddRange(__instance.m_lastSearch);
            
        if (__instance.m_tabOptions.Count == 0)
            return false;
        
        if (++__instance.m_tabIndex >= __instance.m_tabOptions.Count)
            __instance.m_tabIndex = 0;
        
        // Custom replacement logic for commands
        if (__instance.m_tabCaretPosition - __instance.m_tabLength >= 0)
        {
            // Find the position where the third argument (the option being completed) starts
            int spaceCount = 0;
            int thirdArgStart = 0;
            
            for (int i = 0; i < currentInput.Length; i++)
            {
                if (currentInput[i] == ' ')
                {
                    spaceCount++;
                    if (spaceCount == 2)
                    {
                        thirdArgStart = i + 1;
                        break;
                    }
                }
            }
            
            // Rebuild the command with the selected option
            if (inputParts.Length >= 3 && thirdArgStart > 0)
            {
                // Replace everything from the third argument onwards with the selected option
                string baseCommand = currentInput.Substring(0, thirdArgStart);
                __instance.m_input.text = baseCommand + __instance.m_tabOptions[__instance.m_tabIndex];
            }
            else if (inputParts.Length == 2)
            {
                // Add the selected option as the third argument
                __instance.m_input.text = currentInput + " " + __instance.m_tabOptions[__instance.m_tabIndex];
            }
        }
        
        __instance.m_tabCaretPositionEnd = __instance.m_input.caretPosition = __instance.m_input.text.Length;
        return false;
    }
    
}

