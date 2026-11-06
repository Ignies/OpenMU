// <copyright file="ItemExpiredBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Economy;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.ItemExpiry;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tells the Babylon client that an item expired (C1 EE 31).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ItemExpiredBabylonPlugIn_Name), Description = nameof(PlugInResources.ItemExpiredBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("0B6E3A92-4D1F-4C87-A5E8-2F9D7C1B3E64")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ItemExpiredBabylonPlugIn : IItemExpiredPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemExpiredBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public ItemExpiredBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask ItemExpiredAsync(ExpiredItemStorage storage, byte slot, Item item)
    {
        if (this._player.Connection is { } connection && item.Definition is { } definition)
        {
            await connection.SendItemExpiredAsync((byte)storage, slot, definition.Group, (ushort)definition.Number).ConfigureAwait(false);
        }
    }
}
