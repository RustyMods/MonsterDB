using YamlDotNet.Serialization;

namespace MonsterDB;

public sealed class StrippedCase : INamingConvention
{
    public static readonly INamingConvention Instance = new StrippedCase();
    public string Apply(string value)
    {
        return value.StartsWith("m_") ? value.Substring(2) : value;
    }
}