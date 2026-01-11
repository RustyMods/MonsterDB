using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
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

public sealed class FactionYamlConverter : IYamlTypeConverter
{
    public static readonly IYamlTypeConverter Instance = new FactionYamlConverter();
    public bool Accepts(Type type)
        => Nullable.GetUnderlyingType(type) == typeof(Character.Faction);

    public object ReadYaml(IParser parser, Type type)
    {
        Scalar scalar = parser.Consume<Scalar>();
        return FactionManager.GetFaction(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        Character.Faction faction = (Character.Faction)value;
        emitter.Emit(new Scalar(faction.ToString()));
    }
}