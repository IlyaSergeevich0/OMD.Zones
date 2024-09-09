using Autofac;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OMD.Events.Models;
using OMD.Events.Services;
using OMD.Zones.Events.Models;
using OMD.Zones.Models.Triggers;
using OMD.Zones.Models.Zones;
using OMD.Zones.Services.API;
using OMD.Zones.Services.Models;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Vehicles;
using SDG.Unturned;
using System;
using UnityEngine;
using Action = System.Action;

namespace OMD.Zones.Events.Handlers;

public sealed class ZoneEventsHandler : EventsHandler
{
    private enum ZoneTriggerType
    {
        Entered,
        Exited,
        None
    }

    private delegate void Teleporting(Player nativePlayer, Vector3 position);
    private static event Teleporting? OnTeleporting;

    private static event Action? OnZonesServiceInitialized;

    private readonly IZonesService _zonesService;

    public ZoneEventsHandler(IEventsService eventsService) : base(eventsService)
    {
        var serviceProvider = eventsService.OpenModHost.LifetimeScope.Resolve<IServiceProvider>();

        _zonesService = serviceProvider.GetRequiredService<IZonesService>();
    }

    public override void Subscribe()
    {
        ZoneTriggers.OnTriggerEntered += Events_OnTriggerEntered;
        ZoneTriggers.OnTriggerExited += Events_OnTriggerExited;

        OnTeleporting += Events_OnTeleporting;
        OnZonesServiceInitialized += Events_OnZonesServiceInitialized;
    }

    public override void Unsubscribe()
    {
        ZoneTriggers.OnTriggerEntered -= Events_OnTriggerEntered;
        ZoneTriggers.OnTriggerExited -= Events_OnTriggerExited;

        OnTeleporting -= Events_OnTeleporting;
        OnZonesServiceInitialized -= Events_OnZonesServiceInitialized;
    }

    // For some reason, when player is teleported, OnColliderExit is not being called
    // So this a workaround with teleport event handler

    private void Events_OnTeleporting(Player nativePlayer, Vector3 position)
    {
        Zone exitedZone = null!;

        var playerPosition = nativePlayer.transform.position.ToSystemVector();
        var newPosition = position.ToSystemVector();

        foreach (var zone in _zonesService.Zones)
        {
            /*if (zone.IsPointInside(newPosition))
                enteredZone = zone;*/

            if (zone.IsPointInside(playerPosition))
                exitedZone = zone;
        }

        var player = GetUnturnedPlayer(nativePlayer);

        if (exitedZone is not null)
        {
            var exitedEvent = new UnturnedPlayerExitedZoneEvent(exitedZone, player);

            EventsService.Logger.LogDebug("Player \"{PlayerName}\" ({PlayerSteamId}) has teleported from zone \"{Name}\"",
                player.SteamPlayer.playerID.characterName, player.SteamId, exitedZone.Name);

            Emit(exitedEvent);
        }
        /*
        if (enteredZone is not null)
        {
            var enteredEvent = new UnturnedPlayerEnteredZoneEvent(enteredZone, player);

            EventsService.Logger.LogDebug("Player \"{PlayerName}\" ({PlayerSteamId}) has teleported into zone \"{Name}\"",
                player.SteamPlayer.playerID.characterName, player.SteamId, enteredZone.Name);

            Emit(enteredEvent);
        }
        */
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

    private void Events_OnZonesServiceInitialized()
    {
        var @event = new ZonesServiceInitializedEvent();

        Emit(@event);
    }

    [HarmonyPatch(typeof(ZonesService))]
    private static class ZonesServicePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ZonesService.IsInitialized), MethodType.Setter)]
        private static void IsInitializedPostfix(bool value)
        {
            if (!value)
                return;

            OnZonesServiceInitialized?.Invoke();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), nameof(Player.ReceiveTeleport))]
        public static void ReceiveTeleportPrefix(Player __instance, Vector3 position)
        {
            OnTeleporting?.Invoke(__instance, position);
        }
    }
}