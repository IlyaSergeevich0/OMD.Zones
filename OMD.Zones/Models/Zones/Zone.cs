using OMD.Zones.Models.Triggers;
using OpenMod.UnityEngine.Extensions;
using SDG.Unturned;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector3 = System.Numerics.Vector3;

namespace OMD.Zones.Models.Zones;

public abstract class Zone
{
    public static readonly Lazy<GameObject> Prefab = new(() => {
        const ushort PrefabItemId = 328;

        var asset = Assets.find(EAssetType.ITEM, PrefabItemId) as ItemBarricadeAsset;

        return asset?.barricade ?? throw new ArgumentNullException(nameof(asset));
    });

    public string Name { get; set; } = null!;

    public virtual Vector3 Position {
        get {
            if (Instance == null)
                return _position;

            return Instance.transform.position.ToSystemVector();
        }
        set {
            _position = value;

            if (Instance != null)
                Instance.transform.position = _position.ToUnityVector();
        }
    }

    protected ZoneTriggers Triggers { get; private set; } = null!;

    protected GameObject Instance = null!;

    private Vector3 _position;

    internal void Init()
    {
        if (Instance != null)
            return;

        Instance = Object.Instantiate(Prefab.Value);

        Object.DontDestroyOnLoad(Instance);

        foreach (var rigidBody in Instance.GetComponents<Rigidbody>())
            Object.Destroy(rigidBody);

        Instance.transform.position = _position.ToUnityVector();

        OnInitialized();
    }

    protected virtual void OnInitialized() { }

    internal void Destroy()
    {
        if (Triggers != null)
            Object.Destroy(Triggers);

        if (Instance != null)
            Object.Destroy(Instance);

        OnDestroyed();
    }

    protected virtual void OnDestroyed() { }

    public abstract bool IsPointInside(Vector3 point);
}

public abstract class Zone<TTriggers> : Zone
    where TTriggers : ZoneTriggers
{
    protected new TTriggers Triggers { get; private set; } = null!;

    protected override void OnInitialized()
    {
        Triggers = Instance.AddComponent<TTriggers>();
        Triggers.Zone = this;
    }

    public override bool IsPointInside(Vector3 point)
    {
        return Triggers.Collider.bounds.Contains(point.ToUnityVector());
    }
}