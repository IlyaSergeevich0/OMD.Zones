using Cysharp.Threading.Tasks;
using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
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

        Zone.Initialized += OnZoneInitializedOrDestroyed;
        Zone.Destroyed += OnZoneInitializedOrDestroyed;
        Zone.Updated += OnZoneUpdated;
    }

    protected override UniTask DisposeAsync()
    {
        Zone.Initialized -= OnZoneInitializedOrDestroyed;
        Zone.Destroyed -= OnZoneInitializedOrDestroyed;
        Zone.Updated -= OnZoneUpdated;

        return UniTask.CompletedTask;
    }

    private void OnZoneUpdated(Zone zone)
    {
        if (TargetZones.Any(z => z.Id == zone.Id))
            Refresh();
    }

    private void OnZoneInitializedOrDestroyed(Zone zone)
    {
        // Zone might be deleted from/not yet added to target zones (e.g. reference to a list of cached zones)
        // So we forcing displayer to refresh

        Refresh();
    }

    public void Refresh()
    {
        UniTask.Create(RefreshAsync).Forget();
    }

    protected abstract UniTask RefreshAsync();
}
