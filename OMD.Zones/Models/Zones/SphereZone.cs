using System;

namespace OMD.Zones.Models.Zones;

public abstract class SphereZone : Zone
{
    public virtual float Radius {
        get {
            return _radius;
        }
        set {
            _radius = value;

            InvokeOnUpdated();
        }
    }

    private float _radius;

    public SphereZone() : base() { }

    public SphereZone(Guid id, SVector3 position, float radius)
        : base(id, position)
    {
        if (radius <= 0)
            throw new ArgumentException($"{nameof(radius)} must be greater than 0");

        _radius = radius;
    }

    public sealed override bool Contains(SVector3 point)
    {
        var sqrRadius = _radius * _radius;
        var distance = Center - point;

        return distance.LengthSquared() <= sqrRadius;
    }
}
