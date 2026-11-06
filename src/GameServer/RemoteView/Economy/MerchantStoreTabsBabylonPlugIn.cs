// <copyright file="MerchantStoreTabsBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Economy;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the tabs of the store of a merchant to the Babylon client (C1 EE 74).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MerchantStoreTabsBabylonPlugIn_Name), Description = nameof(PlugInResources.MerchantStoreTabsBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("2E9C7B46-5F1A-4D38-B6E0-8A3D1F7C9B25")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class MerchantStoreTabsBabylonPlugIn : IMerchantStoreTabsPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="MerchantStoreTabsBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public MerchantStoreTabsBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask ShowMerchantStoreTabsAsync(byte tabCount, byte activeTab)
    {
        if (this._player.Connection is { } connection)
        {
            await connection.SendMerchantStoreTabsAsync(tabCount, activeTab).ConfigureAwait(false);
        }
    }
}
