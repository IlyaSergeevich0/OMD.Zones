using OMD.Zones.Models.Triggers;
using OpenMod.UnityEngine.Extensions;
using System.Numerics;

namespace OMD.Zones.Models.Zones;

public class CubicZone : Zone<CubicZoneTriggers>
{
    public virtual Vector3 Size {
        get {
            return _size;
        }
        set {
            _size = value;

            if (!IsInitialized)
                return;

            SetColliderSize();

            InvokeOnUpdated();
        }
    }

    private Vector3 _size;

    public CubicZone(string name, Vector3 position, Quaternion rotation, Vector3 size) :
        base(name, position, rotation)
    {
        _size = size;
    }

    public CubicZone() : base() { }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Triggers.Collider.center = new(0, 0, 0);

        SetColliderSize();
    }

    private void SetColliderSize()
    {
        Triggers.Collider.size = _size.ToUnityVector();
    }
}