using OMD.Zones.Models.Triggers;
using OpenMod.UnityEngine.Extensions;
using System;
using System.Numerics;

namespace OMD.Zones.Models.Zones;

public class CubicZone : Zone<CubicZoneTriggers>
{
    public override Vector3 Position {
        get {
            return Triggers == null ? base.Position : new Vector3(
                Triggers.TriggerCollider.center.x,
                Triggers.TriggerCollider.bounds.min.y,
                Triggers.TriggerCollider.center.z
            );
        }
        set {
            base.Position = value;

            if (Triggers != null)
            {
                Triggers.TriggerCollider.center = Position.ToUnityVector();
            }
        }
    }

    public virtual Vector3 Size {
        get {
            return _size;
        }
        set {
            _size = value;

            if (Triggers != null)
            {
                Triggers.TriggerCollider.size = _size.ToUnityVector();
            }
        }
    }

    [NonSerialized] private Vector3 _size;

    internal override void Initialize()
    {
        base.Initialize();

        Size = _size;
    }

    public override bool IsPointInside(Vector3 point) => throw new NotImplementedException();
}