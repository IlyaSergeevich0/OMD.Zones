using Cysharp.Threading.Tasks;
using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
using System;

namespace OMD.Zones.Models.Displayers.Base;

public abstract class ZoneDisplayer(UnturnedPlayer targetPlayer) : IDisposable
{
    public readonly UnturnedPlayer TargetPlayer = targetPlayer;

    public void Dispose()
    {
        UniTask.Create(DisposeAsync).Forget();
    }

    protected abstract UniTask DisposeAsync();
}

public abstract class ZoneDisplayer<TZone> : ZoneDisplayer
    where TZone : Zone
{
    public readonly TZone TargetZone;

    public ZoneDisplayer(UnturnedPlayer targetPlayer, TZone zone) : base(targetPlayer)
    {
        TargetZone = zone;

        Refresh();

        Zone.Initialized += OnZoneStateChanged;
        Zone.Destroyed += OnZoneStateChanged;
        Zone.Updated += OnZoneStateChanged;
    }

    protected override UniTask DisposeAsync()
    {
        Zone.Initialized -= OnZoneStateChanged;
        Zone.Destroyed -= OnZoneStateChanged;
        Zone.Updated -= OnZoneStateChanged;

        return UniTask.CompletedTask;
    }

    private void OnZoneStateChanged(Zone zone)
    {
        if (zone.Id == TargetZone.Id)
            Refresh();
    }

    public void Refresh()
    {
        UniTask.Create(RefreshAsync).Forget();
    }

    protected abstract UniTask RefreshAsync();
}