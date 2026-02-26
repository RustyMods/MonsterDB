using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace MonsterDB;

public sealed class FieldPrefixStrippingNamingConvention : INamingConvention
{
    private readonly string prefix;
    public FieldPrefixStrippingNamingConvention(string prefix)
    {
        this.prefix = prefix;
    }
    public string Apply(string value)
    {
        return value.StartsWith(prefix) ? value.Substring(prefix.Length) : value;
    }
}

public sealed class FactionConverter : IYamlTypeConverter
{
    public static readonly IYamlTypeConverter Instance = new FactionConverter();
    public bool Accepts(Type type)
        => (Nullable.GetUnderlyingType(type) ?? type) == typeof(Character.Faction);

    public object ReadYaml(IParser parser, Type type)
    {
        Scalar scalar = parser.Consume<Scalar>();
        return FactionManager.GetFaction(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value == null) return;
        Character.Faction faction = (Character.Faction)value;
        emitter.Emit(new Scalar(faction.ToString()));
    }
}

public sealed class VectorConverter : IYamlTypeConverter
{
    public static readonly VectorConverter Instance = new();
    public bool Accepts(Type type)
    {
        return (Nullable.GetUnderlyingType(type) ?? type) == typeof(Vector3Ref);
    }
    public object ReadYaml(IParser parser, Type type)
    {
        // Check if it's a scalar value (single number)
        if (parser.Current is Scalar scalar)
        {
            parser.MoveNext();
            float value = float.Parse(scalar.Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            return new Vector3(value, value, value);
        }
        
        // Otherwise, parse as object with x, y, z properties
        parser.Consume<MappingStart>();
        
        float x = 1f, y = 1f, z = 1f;
        
        while (!parser.TryConsume<MappingEnd>(out _))
        {
            string key = parser.Consume<Scalar>().Value;
            string valueScalar = parser.Consume<Scalar>().Value;
            
            switch (key.ToLower())
            {
                case "x":
                    x = float.Parse(valueScalar.Replace(',', '.'), CultureInfo.InvariantCulture);
                    break;
                case "y":
                    y = float.Parse(valueScalar.Replace(',', '.'), CultureInfo.InvariantCulture);
                    break;
                case "z":
                    z = float.Parse(valueScalar.Replace(',', '.'), CultureInfo.InvariantCulture);
                    break;
            }
        }
        
        return new Vector3Ref(x, y, z);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is not Vector3Ref vec)
        {
            return;
        }

        if (Mathf.Approximately(vec.x, vec.y) && Mathf.Approximately(vec.y, vec.z))
        {
            emitter.Emit(new Scalar(
                AnchorName.Empty,
                TagName.Empty,
                vec.x.ToString(CultureInfo.InvariantCulture),
                ScalarStyle.Plain,
                isPlainImplicit: true,
                isQuotedImplicit: false
            ));
            return;
        }
        
        emitter.Emit(new MappingStart());
        
        emitter.Emit(new Scalar("x"));
        emitter.Emit(new Scalar(vec.x.ToString(CultureInfo.InvariantCulture)));
        
        emitter.Emit(new Scalar("y"));
        emitter.Emit(new Scalar(vec.y.ToString(CultureInfo.InvariantCulture)));
        
        emitter.Emit(new Scalar("z"));
        emitter.Emit(new Scalar(vec.z.ToString(CultureInfo.InvariantCulture)));
        
        emitter.Emit(new MappingEnd());
    }
}

public sealed class InvariantCultureFloatConverter : IYamlTypeConverter
{
    public static readonly InvariantCultureFloatConverter Instance = new();

    public bool Accepts(Type type) =>
        type == typeof(float) || type == typeof(double) || type == typeof(decimal);

    public object ReadYaml(IParser parser, Type type)
    {
        Scalar scalar = parser.Consume<Scalar>();
        return type switch
        {
            _ when type == typeof(float)   => float.Parse(scalar.Value, CultureInfo.InvariantCulture),
            _ when type == typeof(double)  => double.Parse(scalar.Value, CultureInfo.InvariantCulture),
            _ when type == typeof(decimal) => decimal.Parse(scalar.Value, CultureInfo.InvariantCulture),
            _ => throw new NotSupportedException()
        };
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        string str = value switch
        {
            float f   => f.ToString(CultureInfo.InvariantCulture),
            double d  => d.ToString(CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            _ => throw new NotSupportedException()
        };
        emitter.Emit(new Scalar(str));
    }
}