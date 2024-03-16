using Microsoft.Extensions.Logging;
using OMD.Events.Models;
using OMD.Events.Services;
using OMD.Zones.Models.Triggers;
using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Vehicles;
using SDG.Unturned;
using UnityEngine;

namespace OMD.Zones.Events.Handlers;

public sealed class ZoneTriggersEventsHandler(IEventsService eventsService) : EventsHandler(eventsService)
{
    private enum ZoneTriggerType
    {
        Entered,
        Exited,
        None
    }

    public override void Subscribe()
    {
        ZoneTriggers.OnTriggerEntered += Events_OnTriggerEntered;
        ZoneTriggers.OnTriggerExited += Events_OnTriggerExited;
    }

    public override void Unsubscribe()
    {
        ZoneTriggers.OnTriggerEntered -= Events_OnTriggerEntered;
        ZoneTriggers.OnTriggerExited -= Events_OnTriggerExited;
    }

    // Seems like Animal and Zombie are not being detected by colliders
    // Don't know the reason for now, but I shall investigate it in future
    // So for now it is only Players, Vehicles and other GameObjects

    private void Events_OnTriggerEntered(Zone zone, Collider other) =>
        OnTriggered(zone, other, ZoneTriggerType.Entered);

    private void Events_OnTriggerExited(Zone zone, Collider other) =>
        OnTriggered(zone, other, ZoneTriggerType.Exited);

    private void OnTriggered(Zone zone, Collider other, ZoneTriggerType triggerType)
    {
        EventsService.Logger.LogDebug("Something \"{Name}\" | \"{Tag}\" {Type} zone \"{ZoneName}\"",
            other.name, other.tag, triggerType.ToString().ToLowerInvariant(), zone.Name);

        var nativePlayer = DamageTool.getPlayer(other.transform);

        if (nativePlayer is not null)
        {
            var player = GetUnturnedPlayer(nativePlayer);

            UnturnedPlayerTriggeredZoneEvent @event = triggerType == ZoneTriggerType.Entered ?
                new UnturnedPlayerEnteredZoneEvent(zone, player) :
                new UnturnedPlayerExitedZoneEvent(zone, player);

            Emit(@event);
            return;
        }

        if (other.TryGetComponent(out InteractableVehicle vehicle))
        {
            var unturnedVehicle = new UnturnedVehicle(vehicle);

            UnturnedVehicleTriggeredZoneEvent @event = triggerType == ZoneTriggerType.Entered ?
                new VehicleEnteredZoneEvent(zone, unturnedVehicle) :
                new VehicleExitedZoneEvent(zone, unturnedVehicle);

            Emit(@event);
            return;
        }

        GameObjectTriggeredZoneEvent gameObjectEvent = triggerType == ZoneTriggerType.Entered ?
            new GameObjectEnteredZoneEvent(zone, other.gameObject) :
            new GameObjectExitedZoneEvent(zone, other.gameObject);

        Emit(gameObjectEvent);
    }
}