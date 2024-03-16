using OMD.Zones.Models.Triggers;

namespace OMD.Zones.Models.Zones;

public class SphericalZone : Zone<SphericalZoneTriggers>
{
    public virtual float Radius {
        get {
            return _radius;
        }
        set {
            _radius = value;

            if (Triggers != null)
                Triggers.Collider.radius = _radius;
        }
    }

    private float _radius;
}
