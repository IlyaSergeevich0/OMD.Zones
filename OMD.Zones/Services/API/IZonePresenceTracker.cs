using OMD.Zones.Data;
using OMD.Zones.Main;
using OMD.Zones.Models.Zones;
using OpenMod.API.Ioc;
using OpenMod.Extensions.Games.Abstractions.Entities;

namespace OMD.Zones.Services.API;

[Service]
public interface IZonePresenceTracker
{
    internal void Initialize(ZonesPlugin plugin, ZonesConfiguration zonesConfiguration);
    internal void Run();

    internal void AddZoneToGrid(Zone zone);
    internal void RemoveZoneFromGrid(Zone zone);
    internal void UpdateZoneInGrid(Zone zone);

    public void StartTracking(IGameObject gameObject);
    public void StopTracking(IGameObject gameObject);
}
