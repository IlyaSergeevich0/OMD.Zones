using OMD.Zones.Models.Zones;
using OpenMod.Extensions.Games.Abstractions.Entities;

namespace OMD.Zones.Events.Models;

public class GameObjectTriggeredZoneEvent(Zone zone, IGameObject gameObject) : ZoneTriggeredEvent(zone)
{
    public readonly IGameObject GameObject = gameObject;
}

public class GameObjectEnteredZoneEvent(Zone zone, IGameObject gameObject) : GameObjectTriggeredZoneEvent(zone, gameObject);

public class GameObjectExitedZoneEvent(Zone zone, IGameObject gameObject) : GameObjectTriggeredZoneEvent(zone, gameObject) { }