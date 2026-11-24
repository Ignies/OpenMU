// <copyright file="MineCartRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="MineCartRequest"/> of the Babylon client: boarding or leaving the Kubera Mine cart.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MineCartRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.MineCartRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6D1C8E52-3A7F-4B09-9E24-B5F80C3A71D6")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class MineCartRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly MineCartAction _action = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => MineCartRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < MineCartRequest.Length)
        {
            return;
        }

        MineCartRequest message = packet;
        if (message.Action == 1)
        {
            await this._action.LeaveAsync(player).ConfigureAwait(false);
        }
        else
        {
            await this._action.BoardAsync(player).ConfigureAwait(false);
        }
    }
}
