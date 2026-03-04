namespace MonsterDB;

public static partial class Commands
{
    private static void RRRConvert(Terminal.ConsoleEventArgs args)
    {
        RRRConverter.Read();
        RRRConverter.ConvertAll(args);
    }
}