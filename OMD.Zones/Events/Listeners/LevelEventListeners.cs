using Microsoft.Extensions.DependencyInjection;
using OMD.Zones.Main;
using OMD.Zones.Services;
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
    public Task HandleEventAsync(object? sender, UnturnedPostLevelLoadedEvent @event)
    {
        var plugin = pluginAccessor.Instance!;

        zonesService.Initialize(plugin);

        return Task.CompletedTask;
    }
}
