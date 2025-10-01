using OMD.Zones.Models.Triggers;
using OpenMod.UnityEngine.Extensions;
using SDG.Unturned;
using System;
using UnityEngine;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace OMD.Zones.Models.Zones;

public abstract class Zone
{
    public static event Action<Zone>? OnUpdated;

    [YamlIgnore] public bool IsInitialized => Instance != null;

    public string Name { get; set; }

    public virtual Vector3 Position {
        get {
            return IsInitialized
                ? Instance.transform.position.ToSystemVector()
                : _position;
        }
        set {
            _position = value;

            if (!IsInitialized)
                return;

            Instance.transform.position = _position.ToUnityVector();

            InvokeOnUpdated();
        }
    }

    public virtual Quaternion Rotation {
        get {
            return IsInitialized
                ? Instance.transform.rotation.ToSystemQuaternion()
                : _rotation;
        }
        set {
            _rotation = value;

            if (!IsInitialized)
                return;

            Instance.transform.rotation = _rotation.ToUnityQuaternion();

            InvokeOnUpdated();
        }
    }

    [YamlIgnore] protected GameObject Instance = null!;

    [YamlIgnore] private Vector3 _position;

    [YamlIgnore] private Quaternion _rotation;

    public Zone()
    {
        Name = null!;
    }

    public Zone(string name, Vector3 position, Quaternion rotation)
    {
        Name = name;
        _position = position;
        _rotation = rotation;
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

    public abstract bool IsPointInside(Vector3 point);
}