using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;

namespace OMD.Zones.Events.Models;

public class UnturnedPlayerTriggeredZoneEvent(Zone zone, UnturnedPlayer unturnedPlayer) : ZoneTriggeredEvent(zone)
{
    public readonly UnturnedPlayer UnturnedPlayer = unturnedPlayer;
}

public class UnturnedPlayerEnteredZoneEvent(Zone zone, UnturnedPlayer unturnedPlayer) : UnturnedPlayerTriggeredZoneEvent(zone, unturnedPlayer);

public class UnturnedPlayerExitedZoneEvent(Zone zone, UnturnedPlayer unturnedPlayer) : UnturnedPlayerTriggeredZoneEvent(zone, unturnedPlayer);