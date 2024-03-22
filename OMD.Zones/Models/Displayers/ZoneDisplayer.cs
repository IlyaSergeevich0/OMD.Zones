using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
using SDG.Unturned;

namespace OMD.Zones.Models.Displayers;

public abstract class ZoneDisplayer
{
    public abstract void Refresh();

    public void DisplayTo(UnturnedPlayer player) => DisplayTo(player.Player);

    public abstract void DisplayTo(Player nativePlayer);
}

public abstract class ZoneDisplayer<TZone> : ZoneDisplayer
    where TZone : Zone
{
    public readonly TZone Zone;

    public ZoneDisplayer(TZone zone)
    {
        Zone = zone;

        Refresh();
    }
}