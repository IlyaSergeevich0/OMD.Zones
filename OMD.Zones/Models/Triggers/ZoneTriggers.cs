using OMD.Zones.Models.Zones;
using SDG.Unturned;
using System;
using UnityEngine;

namespace OMD.Zones.Models.Triggers;

public abstract class ZoneTriggers : MonoBehaviour
{
    internal static event Action<Zone, Collider>? OnTriggerEntered;

    internal static event Action<Zone, Collider>? OnTriggerExited;

    public Zone Zone { get; internal set; } = null!;

    public Collider TriggerCollider { get; protected set; } = null!;

    private void OnTriggerEnter(Collider other) => OnTriggerEntered?.Invoke(Zone, other);

    private void OnTriggerExit(Collider other) => OnTriggerExited?.Invoke(Zone, other);
}

public abstract class ZoneTriggers<TCollider> : ZoneTriggers where TCollider : Collider
{
    public new TCollider TriggerCollider { get; protected set; } = null!;

    private void Awake()
    {
        gameObject.layer = LayerMasks.CLIP;

        foreach (var collider in GetComponents<Collider>()) Destroy(collider);

        TriggerCollider = gameObject.AddComponent<TCollider>();
        TriggerCollider.isTrigger = true;
        TriggerCollider.material = null;
        TriggerCollider.sharedMaterial = null;
    }
}