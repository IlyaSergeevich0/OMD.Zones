using OMD.Zones.API;
using OMD.Zones.Models.Zones;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMD.Zones.Services.API;

[Service]
public interface IZonesRegistry
{
    public IEnumerable<IZonesProvider> Providers { get; }

    internal Task InitializeAsync();

    public Task RegisterZoneAsync<TZone>(TZone zone) where TZone : Zone;
    public Task UnregisterZoneAsync<TZone>(TZone zone) where TZone : Zone;
    public Task UpdateZoneAsync<TZone>(TZone zone) where TZone : Zone;
    public Task RegisterZonesAsync<TZone>(IEnumerable<TZone> zones) where TZone : Zone;
    public Task UnregisterZonesAsync<TZone>(IEnumerable<TZone> zones) where TZone : Zone;

    public Task<TZone?> Find<TZone>(Guid id) where TZone : Zone;
    public Task<TZone?> Find<TZone>(Predicate<TZone> predicate) where TZone : Zone;

    public IAsyncEnumerable<TZone> GetZonesOfTypeAsync<TZone>() where TZone : Zone;
    public IAsyncEnumerable<Zone> GetAllZonesAsync();
}
