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
    [NonSerialized]
    public static readonly Lazy<GameObject> Prefab = new(() => {
        const ushort PrefabItemId = 328;

        var asset = Assets.find(EAssetType.ITEM, PrefabItemId) as ItemBarricadeAsset;

        return asset?.barricade ?? throw new ArgumentNullException(nameof(asset));
    });

    public string Name { get; set; } = null!;

    public virtual Vector3 Position {
        get {
            return Instance?.transform.position.ToSystemVector() ?? _position;
        }
        set {
            _position = value;

            if (Instance != null)
                Instance.transform.position = _position.ToUnityVector();
        }
    }

    [field: NonSerialized] protected ZoneTriggers Triggers { get; private set; } = null!;

    [NonSerialized] protected GameObject Instance = null!;

    [NonSerialized] private Vector3 _position;

    internal virtual void Initialize()
    {
        if (Instance != null) return;

        Instance = Object.Instantiate(Prefab.Value);

        Object.DontDestroyOnLoad(Instance);

        foreach (var rigidBody in Instance.GetComponents<Rigidbody>()) Object.Destroy(rigidBody);

        Instance.transform.position = _position.ToUnityVector();
    }

    internal virtual void Destroy()
    {
        if (Triggers != null) Object.Destroy(Triggers);

        if (Instance != null) Object.Destroy(Instance);
    }

    public abstract bool IsPointInside(Vector3 point);
}

public abstract class Zone<TTriggers> : Zone where TTriggers : ZoneTriggers
{
    [field: NonSerialized] protected new TTriggers Triggers { get; private set; } = null!;

    internal override void Initialize()
    {
        base.Initialize();

        Triggers = Instance.AddComponent<TTriggers>();
        Triggers.Zone = this;
    }
}