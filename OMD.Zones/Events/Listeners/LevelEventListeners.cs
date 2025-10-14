using Microsoft.Extensions.DependencyInjection;
using OMD.Zones.Services.API;
using OpenMod.API.Eventing;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Level.Events;
using System.Threading.Tasks;

namespace OMD.Zones.Events.Listeners;

[EventListenerLifetime(ServiceLifetime.Transient)]
public sealed class LevelEventListeners(IZonesRegistry zonesRegistry) :
    IEventListener<UnturnedPostLevelLoadedEvent>
{
    [EventListener(IgnoreCancelled = true, Priority = EventListenerPriority.Monitor)]
    public async Task HandleEventAsync(object? sender, UnturnedPostLevelLoadedEvent @event)
    {
        await zonesRegistry.InitializeAsync();
    }
}
