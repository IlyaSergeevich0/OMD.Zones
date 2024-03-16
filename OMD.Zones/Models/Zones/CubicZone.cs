using OMD.Zones.Models.Triggers;
using OpenMod.UnityEngine.Extensions;
using System.Numerics;

namespace OMD.Zones.Models.Zones;

public class CubicZone : Zone<CubicZoneTriggers>
{
    public override Vector3 Position {
        get {
            return Triggers == null ? base.Position : new Vector3(
                Triggers.Collider.center.x,
                Triggers.Collider.bounds.min.y,
                Triggers.Collider.center.z
            );
        }
        set {
            base.Position = value;

            if (Triggers != null)
                Triggers.Collider.center = Position.ToUnityVector();
        }
    }

    public virtual Vector3 Size {
        get {
            return _size;
        }
        set {
            _size = value;

            if (Triggers != null)
                Triggers.Collider.size = _size.ToUnityVector();
        }
    }

    private Vector3 _size;
}