using System;
using System.Globalization;
using System.Numerics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace OMD.Zones.Persistence.Converters;

public sealed class YamlQuaternionTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(Quaternion);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        if (parser.Current is null)
            throw new NullReferenceException("Current cannot be null!");

        var scalar = (Scalar)parser.Current;

        parser.MoveNext();

        var parts = scalar.Value.Split(';');

        if (parts.Length != 4)
            throw new InvalidOperationException("Invalid Quaternion format");

        var x = float.Parse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
        var y = float.Parse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
        var z = float.Parse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
        var w = float.Parse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

        return new Quaternion(x, y, z, w);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is null)
            return;

        var quaternion = (Quaternion)value;
        var xAsString = quaternion.X.ToString("0.0000", CultureInfo.InvariantCulture);
        var yAsString = quaternion.Y.ToString("0.0000", CultureInfo.InvariantCulture);
        var zAsString = quaternion.Z.ToString("0.0000", CultureInfo.InvariantCulture);
        var wAsString = quaternion.W.ToString("0.0000", CultureInfo.InvariantCulture);
        var quaternionAsString = $"{xAsString};{yAsString};{zAsString};{wAsString}";

        var scalar = new Scalar(null, null, quaternionAsString, ScalarStyle.Plain, true, false);

        emitter.Emit(scalar);
    }
}
