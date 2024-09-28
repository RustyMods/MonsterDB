using System.Collections.Generic;
using HarmonyLib;
using MonsterDB.Solution.Methods;

namespace MonsterDB.Solution;

public static class Commands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
    private static class Terminal_Awake_Patch
    {
        private static void Postfix() => LoadCommands();
    }

    private static void LoadCommands()
    {
        Terminal.ConsoleCommand commands = new Terminal.ConsoleCommand("monsterdb", "use help to list out commands",
            (Terminal.ConsoleEventFailable)(
                args =>
                {
                    if (args.Length < 2) return false;

                    switch (args[1])
                    {
                        case "help":
                            foreach (string command in new List<string>()
                                     {
                                         "write [prefabName] - write creature data to disk", 
                                         "update [prefabName] - reads files and updates creature",
                                         "clone [prefabName] [cloneName] - clones creature, and saves to disk", 
                                         "reset [prefabName] - resets creature data to original state",
                                         "reload - reloads all MonsterDB files",
                                         "save [prefabName] - Saves ItemData to file for reference"
                                     })
                            {
                                MonsterDBPlugin.MonsterDBLogger.LogInfo(command);
                            }
                            break;
                        case "write" or "update" or "clone" or "reset" or "save":
                            if (args.Length < 3 || !ZNetScene.instance) return false;
                            string prefabName = args[2];
                            var prefab = DataBase.TryGetGameObject(prefabName);
                            if (prefab == null) return false;
                            switch (args[1])
                            {
                                case "write":
                                    if (CreatureManager.IsClone(prefab))
                                    {
                                        MonsterDBPlugin.MonsterDBLogger.LogInfo("Creature is a MonsterDB clone, will not write to disk");
                                        return false;
                                    }
                                    CreatureManager.Write(prefab, out string folderPath);
                                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Saved {prefabName} at:");
                                    MonsterDBPlugin.MonsterDBLogger.LogInfo(folderPath);
                                    break;
                                case "update":
                                    CreatureManager.Read(prefab.name, CreatureManager.IsClone(prefab));
                                    CreatureManager.Update(prefab);
                                    MonsterDBPlugin.MonsterDBLogger.LogInfo($"Updated {prefabName}");
                                    break;
                                case "clone":
                                    if (args.Length < 4) return false;
                                    var cloneName = args[3];
                                    CreatureManager.Clone(prefab, cloneName);
                                    break;
                                case "reset":
                                    CreatureManager.Reset(prefab);
                                    break;
                                case "save":
                                    if (!prefab.GetComponent<ItemDrop>()) return false;
                                    ItemDataMethods.Write(prefab);
                                    break;
                            }
                            break;
                        case "reload":
                            Initialization.RemoveAll();
                            Initialization.CloneAll();
                            Initialization.UpdateAll();
                            break;
                    }
                    
                    return true;
                }), onlyAdmin: true, optionsFetcher: () => new List<string>
            {
                "help", "write", "update", "clone", "reset", "reload", "save"
            });
    }
}