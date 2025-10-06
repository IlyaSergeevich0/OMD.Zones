using OMD.Zones.API;
using OMD.Zones.Main;
using OMD.Zones.Models.Zones;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace OMD.Zones.Services.API;

[Service]
public interface IZonesService
{
    public IEnumerable<IZonesProvider> Providers { get; }    

    internal Task Initialize();

    public TZone? Find<TZone>(Guid id) where TZone : Zone;
    public bool TryFind<TZone>(Guid id, [NotNullWhen(returnValue: true)] out TZone? zone) where TZone : Zone;
    public TZone? Find<TZone>(Predicate<TZone> predicate) where TZone : Zone;
    public bool TryFind<TZone>(Predicate<TZone> predicate, [NotNullWhen(returnValue: true)] out TZone? zone) where TZone : Zone;

    public IEnumerable<TZone> GetZonesOfType<TZone>() where TZone : Zone;
}
