using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Vehicles;
using UnityEngine;

namespace OMD.Zones.Events;

public class UnturnedPlayerExitedZoneEvent(Zone zone, UnturnedPlayer player) : UnturnedPlayerTriggeredZoneEvent(zone, player) { }

public class VehicleExitedZoneEvent(Zone zone, UnturnedVehicle vehicle) : UnturnedVehicleTriggeredZoneEvent(zone, vehicle) { }

public class GameObjectExitedZoneEvent(Zone zone, GameObject @object) : GameObjectTriggeredZoneEvent(zone, @object) { }