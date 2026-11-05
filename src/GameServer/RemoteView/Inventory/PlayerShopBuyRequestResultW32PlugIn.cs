// <copyright file="PlayerShopBuyRequestResultW32PlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Inventory;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The player shop buy result for the Babylon client: the Season 6 packet has a fixed 12 byte item, so it gets the
/// extended packet, whose item takes the size of the <see cref="ItemSerializerW32"/>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.PlayerShopBuyRequestResultW32PlugIn_Name), Description = nameof(PlugInResources.PlayerShopBuyRequestResultW32PlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3E9B5C27-D8A1-4F60-9B2E-71C4A8D05F16")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class PlayerShopBuyRequestResultW32PlugIn : PlayerShopBuyRequestResultExtendedPlugIn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerShopBuyRequestResultW32PlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public PlayerShopBuyRequestResultW32PlugIn(RemotePlayer player)
        : base(player)
    {
    }
}
