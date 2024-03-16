using OMD.Zones.Models.Triggers;
using System;
using System.Numerics;

namespace OMD.Zones.Models.Zones
{
    public class SphericalZone : Zone<SphericalZoneTriggers>
    {
        public virtual float Radius {
            get {
                return _radius;
            }
            set {
                _radius = value;

                if (Triggers != null) Triggers.TriggerCollider.radius = _radius;
            }
        }

        [NonSerialized] private float _radius;

        internal override void Initialize()
        {
            base.Initialize();

            Radius = _radius;
        }

        public override bool IsPointInside(Vector3 point)
        {
            return (point - Position).LengthSquared() < Radius * Radius;
        }
    }
}
