using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MonsterDB.Solution.Methods;
using UnityEngine;

namespace MonsterDB.Solution;

public static class Commands
{
    private static readonly Dictionary<string, CommandInfo> m_commands = new();
    private static List<string> MonsterNames = new();
    private static List<string> ItemNames = new();

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.updateSearch))]
    private static class Terminal_UpdateSearch_Patch
    {
        private static bool Prefix(Terminal __instance, string word)
        {
            if (!ZNetScene.instance) return true;
            if (__instance.m_search == null) return true;
            string[] strArray = __instance.m_input.text.Split(' ');
            if (strArray.Length < 3) return true;
            if (strArray[0] != "monsterdb") return true;
            HandleSearch(__instance, word, strArray);
            return false;
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
    private static class Terminal_Awake_Patch
    {
        private static void Postfix()
        {
            CommandInfo convert = new("convert", "Reads json files in import folder", _ =>
            {
                RRRConverter.ReadAllRRRFiles();
            });
            CommandInfo help = new("help", "Use to list out MonsterDB commands", _ =>
            {
                foreach (CommandInfo command in m_commands.Values)
                {
                    if (!command.m_show) continue;
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"{command.m_input} - {command.m_description}");
                }
            });
            CommandInfo reload = new("reload", "Reloads all MonsterDB files", _ =>
            {
                Initialization.RemoveAllClones();
                Initialization.ReadLocalFiles();
                Initialization.CloneAll();
                Initialization.UpdateAll();
            });
            CommandInfo import = new("import", "Imports all creature files from Import folder, and reloads data", _ =>
            {
                CreatureManager.Import();
                Initialization.RemoveAllClones();
                Initialization.CloneAll();
                Initialization.UpdateAll();
            });
            CommandInfo write_all = new("write_all", "Writes all creatures data to disk", _ =>
            {
                if (!ZNetScene.instance) return;
                foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
                {
                    if (!prefab.GetComponent<Character>()) continue;
                    Write(prefab, true);
                }
            });
            CommandInfo write = new("write", "[prefabName] Writes creature data to disk", args =>
            {
                if (args.Length < 3 || !ZNetScene.instance) return;
                string prefabName = args[2];
                GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                    return;
                }
                Write(prefab);
            });
            CommandInfo update = new("update", "[prefabName] Reads files and updates creature", args =>
            {
                if (args.Length < 3 || !ZNetScene.instance) return;
                string prefabName = args[2];
                GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                    return;
                }
                CreatureManager.Read(prefab.name, CreatureManager.IsClone(prefab));
                CreatureManager.Update(prefab, false);
                MonsterDBPlugin.MonsterDBLogger.LogInfo($"Updated {prefab.name}");
            });
            CommandInfo clone = new("clone", "[prefabName] [cloneName] Clones creature, and saves to disk", args =>
            {
                if (args.Length < 4 || !ZNetScene.instance) return;
                string prefabName = args[2];
                GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                    return;
                }
                var cloneName = args[3];
                CreatureManager.Clone(prefab, cloneName);
            });
            CommandInfo reset = new("reset", "[prefabName] Resets creature data to original state", args =>
            {
                if (args.Length < 3 || !ZNetScene.instance) return;
                string prefabName = args[2];
                GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                    return;
                }
                CreatureManager.Reset(prefab);
            });
            CommandInfo write_item = new("write_item", "[prefabName] Saves itemData to file for reference", args =>
            {
                if (args.Length < 3 || !ZNetScene.instance) return;
                string prefabName = args[2];
                GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                    return;
                }
                if (!prefab.GetComponent<ItemDrop>()) return;
                ItemDataMethods.Write(prefab);
            });
            CommandInfo clone_item = new("clone_item", "[prefabName] [cloneName] Clones item to use as a new attack for creatures", args =>
            {
                if (args.Length < 4 || !ZNetScene.instance) return;
                string prefabName = args[2];
                GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                    return;
                }
                string itemName = args[3];
                ItemDataMethods.Clone(prefab, itemName, true);
            });
            CommandInfo write_spawn = new("write_spawn", "[prefabName] Writes to disk spawn data, of current spawn system", args =>
            {
                if (args.Length < 3 || !ZNetScene.instance) return;
                string prefabName = args[2];
                GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                    return;
                }

                MonsterDBPlugin.MonsterDBLogger.LogInfo(SpawnMan.Write(prefab)
                    ? "Wrote spawn data to disk"
                    : "Failed to find spawn data");
            });
            CommandInfo export = new("export", "[prefabName] Writes creature data to a single YML document to share", args =>
            {
                if (args.Length < 3 || !ZNetScene.instance) return;
                string prefabName = args[2];
                GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                if (prefab == null)
                {
                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                    return;
                }
                CreatureManager.Export(prefabName);
            });
            Terminal.ConsoleCommand mainCommand = new Terminal.ConsoleCommand("monsterdb", "use help to list out commands", (Terminal.ConsoleEventFailable)(args =>
            {
                if (args.Length < 2) return false;
                if (!m_commands.TryGetValue(args[1], out CommandInfo command)) return false;
                command.Run(args);
                return true;
            }), onlyAdmin: true, optionsFetcher: m_commands.Keys.ToList);
        }
    }
    private static void HandleSearch(Terminal __instance, string word, string[] strArray)
    {
        if (word is "reload" or "import" or "write_spawn" or "help")
        {
            __instance.m_search.text = word switch
            {
                "reload" => "<color=red>Reloads all MonsterDB files</color>",
                "import" => "<color=yellow>Imports all creature files from Import folder, and reload data</color>",
                "write_spawn" =>
                    "<color=yellow>Tries to export spawn system data of creature</color>, <color=red>location dependant</color>",
                "help" => "<color=white>Lists out MonsterDB commands and descriptions</color>",
                _ => ""
            };
        }
        else
        {
            List<string> list = word switch
            {
                "write" or "clone" => GetMonsterList(),
                "update" or "reset" => GetUpdateList(),
                "write_item" or "clone_item" => GetItemList(),
                _ => new List<string>()
            };
            list.Sort();

            List<string> output;
            string currentSearch = strArray[2];

            if (!currentSearch.IsNullOrWhiteSpace())
            {
                int indexOf = list.IndexOf(currentSearch);
                output = indexOf != -1 ? list.GetRange(indexOf, list.Count - indexOf) : list;
                output = output.FindAll(x => x.ToLower().Contains(currentSearch.ToLower()));
            }
            else output = list;

            __instance.m_lastSearch.Clear();
            __instance.m_lastSearch.AddRange(output);
            __instance.m_lastSearch.Remove(word);
            __instance.m_search.text = "";

            int maxShown = 10;
            int count = Math.Min(__instance.m_lastSearch.Count, maxShown);

            for (int index = 0; index < count; ++index)
            {
                string text = __instance.m_lastSearch[index];
                int num = text.ToLower().IndexOf(word.ToLower(), StringComparison.Ordinal);
                __instance.m_search.text += __instance.safeSubstring(text, 0, num) + "  ";
            }

            if (__instance.m_lastSearch.Count <= maxShown) return;
            int remainder = __instance.m_lastSearch.Count - maxShown;
            __instance.m_search.text += $"... {remainder} more.";
        }
    }

    private static void Write(GameObject prefab, bool writeAll = false)
    {
        if (prefab.GetComponent<Player>()) return;
        if (CreatureManager.IsClone(prefab))
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Creature is a MonsterDB clone, will not write to disk");
            return;
        }

        if (!CreatureManager.Write(prefab, out string folderPath, writeAll: writeAll)) return;
        MonsterDBPlugin.MonsterDBLogger.LogInfo($"Saved {prefab.name} at:");
        MonsterDBPlugin.MonsterDBLogger.LogInfo(folderPath);
    }

    private static List<string> GetMonsterList()
    {
        if (MonsterNames.Count > 0) return MonsterNames;
        List<string> output = new();
        foreach (var prefab in ZNetScene.instance.m_prefabs)
        {
            if (CreatureManager.IsClone(prefab)) continue;
            if (prefab.GetComponent<Character>()) output.Add(prefab.name);
        }

        MonsterNames = output;
        return output;
    }

    private static List<string> GetItemList()
    {
        if (ItemNames.Count > 0) return ItemNames;
        List<string> output = new();
        foreach (GameObject prefab in DataBase.m_allObjects.Values)
        {
            if (prefab == null) continue;
            if (ItemDataMethods.IsClone(prefab)) continue;
            if (prefab.GetComponent<ItemDrop>()) output.Add(prefab.name);
        }

        ItemNames = output;
        return output;
    }

    private static List<string> GetUpdateList() => CreatureManager.m_data.Keys.ToList();
    
    private class CommandInfo
    {
        public readonly string m_input;
        public readonly string m_description;
        public readonly bool m_show;
        private readonly Action<Terminal.ConsoleEventArgs> m_command;
        public void Run(Terminal.ConsoleEventArgs args) => m_command(args);
        public CommandInfo(string input, string description, Action<Terminal.ConsoleEventArgs> command, bool show = true)
        {
            m_input = input;
            m_description = description;
            m_command = command;
            m_show = show;
            m_commands[m_input] = this;
        }
    }
}