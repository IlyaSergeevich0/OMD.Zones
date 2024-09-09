using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OMD.Zones.Models.Displayers.Base;

public abstract class ZonesDisplayer<TZone> : ZoneDisplayer
    where TZone : Zone
{
    public readonly IEnumerable<TZone> TargetZones;

    public ZonesDisplayer(UnturnedPlayer targetPlayer, IEnumerable<TZone> zones) : base(targetPlayer)
    {
        TargetZones = zones;

        Refresh();

        Zone.OnUpdated += OnZoneUpdated;
    }

    public override void Dispose()
    {
        Zone.OnUpdated -= OnZoneUpdated;
    }

    private void OnZoneUpdated(Zone zone)
    {
        if (TargetZones.Any(z => z.Name.Equals(zone.Name, StringComparison.Ordinal)))
            Refresh();
    }

    protected abstract void Refresh();
}
