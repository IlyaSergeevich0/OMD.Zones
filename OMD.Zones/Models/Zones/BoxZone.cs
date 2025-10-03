using System;

namespace OMD.Zones.Models.Zones;

public abstract class BoxZone : Zone
{
    public virtual UVector3 Size {
        get {
            return _size;
        }
        set {
            _size = value;

            InvokeOnUpdated();
        }
    }

    private UVector3 _size;

    public BoxZone(Guid id, UVector3 position, UVector3 size) :
        base(id, position)
    {
        _size = size;
    }

    public BoxZone() : base() { }

    public sealed override bool Contains(UVector3 point)
    {
        var halfSize = Size * 0.5f;

        return point.x >= Center.x - halfSize.x && point.x <= Center.x + halfSize.x &&
               point.y >= Center.y - halfSize.y && point.y <= Center.y + halfSize.y &&
               point.z >= Center.z - halfSize.z && point.z <= Center.z + halfSize.z;
    }
}