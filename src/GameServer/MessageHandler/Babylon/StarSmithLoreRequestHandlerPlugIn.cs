// <copyright file="StarSmithLoreRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the Star Smithing lore (C2 EE 95).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithLoreRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.StarSmithLoreRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("4A12F232-DC43-43D8-93B5-257ADA9B3DA2")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class StarSmithLoreRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => StarSmithLoreRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < StarSmithLoreRequest.Length || StarSmithingFeaturePlugIn.Get(player.GameContext) is not { } smithing)
        {
            return;
        }

        StarSmithLoreRequest message = packet;
        await smithing.HandleAsync(player, StarSmithAction.Lore, message.Slot, packet.Slice(6, 420)).ConfigureAwait(false);
    }
}
