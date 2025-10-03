using OMD.Zones.Models.Zones;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMD.Zones.API;

[Service]
public interface IZonesProvider
{
    public IReadOnlyList<Zone> Zones { get; }

    public bool Supports(Type zoneType);
}
