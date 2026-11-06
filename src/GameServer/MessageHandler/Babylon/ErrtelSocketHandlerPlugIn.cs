// <copyright file="ErrtelSocketHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="ErrtelSocketRequest"/> of the Babylon client: an errtel into a socket of a pentagram, or out of it.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ErrtelSocketHandlerPlugIn_Name), Description = nameof(PlugInResources.ErrtelSocketHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("287AF708-09BC-48FC-BBBC-DA25E5F08E64")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class ErrtelSocketHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly ErrtelSocketAction _action = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => ErrtelSocketRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < ErrtelSocketRequest.Length)
        {
            return;
        }

        ErrtelSocketRequest message = packet;
        await this._action.HandleAsync(player, (ErrtelSocketMode)message.Mode, message.PentagramSlot, message.ErrtelSlot, message.SocketIndex).ConfigureAwait(false);
    }
}
