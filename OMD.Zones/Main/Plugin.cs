using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

[assembly: PluginMetadata("OMD.Zones", DisplayName = "OMD.Zones", Author = "K1nd")]

namespace OMD.Zones.Main;

public sealed class ZonesPlugin(ILogger<ZonesPlugin> logger, IServiceProvider serviceProvider) : OpenModUnturnedPlugin(serviceProvider)
{
    protected override UniTask OnLoadAsync()
    {
        logger.LogInformation("Made with <3 by K1nd");
        logger.LogInformation("Discord: k1nd_");

        return UniTask.CompletedTask;
    }
}
