using OMD.Zones.Models.Zones;
using System;
using System.Collections.Generic;

namespace OMD.Zones.Data;

[Serializable]
public sealed class ZonesStore
{
    public List<Zone> Zones { get; set; } = [];
}
