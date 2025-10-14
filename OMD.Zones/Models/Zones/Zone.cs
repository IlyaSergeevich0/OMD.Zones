using System;

namespace OMD.Zones.Models.Zones;

public abstract class Zone : IEquatable<Zone?>
{
    public static event Action<Zone>? Initialized;
    public static event Action<Zone>? Destroyed;
    public static event Action<Zone>? Updated;

    public Guid Id { get; set; }

    public virtual SVector3 Center {
        get {
            return _center;
        }
        set {
            _center = value;

            InvokeOnUpdated();
        }
    }

    private SVector3 _center;

    public Zone()
    {
        Id = Guid.Empty;
    }

    public Zone(Guid id, SVector3 center)
    {
        Id = id;
        _center = center;
    }

    internal void Initialize()
    {
        OnInitialized();

        Initialized?.Invoke(this);
    }

    internal void Destroy()
    {
        OnDestroyed();

        Destroyed?.Invoke(this);
    }

    protected virtual void OnInitialized() { }

    protected virtual void OnDestroyed() { }

    protected void InvokeOnUpdated()
    {
        Updated?.Invoke(this);
    }

    public abstract bool Contains(SVector3 point);

    public override bool Equals(object? obj)
    {
        return Equals(obj as Zone);
    }

    public bool Equals(Zone? other)
    {
        return other is not null &&
               Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}