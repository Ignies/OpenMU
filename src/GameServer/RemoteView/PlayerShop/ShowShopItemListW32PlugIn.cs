// <copyright file="ShowShopItemListW32PlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.PlayerShop;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The shop item list for the Babylon client: the Season 6 packet has fixed 12 byte items, so it gets the extended
/// packet, whose items take the size of the <see cref="ItemSerializerW32"/>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ShowShopItemListW32PlugIn_Name), Description = nameof(PlugInResources.ShowShopItemListW32PlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("A7D3F190-2C4B-4E8A-B15F-9E6C0D2A4B83")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ShowShopItemListW32PlugIn : ShowShopItemListExtendedPlugIn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowShopItemListW32PlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ShowShopItemListW32PlugIn(RemotePlayer player)
        : base(player)
    {
    }
}
