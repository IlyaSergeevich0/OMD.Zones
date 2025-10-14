using Microsoft.Extensions.DependencyInjection;
using OMD.Zones.Data;
using OMD.Zones.Events.Models;
using OMD.Zones.Main;
using OMD.Zones.Models.Utility;
using OMD.Zones.Models.Zones;
using OMD.Zones.Services.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.Core.Helpers;
using OpenMod.Extensions.Games.Abstractions.Entities;
using OpenMod.Unturned.Players;
using System;

namespace OMD.Zones.Services.Models;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
public sealed class ZonePresenceTracker(IEventBus eventBus) : IZonePresenceTracker, IDisposable
{
    private ZonesPlugin _plugin = null!;
    private ZonePresenceTrackingCore _zonePresenceTrackingCore = null!;

    private readonly IEventBus _eventBus = eventBus;

    void IZonePresenceTracker.Initialize(ZonesPlugin plugin, ZonesConfiguration zonesConfiguration)
    {
        _plugin = plugin;
        _zonePresenceTrackingCore = new ZonePresenceTrackingCore(zonesConfiguration);
    }

    void IZonePresenceTracker.Run()
    {
        _zonePresenceTrackingCore.GameObjectEnteredZone += OnGameObjectEnteredZone;
        _zonePresenceTrackingCore.GameObjectExitedZone += OnGameObjectExitedZone;

        _zonePresenceTrackingCore.Run();
    }

    public void Dispose()
    {
        _zonePresenceTrackingCore.GameObjectEnteredZone -= OnGameObjectEnteredZone;
        _zonePresenceTrackingCore.GameObjectExitedZone -= OnGameObjectExitedZone;

        _zonePresenceTrackingCore.Dispose();
    }

    void IZonePresenceTracker.AddZoneToGrid(Zone zone)
    {
        _zonePresenceTrackingCore.AddZoneToGrid(zone);

        zone.Initialize();
    }

    void IZonePresenceTracker.RemoveZoneFromGrid(Zone zone)
    {
        _zonePresenceTrackingCore.RemoveZoneFromGrid(zone);

        zone.Destroy();
    }

    void IZonePresenceTracker.UpdateZoneInGrid(Zone zone)
    {
        _zonePresenceTrackingCore.RemoveZoneFromGridRoughly(zone);
        _zonePresenceTrackingCore.AddZoneToGrid(zone);
    }

    public void StartTracking(IGameObject gameObject)
    {
        _zonePresenceTrackingCore.StartTracking(gameObject);
    }

    public void StopTracking(IGameObject gameObject)
    {
        _zonePresenceTrackingCore.StopTracking(gameObject);
    }

    private void OnGameObjectEnteredZone(IGameObject gameObject, Zone zone)
    {
        ZoneTriggeredEvent @event = gameObject switch {
            UnturnedPlayer unturnedPlayer => new UnturnedPlayerEnteredZoneEvent(zone, unturnedPlayer),
            _ => new GameObjectEnteredZoneEvent(zone, gameObject)
        };

        AsyncHelper.RunSync(() => _eventBus.EmitAsync(_plugin, this, @event));
    }

    private void OnGameObjectExitedZone(IGameObject gameObject, Zone zone)
    {
        ZoneTriggeredEvent @event = gameObject switch {
            UnturnedPlayer unturnedPlayer => new UnturnedPlayerExitedZoneEvent(zone, unturnedPlayer),
            _ => new GameObjectExitedZoneEvent(zone, gameObject)
        };

        AsyncHelper.RunSync(() => _eventBus.EmitAsync(_plugin, this, @event));
    }
}
