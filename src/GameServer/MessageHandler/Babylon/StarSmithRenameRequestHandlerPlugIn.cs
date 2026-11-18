// <copyright file="StarSmithRenameRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles the Star Smithing name (EE 94).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithRenameRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.StarSmithRenameRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("01A17999-C6F5-4877-A2AD-968D88B03A62")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class StarSmithRenameRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => StarSmithRenameRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < StarSmithRenameRequest.Length || StarSmithingFeaturePlugIn.Get(player.GameContext) is not { } smithing)
        {
            return;
        }

        StarSmithRenameRequest message = packet;
        await smithing.HandleAsync(player, StarSmithAction.Rename, message.Slot, packet.Slice(5, 72)).ConfigureAwait(false);
    }
}
