using System;
using System.Globalization;
using System.Numerics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OMD.Zones.Persistence.Converters;

public sealed class YamlVector3TypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(Vector3);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        if (parser.Current is null)
            throw new NullReferenceException("Current cannot be null!");

        var scalar = (Scalar)parser.Current;

        parser.MoveNext();

        var parts = scalar.Value.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
            throw new InvalidOperationException("Invalid Vector3 format");

        var x = float.Parse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
        var y = float.Parse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
        var z = float.Parse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

        return new Vector3(x, y, z);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is null)
            return;

        var vector3 = (Vector3)value;
        var xAsString = vector3.X.ToString("0.0000", CultureInfo.InvariantCulture);
        var yAsString = vector3.Y.ToString("0.0000", CultureInfo.InvariantCulture);
        var zAsString = vector3.Z.ToString("0.0000", CultureInfo.InvariantCulture);
        var vectorAsString = $"{xAsString};{yAsString};{zAsString}";
        var scalar = new Scalar(null, null, vectorAsString, ScalarStyle.Plain, true, false);

        emitter.Emit(scalar);
    }
}
