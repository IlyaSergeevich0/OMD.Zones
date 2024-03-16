using OMD.Zones.Models.Zones;
using OpenMod.Core.Eventing;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using GameObject = UnityEngine.GameObject;

namespace OMD.Zones.Events;

public abstract class ZoneTriggeredEvent(Zone zone) : Event
{
    public readonly Zone Zone = zone;
}

public class UnturnedPlayerTriggeredZoneEvent(Zone zone, UnturnedPlayer player) : ZoneTriggeredEvent(zone)
{
    public readonly UnturnedPlayer Player = player;
}

public class VehicleTriggeredZoneEvent(Zone zone, InteractableVehicle vehicle) : ZoneTriggeredEvent(zone)
{
    public readonly InteractableVehicle Vehicle = vehicle;
}

public class GameObjectTriggeredZoneEvent(Zone zone, GameObject @object) : ZoneTriggeredEvent(zone)
{
    public readonly GameObject Object = @object;
}