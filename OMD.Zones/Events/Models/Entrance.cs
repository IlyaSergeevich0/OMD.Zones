using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
using OpenMod.Unturned.Vehicles;
using UnityEngine;

namespace OMD.Zones.Events;

public class UnturnedPlayerEnteredZoneEvent(Zone zone, UnturnedPlayer player) : UnturnedPlayerTriggeredZoneEvent(zone, player) { }

public class VehicleEnteredZoneEvent(Zone zone, UnturnedVehicle vehicle) : UnturnedVehicleTriggeredZoneEvent(zone, vehicle) { }

public class GameObjectEnteredZoneEvent(Zone zone, GameObject @object) : GameObjectTriggeredZoneEvent(zone, @object) { }