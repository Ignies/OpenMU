// <copyright file="MuunItemMoveHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Muuns;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="MuunItemMove"/> of the Babylon client: a move of an item between the inventory and the Muun inventory.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MuunItemMoveHandlerPlugIn_Name), Description = nameof(PlugInResources.MuunItemMoveHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6D1A3F82-4C9E-4B57-8E20-1F7B5D3A9C64")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class MuunItemMoveHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => MuunItemMove.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < MuunItemMove.Length)
        {
            return;
        }

        MuunItemMove message = packet;
        await MuunService.MoveAsync(player, (MuunStorageKind)message.FromStorage, message.FromSlot, (MuunStorageKind)message.ToStorage, message.ToSlot).ConfigureAwait(false);
    }
}
