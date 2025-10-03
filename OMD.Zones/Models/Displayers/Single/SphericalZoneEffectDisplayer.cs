using OMD.Zones.Models.Displayers.Base;
using OMD.Zones.Models.Zones;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using System;
using UnityEngine;

namespace OMD.Zones.Models.Displayers.Single;

public sealed class SphericalZoneEffectDisplayer(UnturnedPlayer targetPlayer, SphereZone zone, EffectAsset? effectAsset)
    : ZoneDisplayer<SphereZone>(targetPlayer, zone)
{
    public readonly EffectAsset EffectAsset = effectAsset ??
            throw new ArgumentNullException(nameof(effectAsset));

    public SphericalZoneEffectDisplayer(UnturnedPlayer targetPlayer, SphereZone zone, ushort effectId)
        : this(targetPlayer, zone, Assets.find(EAssetType.EFFECT, effectId) as EffectAsset) { }

    public SphericalZoneEffectDisplayer(UnturnedPlayer targetPlayer, SphereZone zone, Guid effectGuid)
        : this(targetPlayer, zone, Assets.find(effectGuid) as EffectAsset) { }

    protected override void Refresh()
    {
        const float ScaleMultiplier = 2;

        var transportConnection = TargetPlayer.Player.channel.owner.transportConnection;
        var triggerEffectParameters = new TriggerEffectParameters(EffectAsset) {
            position = TargetZone.Center,
            scale = Vector3.one * TargetZone.Radius * ScaleMultiplier
        };

        triggerEffectParameters.SetRelevantPlayer(transportConnection);

        EffectManager.ClearEffectByGuid(EffectAsset.GUID, transportConnection);
        EffectManager.triggerEffect(triggerEffectParameters);
    }
}
