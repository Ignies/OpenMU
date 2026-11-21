// <copyright file="StarSmithRecolorRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles the Star Smithing recolour (EE 93): the item's new colours.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithRecolorRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.StarSmithRecolorRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("28AE1C21-3ECB-4F3E-9C58-2EE69B86430D")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class StarSmithRecolorRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => StarSmithRecolorRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < StarSmithRecolorRequest.Length || StarSmithingFeaturePlugIn.Get(player.GameContext) is not { } smithing)
        {
            return;
        }

        StarSmithRecolorRequest message = packet;
        await smithing.HandleAsync(player, StarSmithAction.Recolor, message.Slot, packet.Slice(5, 31), message.DyeSlot, message.RadiantSlot).ConfigureAwait(false);
    }
}
