using Cysharp.Threading.Tasks;
using OMD.Zones.Models.Zones;
using OMD.Zones.Services;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace OMD.Zones.Commands;

[Command("zones")]
[CommandActor(typeof(UnturnedUser))]
public sealed class CommandZones(IZonesService zonesService, IUnturnedUserDirectory unturnedUserDirectory, IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    protected override async UniTask OnExecuteAsync()
    {
        var player = unturnedUserDirectory.FindUser(Context.Actor.Id, UserSearchMode.FindById)!.Player;

        await zonesService.Add(new SphericalZone {
            Name = Guid.NewGuid().ToString(),
            Position = player.Transform.Position,
            Radius = 3
        });

        await PrintAsync("Added!");
    }
}