using OMD.Zones.Models.Displayers.Base;
using OMD.Zones.Models.Zones;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OMD.Zones.Models.Displayers.Multiple;

public sealed class SphericalZonesEffectDisplayer(UnturnedPlayer targetPlayer, IEnumerable<SphericalZone> zones, EffectAsset? effectAsset)
    : ZonesDisplayer<SphericalZone>(targetPlayer, zones)
{
    public readonly EffectAsset EffectAsset = effectAsset ??
            throw new ArgumentNullException(nameof(effectAsset));

    public SphericalZonesEffectDisplayer(UnturnedPlayer targetPlayer, IEnumerable<SphericalZone> zones, ushort effectId)
        : this(targetPlayer, zones, Assets.find(EAssetType.EFFECT, effectId) as EffectAsset) { }

    public SphericalZonesEffectDisplayer(UnturnedPlayer targetPlayer, IEnumerable<SphericalZone> zones, Guid effectGuid)
        : this(targetPlayer, zones, Assets.find(effectGuid) as EffectAsset) { }

    public override void Dispose()
    {
        base.Dispose();

        var transportConnection = TargetPlayer.Player.channel.owner.transportConnection;

        EffectManager.ClearEffectByGuid(EffectAsset.GUID, transportConnection);
    }

    protected override void Refresh()
    {
        const float ScaleMultiplier = 2;

        var transportConnection = TargetPlayer.Player.channel.owner.transportConnection;
        var triggerEffectParameters = new TriggerEffectParameters(EffectAsset);

        triggerEffectParameters.SetRelevantPlayer(transportConnection);

        EffectManager.ClearEffectByGuid(EffectAsset.GUID, transportConnection);

        foreach (var zone in TargetZones)
        {
            triggerEffectParameters.position = zone.Position.ToUnityVector();
            triggerEffectParameters.scale = Vector3.one * zone.Radius * ScaleMultiplier;

            EffectManager.triggerEffect(triggerEffectParameters);
        }
    }
}
