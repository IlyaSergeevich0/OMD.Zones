using System;

namespace OMD.Zones.Models.Zones;

public abstract class BoxZone : Zone
{
    public virtual SVector3 Size {
        get {
            return _size;
        }
        set {
            _size = value;

            InvokeOnUpdated();
        }
    }

    private SVector3 _size;

    public BoxZone(Guid id, SVector3 position, SVector3 size) :
        base(id, position)
    {
        _size = size;
    }

    public BoxZone() : base() { }

    public sealed override bool Contains(SVector3 point)
    {
        var halfSize = Size * 0.5f;

        return point.X >= Center.X - halfSize.X && point.X <= Center.X + halfSize.X &&
               point.Y >= Center.Y - halfSize.Y && point.Y <= Center.Y + halfSize.Y &&
               point.Z >= Center.Z - halfSize.Z && point.Z <= Center.Z + halfSize.Z;
    }
}