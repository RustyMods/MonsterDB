using System;
using System.Globalization;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public struct Vector3Ref
{
    public float x;
    public float y;
    public float z;

    public Vector3Ref(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Ref(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3() => new(x, y, z);

    public override string ToString()
        => $"{x}, {y}, {z}";

    public static Vector3Ref Parse(string value)
    {
        var parts = value.Split(',');
        if (parts.Length != 3)
            throw new FormatException("Invalid Vector3 format");

        return new Vector3Ref(
            float.Parse(parts[0], CultureInfo.InvariantCulture),
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture)
        );
    }

    public static implicit operator Vector3(Vector3Ref v) => v.ToVector3();
    public static implicit operator Vector3Ref(Vector3 v) => new(v);
}