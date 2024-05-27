using System.IO;

namespace MonsterDB.DataBase;

public static class Paths
{
    private static readonly string FolderPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + "MonsterDB";
    public static readonly string MonsterPath = FolderPath + Path.DirectorySeparatorChar + "Monsters";
    public static readonly string TexturePath = FolderPath + Path.DirectorySeparatorChar + "Textures";
    public static readonly string SpawnPath = FolderPath + Path.DirectorySeparatorChar + "SpawnData";
    public static readonly string DataPath = FolderPath + Path.DirectorySeparatorChar + "Data";

    public static void CreateDirectories()
    {
        if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
        if (!Directory.Exists(MonsterPath)) Directory.CreateDirectory(MonsterPath);
        if (!Directory.Exists(TexturePath)) Directory.CreateDirectory(TexturePath);
        if (!Directory.Exists(SpawnPath)) Directory.CreateDirectory(SpawnPath);
        if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
    }
}