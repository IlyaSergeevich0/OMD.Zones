using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OMD.Zones.API;
using OMD.Zones.Data;
using OMD.Zones.Main;
using OMD.Zones.Models.Zones;
using OMD.Zones.Services.API;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMD.Zones.Services.Models;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
public sealed class ZonesRegistry(
    IUnturnedUserDirectory unturnedUserDirectory,
    IZonePresenceTracker zonePresenceTracker,
    IPluginAccessor<ZonesPlugin> pluginAccessor,
    IEnumerable<IZonesProvider> providers) : IZonesRegistry
{
    public IEnumerable<IZonesProvider> Providers { get; } = providers;

    private readonly IUnturnedUserDirectory _unturnedUserDirectory = unturnedUserDirectory;
    private readonly IZonePresenceTracker _zonePresenceTracker = zonePresenceTracker;
    private readonly IPluginAccessor<ZonesPlugin> _pluginAccessor = pluginAccessor;
    private readonly ZonesConfiguration _configuration = new();

    async Task IZonesRegistry.InitializeAsync()
    {
        var plugin = _pluginAccessor.Instance
            ?? throw new InvalidOperationException($"Failed to find {nameof(ZonesPlugin)}!");

        plugin.Configuration.Bind(_configuration);

        foreach (var provider in Providers)
            await provider.InitializeAsync();

        _zonePresenceTracker.Initialize(plugin, _configuration);

        await foreach (var zone in GetAllZonesAsync())
            _zonePresenceTracker.AddZoneToGrid(zone);

        foreach (var unturnedUser in _unturnedUserDirectory.GetOnlineUsers())
        {
            var unturnedPlayer = unturnedUser.Player;

            _zonePresenceTracker.StartTracking(unturnedPlayer);
        }

        _zonePresenceTracker.Run();
    }

    public async Task RegisterZoneAsync<TZone>(TZone zone) where TZone : Zone
    {
        var provider = GetSupportingProviderOf<TZone>();

        await provider.AddAsync(zone);

        _zonePresenceTracker.AddZoneToGrid(zone);
    }

    public async Task UnregisterZoneAsync<TZone>(TZone zone) where TZone : Zone
    {
        var provider = GetSupportingProviderOf<TZone>();

        await provider.RemoveAsync(zone);

        _zonePresenceTracker.RemoveZoneFromGrid(zone);
    }

    public async Task UpdateZoneAsync<TZone>(TZone zone) where TZone : Zone
    {
        var provider = GetSupportingProviderOf<TZone>();

        await provider.UpdateAsync(zone);

        _zonePresenceTracker.UpdateZoneInGrid(zone);
    }

    public async Task RegisterZonesAsync<TZone>(IEnumerable<TZone> zones) where TZone : Zone
    {
        var provider = GetSupportingProviderOf<TZone>();

        await provider.AddRangeAsync(zones);

        foreach (var zone in zones)
            _zonePresenceTracker.AddZoneToGrid(zone);
    }

    public async Task UnregisterZonesAsync<TZone>(IEnumerable<TZone> zones) where TZone : Zone
    {
        var provider = GetSupportingProviderOf<TZone>();

        await provider.RemoveRangeAsync(zones);

        foreach (var zone in zones)
            _zonePresenceTracker.RemoveZoneFromGrid(zone);
    }

    public Task<TZone?> Find<TZone>(Guid id) where TZone : Zone
    {
        return Find<TZone>(z => z.Id == id);
    }

    public async Task<TZone?> Find<TZone>(Predicate<TZone> predicate) where TZone : Zone
    {
        await foreach (var zone in GetZonesOfTypeAsync<TZone>())
            if (predicate(zone))
                return zone;

        return null;
    }

    public async IAsyncEnumerable<TZone> GetZonesOfTypeAsync<TZone>() where TZone : Zone
    {
        var zoneType = typeof(TZone);

        foreach (var provider in Providers)
        {
            if (provider.Supports(zoneType))
            {
                await foreach (var zone in provider.GetZonesAsync())
                    yield return (TZone)zone;
            }
        }
    }

    public async IAsyncEnumerable<Zone> GetAllZonesAsync()
    {
        foreach (var provider in Providers)
        {
            var zones = provider.GetZonesAsync();

            await foreach (var zone in zones)
                yield return zone;
        }
    }

    private IZonesProvider GetSupportingProviderOf<TZone>()
    {
        var zoneType = typeof(TZone);

        return Providers.FirstOrDefault(p => p.Supports(zoneType))
            ?? throw new InvalidOperationException($"Failed to find any {nameof(IZonesProvider)}, which supports {zoneType.FullName}");
    }
}