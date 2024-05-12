using Microsoft.Extensions.DependencyInjection;
using OMD.Zones.Main;
using OMD.Zones.Services.API;
using OpenMod.API.Eventing;
using OpenMod.API.Plugins;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Level.Events;
using System.Threading.Tasks;

namespace OMD.Zones.Events.Listeners;

[EventListenerLifetime(ServiceLifetime.Transient)]
public sealed class LevelEventListeners(IZonesService zonesService, IPluginAccessor<ZonesPlugin> pluginAccessor) :
    IEventListener<UnturnedPostLevelLoadedEvent>
{
    [EventListener(IgnoreCancelled = true, Priority = EventListenerPriority.Highest)]
    public async Task HandleEventAsync(object? sender, UnturnedPostLevelLoadedEvent @event)
    {
        var plugin = pluginAccessor.Instance!;

        await zonesService.Initialize(plugin);
    }
}
