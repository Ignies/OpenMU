// <copyright file="MuunExchangeRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles the <see cref="MuunExchangeRequest"/> of the Babylon client: an offer taken at the Muun exchange NPC.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MuunExchangeRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.MuunExchangeRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("C2E65A93-8D1F-4B70-A4C9-5E3B7F1D2A86")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class MuunExchangeRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => MuunExchangeRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < MuunExchangeRequest.Length)
        {
            return;
        }

        MuunExchangeRequest message = packet;
        await MuunService.ExchangeAsync(player, message.OfferIndex).ConfigureAwait(false);
    }
}
