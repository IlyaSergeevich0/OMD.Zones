using OMD.Zones.Models.Zones;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using UnityEngine;

namespace OMD.Zones.Events;

public class UnturnedPlayerEnteredZoneEvent(Zone zone, UnturnedPlayer player) : UnturnedPlayerTriggeredZoneEvent(zone, player) { }

public class VehicleEnteredZoneEvent(Zone zone, InteractableVehicle vehicle) : VehicleTriggeredZoneEvent(zone, vehicle) { }

public class GameObjectEnteredZoneEvent(Zone zone, GameObject @object) : GameObjectTriggeredZoneEvent(zone, @object) { }