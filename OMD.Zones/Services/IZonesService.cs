using OMD.Zones.Main;
using OMD.Zones.Models.Zones;
using OpenMod.API.Ioc;
using System;
using System.Threading.Tasks;

namespace OMD.Zones.Services;

[Service]
public interface IZonesService
{
    public Guid GUID { get; }

    Task Initialize(ZonesPlugin plugin);

    Task<bool> Add<TZone>(TZone zone) where TZone : Zone;

    Task<bool> RemoveByName(string name);

    Task<bool> Remove<TZone>(TZone zone) where TZone : Zone;

    Zone? Find(string name);

    TZone? Find<TZone>(string name) where TZone : Zone;

    bool TryFind(string name, out Zone zone);

    bool TryFind<TZone>(string name, out TZone zone) where TZone : Zone;
}
