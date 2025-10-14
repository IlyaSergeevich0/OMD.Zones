using Microsoft.Extensions.DependencyInjection;
using OMD.Zones.Services.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players.Connections.Events;
using System.Threading.Tasks;

namespace OMD.Zones.Events.Listeners;

[EventListenerLifetime(ServiceLifetime.Transient)]
public sealed class ConnectionEventListeners(IZonePresenceTracker zonePresenceTracker) :
    IEventListener<UnturnedPlayerConnectedEvent>,
    IEventListener<UnturnedPlayerDisconnectedEvent>
{
    public Task HandleEventAsync(object? sender, UnturnedPlayerConnectedEvent @event)
    {
        zonePresenceTracker.StartTracking(@event.Player);

        return Task.CompletedTask;
    }

    public Task HandleEventAsync(object? sender, UnturnedPlayerDisconnectedEvent @event)
    {
        zonePresenceTracker.StopTracking(@event.Player);

        return Task.CompletedTask;
    }
}
