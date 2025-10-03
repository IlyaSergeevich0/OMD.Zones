using System;
using YamlDotNet.Serialization;

namespace OMD.Zones.Models.Zones;

public abstract class Zone
{
    public static event Action<Zone>? OnUpdated;

    public Guid Id { get; set; }

    public virtual UVector3 Center {
        get {
            return _center;
        }
        set {
            _center = value;

            InvokeOnUpdated();
        }
    }

    [YamlIgnore] private UVector3 _center;

    public Zone()
    {
        Id = Guid.Empty;
    }

    public Zone(Guid id, UVector3 center)
    {
        Id = id;
        _center = center;
    }

    internal void Initialize()
    {
        OnInitialized();

        InvokeOnUpdated();
    }

    internal void Destroy()
    {
        OnDestroyed();

        InvokeOnUpdated();
    }

    protected virtual void OnInitialized() { }

    protected virtual void OnDestroyed() { }

    protected void InvokeOnUpdated()
    {
        OnUpdated?.Invoke(this);
    }

    public abstract bool Contains(UVector3 point);
}