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
        private static void Postfix() => LoadCommands();
    }

    private static void LoadCommands()
    {
        Terminal.ConsoleCommand commands = new Terminal.ConsoleCommand("monsterdb", "use help to list out commands", (Terminal.ConsoleEventFailable)(args =>
        {
            if (args.Length < 2) return false;

            switch (args[1])
            {
                case "help":
                    Help();
                    break;
                case "reload":
                    Initialization.RemoveAllClones();
                    Initialization.CloneAll();
                    Initialization.UpdateAll();
                    break;
                case "import":
                    CreatureManager.Import();
                    Initialization.RemoveAllClones();
                    Initialization.CloneAll();
                    Initialization.UpdateAll();
                    break;
                case "write_all":
                    if (!ZNetScene.instance) return false;
                    WriteAll();
                    break;
                case "write" or "update" or "clone" or "reset" or "write_item" or "clone_item" or "write_spawn" or "export":
                    if (args.Length < 3 || !ZNetScene.instance) return false;
                    string prefabName = args[2];
                    GameObject? prefab = DataBase.TryGetGameObject(prefabName);
                    if (prefab == null)
                    {
                        MonsterDBPlugin.MonsterDBLogger.LogInfo($"Failed to find prefab: {prefabName}");
                        return false;
                    }
                    switch (args[1])
                    {
                        case "write":
                            Write(prefab);
                            break;
                        case "update":
                            Update(prefab);
                            break;
                        case "clone":
                            if (args.Length < 4) return false;
                            var cloneName = args[3];
                            CreatureManager.Clone(prefab, cloneName);
                            break;
                        case "reset":
                            CreatureManager.Reset(prefab);
                            break;
                        case "write_item":
                            if (!prefab.GetComponent<ItemDrop>()) return false;
                            ItemDataMethods.Write(prefab);
                            break;
                        case "clone_item":
                            if (args.Length < 4) return false;
                            string? itemName = args[3];
                            ItemDataMethods.Clone(prefab, itemName, true);
                            break;
                        case "write_spawn":
                            MonsterDBPlugin.MonsterDBLogger.LogInfo(SpawnMan.Write(prefab)
                                ? "Wrote spawn data to disk"
                                : "Failed to find spawn data");
                            break;
                        case "export":
                            CreatureManager.Export(prefabName);
                            break;
                    }
                    break;
            }
            
            return true;
        }), onlyAdmin: true, optionsFetcher: options);
    }

    private static readonly Terminal.ConsoleOptionsFetcher options = () => new List<string>()
    {
        "help", "write", "update", "clone", "reset", "reload", "write_item", "clone_item", "import", "export", "write_spawn"
    };
    private static void HandleSearch(Terminal __instance, string word, string[] strArray)
    {
        if (word is "reload" or "import" or "write_spawn" or "help")
        {
            __instance.m_search.text = word switch
            {
                "reload" => "<color=red>Reloads all MonsterDB files</color>",
                "import" => "<color=yellow>Imports all creature files from Import folder, and reload data</color>",
                "write_spawn" => "<color=yellow>Tries to export spawn system data of creature</color>, <color=red>location dependant</color>",
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

    private static void Help()
    {
        foreach (string command in new List<string>()
                 {
                     "write [prefabName] - Write creature data to disk", 
                     "update [prefabName] - Reads files and updates creature",
                     "clone [prefabName] [cloneName] - Clones creature, and saves to disk", 
                     "reset [prefabName] - Resets creature data to original state",
                     "reload - Reloads all MonsterDB files",
                     "write_item [prefabName] - Saves ItemData to file for reference",
                     "clone_item [prefabName] [cloneName] - Clones item to use as a new attack for creatures",
                     "write_spawn [prefabName] - Writes to disk spawn data, of current spawn system",
                     "import - Imports all creature files from Import folder, and reload data",
                     "export [prefabName] - Writes creature data to a single YML document to share"
                 })
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo(command);
        }
    }

    private static void WriteAll()
    {
        foreach (var prefab in ZNetScene.instance.m_prefabs)
        {
            if (!prefab.GetComponent<Character>()) continue;
            Write(prefab);
        }
    }

    private static void Write(GameObject prefab)
    {
        if (CreatureManager.IsClone(prefab))
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Creature is a MonsterDB clone, will not write to disk");
            return;
        }

        CreatureManager.Write(prefab, out string folderPath);
        MonsterDBPlugin.MonsterDBLogger.LogInfo($"Saved {prefab.name} at:");
        MonsterDBPlugin.MonsterDBLogger.LogInfo(folderPath);
    }

    private static void Update(GameObject prefab)
    {
        CreatureManager.Read(prefab.name, CreatureManager.IsClone(prefab));
        CreatureManager.Update(prefab, false);
        MonsterDBPlugin.MonsterDBLogger.LogInfo($"Updated {prefab.name}");
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
        foreach (var prefab in DataBase.m_allObjects.Values)
        {
            if (prefab == null) continue;
            if (ItemDataMethods.IsClone(prefab)) continue;
            if (prefab.GetComponent<ItemDrop>()) output.Add(prefab.name);
        }

        ItemNames = output;
        return output;
    }

    private static List<string> GetUpdateList() => CreatureManager.m_data.Keys.ToList();
    
}