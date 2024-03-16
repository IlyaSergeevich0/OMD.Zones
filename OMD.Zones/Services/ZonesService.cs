using Microsoft.Extensions.DependencyInjection;
using OMD.Zones.Main;
using OMD.Zones.Models.Zones;
using OMD.Zones.Persistence;
using OpenMod.API.Ioc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OMD.Zones.Services;

[ServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
public sealed class ZonesService : IZonesService, IAsyncDisposable
{
    public Guid GUID { get; } = Guid.NewGuid();

    private bool _isInitialized = false;

    private ZonesDataStore _dataStore = null!;

    public async Task Initialize(ZonesPlugin plugin)
    {
        if (_isInitialized)
            throw new InvalidOperationException("Service has already been initialized!");

        _dataStore = new ZonesDataStore(plugin.WorkingDirectory);

        Console.WriteLine($"After initialization: {_dataStore.Instance is null}");

        await _dataStore.Load();

        Console.WriteLine($"After load: {_dataStore.Instance is null}");

        _isInitialized = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isInitialized)
            return;

        await _dataStore.Save();
    }

    public Task<bool> Add<TZone>(TZone zone)
        where TZone : Zone
    {
        ThrowExceptionIfNotInitialized();

        return _dataStore.Add(zone);
    }

    public Task<bool> RemoveByName(string name)
    {
        ThrowExceptionIfNotInitialized();

        return _dataStore.RemoveByName(name);
    }

    public Task<bool> Remove<TZone>(TZone zone)
        where TZone : Zone
    {
        ThrowExceptionIfNotInitialized();

        return _dataStore.Remove(zone);
    }

    public Zone? Find(string name)
    {
        ThrowExceptionIfNotInitialized();

        return _dataStore.Zones.Where(z => z.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();
    }

    public TZone? Find<TZone>(string name) where TZone : Zone
    {
        ThrowExceptionIfNotInitialized();

        return _dataStore.Zones.Where(z => z is TZone && z.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault() as TZone;
    }

    public bool TryFind(string name, out Zone zone)
    {
        ThrowExceptionIfNotInitialized();

        zone = Find(name)!;

        return zone is not null;
    }

    public bool TryFind<TZone>(string name, out TZone zone) where TZone : Zone
    {
        ThrowExceptionIfNotInitialized();

        zone = Find<TZone>(name)!;

        return zone is not null;
    }

    private void ThrowExceptionIfNotInitialized()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Service is not initialized yet!");
    }
}