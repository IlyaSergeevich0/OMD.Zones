using OMD.Zones.Models.Zones;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMD.Zones.API;

[Service]
public interface IZonesProvider
{
    public bool Supports(Type zoneType);

    public Task InitializeAsync();

    public IAsyncEnumerable<Zone> GetZonesAsync();

    public Task AddAsync(Zone zone);
    public Task RemoveAsync(Zone zone);
    public Task UpdateAsync(Zone zone);
    public Task AddRangeAsync(IEnumerable<Zone> zones);
    public Task RemoveRangeAsync(IEnumerable<Zone> zones);
}
