using System.Collections.Generic;
using HarmonyLib;
using MonsterDB.Solution.Methods;
using UnityEngine;

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
                            Help();
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
                    }
                    
                    return true;
                }), onlyAdmin: true, optionsFetcher: () => new List<string>
            {
                "help", "write", "update", "clone", "reset", "reload", "write_item", "clone_item", "import", "export"
            });
    }

    private static void Help()
    {
        foreach (string command in new List<string>()
                 {
                     "write [prefabName] - write creature data to disk", 
                     "update [prefabName] - reads files and updates creature",
                     "clone [prefabName] [cloneName] - clones creature, and saves to disk", 
                     "reset [prefabName] - resets creature data to original state",
                     "reload - reloads all MonsterDB files",
                     "write_item [prefabName] - Saves ItemData to file for reference",
                     "clone_item [prefabName] [cloneName] - clones item to use as a new attack for creatures",
                     "write_spawn [prefabName] - Writes to disk spawn data, of current spawn system",
                     "import - Imports all creature files from Import folder, and reload data",
                     "export [prefabName] - Writes creature data to a single YML document to share"
                 })
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo(command);
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
        CreatureManager.Update(prefab);
        MonsterDBPlugin.MonsterDBLogger.LogInfo($"Updated {prefab.name}");
    }
}