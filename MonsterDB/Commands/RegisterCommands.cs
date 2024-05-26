using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MonsterDB.DataBase;
using UnityEngine;

namespace MonsterDB.Commands;

public static class RegisterCommands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
    private static class RegisterCommandsPatch
    {
        private static void Postfix() => AddCommands();
    }

    private static void AddCommands()
    {
        Terminal.ConsoleCommand write = new("monster_db",
            "MonsterDB console commands, use help to list out available commands", (Terminal.ConsoleEventFailable)(
                args =>
                {
                    if (!ZNet.instance) return false;
                    if (!ZNet.instance.IsServer())
                    {
                        MonsterDBPlugin.MonsterDBLogger.LogInfo("MonsterDB commands only usable by server");
                        return false;
                    }
                    if (args.Length < 2) return false;
                    switch (args[1])
                    {
                        case "help":
                            List<string> info = new()
                            {
                                "save [prefabName] = Writes to disk monster data",
                                "clone [prefabName] [customName] = clones creature and writes to disk clone data",
                                "update [prefabName] = updates creature with latest data",
                                "item [itemName] = prints cached item name",
                                "search texture [filter] = prints a list of textures where filter is contained in name",
                                "print textures = prints to file all the names of textures in available resources",
                                "reset [prefabName = Tries to resets monster to default settings"
                            };
                            foreach(string data in info) MonsterDBPlugin.MonsterDBLogger.LogInfo(data);
                            break;
                        case "save":
                            if (args.Length < 3) return false;
                            if (MonsterManager.Command_SaveMonsterData(args[2]))
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo("Successfully saved " + args[2] + " to file");
                            }
                            else
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to save" + args[2]);
                            }
                            break;
                        case "clone":
                            if (args.Length < 4) return false;
                            string cloneName = args[3].Replace(" ", "_");
                            if (MonsterManager.Command_CloneMonster(args[2], cloneName))
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo("Successfully cloned " + args[2] + " as " + cloneName);
                            }
                            else
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to clone " + args[2]);
                            }
                            break;
                        case "update":
                            if (args.Length < 3) return false;
                            if (MonsterManager.Command_UpdateMonster(args[2]))
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo("Successfully updated " + args[2]);
                            }
                            else
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update " + args[2]);
                            }
                            break;
                        case "item":
                            if (args.Length < 3) return false;
                            int count = 0;
                            foreach (GameObject item in DataBase.MonsterDB.GetCachedItems().Where(item => item.name.StartsWith(args[2]) || item.name.Contains(args[2]) || item.name.EndsWith(args[2])))
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo(item.name);
                                ++count;
                            }

                            if (count == 0)
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find any items matching " + args[2]);
                            }
                            break;
                        case "search":
                            if (args.Length < 4) return false;
                            switch (args[2])
                            {
                                case "textures":
                                    if (args[3] == "all")
                                    {
                                        foreach (var name in DataBase.MonsterDB.m_textures.Keys)
                                        {
                                            MonsterDBPlugin.MonsterDBLogger.LogInfo(name);
                                        }
                                    }
                                    else
                                    {
                                        List<string> textureNames = (from kvp in DataBase.MonsterDB.m_textures where kvp.Key.Contains(args[3]) select kvp.Key).ToList();
                                        foreach (var name in textureNames) MonsterDBPlugin.MonsterDBLogger.LogInfo(name);
                                    }
                                    break;
                            }
                            break;
                        case "print":
                            if (args.Length < 3) return false;
                            switch (args[2])
                            {
                                case "textures":
                                    string filePath = Paths.TexturePath + Path.DirectorySeparatorChar + "Textures.yml";
                                    List<string> textureNames = DataBase.MonsterDB.m_textures.Keys.ToList();
                                    File.WriteAllLines(filePath, textureNames);
                                    break;
                            }
                            break;
                        case "reset":
                            if (args.Length < 3) return false;
                            if (MonsterManager.ResetMonster(args[2]))
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo("Successfully reset " + args[2]);
                                if (MonsterManager.Command_SaveMonsterData(args[2]))
                                {
                                    MonsterDBPlugin.MonsterDBLogger.LogInfo("Saved default data to file");   
                                }
                            }
                            break;
                    }

                    return true;
                }), onlyAdmin: true ,optionsFetcher:ConsoleOptionsFetcher);
    }

    private static List<string> ConsoleOptionsFetcher()
    {
        return new()
        {
            "help", "save", "clone", "update", "items", "search", "print", "write", "reset"
        };
    }
}