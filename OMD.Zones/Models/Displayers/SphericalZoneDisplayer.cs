using OMD.Zones.Models.Zones;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OMD.Zones.Models.Displayers;

public sealed class SphericalZoneDisplayer(EffectAsset? effectAsset, SphericalZone zone) : ZoneDisplayer<SphericalZone>(zone)
{
    private readonly EffectAsset _effectAsset = effectAsset ??
            throw new ArgumentNullException(nameof(effectAsset));

    private Vector3[] _points = null!;

    public SphericalZoneDisplayer(ushort effectId, SphericalZone zone)
        : this(Assets.find(EAssetType.EFFECT, effectId) as EffectAsset, zone) { }

    public SphericalZoneDisplayer(Guid effectGuid, SphericalZone zone)
        : this(Assets.find(effectGuid) as EffectAsset, zone) { }

    public override void Refresh()
    {
        const int PointsAmount = 16;
        const float RadiansIn45Degrees = Mathf.PI / 4;
        const float RadiansInCircle = Mathf.PI * 2;

        var pointsSet = new HashSet<Vector3>(PointsAmount);

        // Adding horizontal points

        for (float angle = 0; angle < RadiansInCircle; angle += RadiansIn45Degrees)
        {
            var x = Zone.Radius * Mathf.Cos(angle);
            var z = Zone.Radius * Mathf.Sin(angle);

            pointsSet.Add(new Vector3(x, 0, z));
        }

        // Adding vertical points

        for (float angle = 0; angle < RadiansInCircle; angle += RadiansIn45Degrees)
        {
            var x = Zone.Radius * Mathf.Cos(angle);
            var y = Zone.Radius * Mathf.Sin(angle);

            pointsSet.Add(new Vector3(x, y, 0));
        }

        _points = [.. pointsSet];
    }

    public override void DisplayTo(Player nativePlayer)
    {
        var triggerEffectParameters = new TriggerEffectParameters(_effectAsset) {
            reliable = true,
            shouldReplicate = true
        };

        triggerEffectParameters.SetRelevantPlayer(nativePlayer.channel.owner.transportConnection);

        foreach (var point in _points)
        {
            triggerEffectParameters.position = point;

            EffectManager.triggerEffect(triggerEffectParameters);
        }
    }
}
