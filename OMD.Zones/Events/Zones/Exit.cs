using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using UnityEngine;

namespace OMD.Zones.Events;

public class UnturnedPlayerExitedZoneEvent(Zone zone, UnturnedPlayer player) : UnturnedPlayerTriggeredZoneEvent(zone, player) { }

public class VehicleExitedZoneEvent(Zone zone, InteractableVehicle vehicle) : VehicleTriggeredZoneEvent(zone, vehicle) { }

public class GameObjectExitedZoneEvent(Zone zone, GameObject @object) : GameObjectTriggeredZoneEvent(zone, @object) { }