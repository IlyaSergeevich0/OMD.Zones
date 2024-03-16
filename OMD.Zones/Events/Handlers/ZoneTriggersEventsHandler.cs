using Microsoft.Extensions.Logging;
using OMD.Events.Models;
using OMD.Events.Services;
using OMD.Zones.Models.Triggers;
using OMD.Zones.Models.Zones;
using UnityEngine;

namespace OMD.Zones.Events.Handlers;

public sealed class ZoneTriggersEventsHandler(IEventsService eventsService) : EventsHandler(eventsService)
{
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

    private void Events_OnTriggerEntered(Zone zone, Collider other)
    {
        EventsService.Logger.LogDebug("Something \"{Name}\" | \"{Tag}\" entered zone \"{ZoneName}\"",
            other.name, other.tag, zone.Name);
    }

    private void Events_OnTriggerExited(Zone zone, Collider other)
    {
        EventsService.Logger.LogDebug("Something \"{Name}\" | \"{Tag}\" exited zone \"{ZoneName}\"",
            other.name, other.tag, zone.Name);
    }
}

/*
 internal static void UpdateStateFor(this Zone zone, Collider other, StateRelativeToZone state)
        {
            if (other.TryGetComponent(out Player player))
                InvokeEventSafe(zone, player.ToUnturnedPlayer(), state, OnPlayerEntered, OnPlayerExited);
            else if (other.TryGetComponent(out Zombie zombie))
                InvokeEventSafe(zone, zombie, state, OnZombieEntered, OnZombieExited);
            else if (other.TryGetComponent(out Animal animal))
                InvokeEventSafe(zone, animal, state, OnAnimalEntered, OnAnimalExited);
            else
                InvokeEventSafe(zone, other.gameObject, state, OnGameObjectEntered, OnGameObjectExited);
        }
*/