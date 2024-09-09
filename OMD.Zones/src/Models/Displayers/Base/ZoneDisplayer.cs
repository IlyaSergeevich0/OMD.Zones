using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
using System;

namespace OMD.Zones.Models.Displayers.Base;

public abstract class ZoneDisplayer(UnturnedPlayer targetPlayer) : IDisposable
{
    public readonly UnturnedPlayer TargetPlayer = targetPlayer;

    public abstract void Dispose();
}

public abstract class ZoneDisplayer<TZone> : ZoneDisplayer
    where TZone : Zone
{
    public readonly TZone TargetZone;

    public ZoneDisplayer(UnturnedPlayer targetPlayer, TZone zone) : base(targetPlayer)
    {
        TargetZone = zone;

        Refresh();

        Zone.OnUpdated += OnZoneUpdated;
    }

    public override void Dispose()
    {
        Zone.OnUpdated -= OnZoneUpdated;
    }

    private void OnZoneUpdated(Zone zone)
    {
        if (zone.Name.Equals(TargetZone.Name, StringComparison.Ordinal))
            Refresh();
    }

    protected abstract void Refresh();
}