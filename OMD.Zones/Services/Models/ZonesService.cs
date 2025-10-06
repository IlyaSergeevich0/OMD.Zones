using Cysharp.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OMD.Zones.API;
using OMD.Zones.Main;
using OMD.Zones.Models.Zones;
using OMD.Zones.Persistence;
using OMD.Zones.Services.API;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OMD.Zones.Services.Models;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
public sealed class ZonesService(IEnumerable<IZonesProvider> providers) : IZonesService, IAsyncDisposable
{
    public IEnumerable<IZonesProvider> Providers { get; } = providers;

    /// <summary>
    /// Internal API.
    /// </summary>    
    public Task Initialize()
    {
        // TODO: Setup checking of zones

        throw new NotImplementedException();
    }

    public async ValueTask DisposeAsync()
    {
        // TODO: Stop checking of zones

        throw new NotImplementedException();
    }

    public TZone? Find<TZone>(Guid id) where TZone : Zone
    {
        return Find<TZone>(z => z.Id == id);
    }

    public bool TryFind<TZone>(Guid id, [NotNullWhen(returnValue: true)] out TZone? zone) where TZone : Zone
    {
        return TryFind(z => z.Id == id, out zone);
    }

    public TZone? Find<TZone>(Predicate<TZone> predicate) where TZone : Zone
    {
        return GetZonesOfType<TZone>().FirstOrDefault(z => predicate(z));
    }

    public bool TryFind<TZone>(Predicate<TZone> predicate, [NotNullWhen(returnValue: true)] out TZone? zone) where TZone : Zone
    {
        zone = GetZonesOfType<TZone>().FirstOrDefault(z => predicate(z));

        return zone is not null;
    }

    public IEnumerable<TZone> GetZonesOfType<TZone>() where TZone : Zone
    {
        var targetType = typeof(TZone);

        foreach(var provider in Providers)
        {
            if (provider.Supports(targetType))
            {
                foreach (var zone in provider.Zones.Cast<TZone>())
                    yield return zone;
            }
        }
    }       
}