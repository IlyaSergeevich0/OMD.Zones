using OMD.Zones.Models.Zones;
using OpenMod.Core.Eventing;

namespace OMD.Zones.Events.Models;

public abstract class ZoneTriggeredEvent(Zone zone) : Event
{
    public readonly Zone Zone = zone;
}