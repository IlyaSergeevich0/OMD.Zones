using OMD.Zones.Models.Triggers;
using System.Numerics;

namespace OMD.Zones.Models.Zones;

public class SphericalZone : Zone<SphericalZoneTriggers>
{
    public virtual float Radius {
        get {
            return _radius;
        }
        set {
            _radius = value;

            if (!IsInitialized)
                return;

            TrySetColliderRadius();

            InvokeOnUpdated();
        }
    }

    private float _radius;

    public SphericalZone() : base() { }

    public SphericalZone(string name, Vector3 position, Quaternion rotation, float radius)
        : base(name, position, rotation)
    {
        _radius = radius;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        TrySetColliderRadius();
    }

    private void TrySetColliderRadius()
    {
        Triggers.Collider.radius = Radius;
    }
}
