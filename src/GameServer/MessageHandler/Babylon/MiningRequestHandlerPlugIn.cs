// <copyright file="MiningRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Mining;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="MiningRequest"/> of the Babylon client: mining the next stage of a spot, or collecting.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MiningRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.MiningRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("2B7E4D93-C51A-4F86-8D3E-9A0F6C2B5E71")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class MiningRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => MiningRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < MiningRequest.Length
            || MiningFeaturePlugIn.Get(player.GameContext) is not { } mining)
        {
            return;
        }

        MiningRequest message = packet;
        if (player.CurrentMap?.GetObject(message.NpcId) is not NonPlayerCharacter spot)
        {
            return;
        }

        if (message.Action == 1)
        {
            await mining.CollectAsync(player, spot).ConfigureAwait(false);
        }
        else
        {
            await mining.MineAsync(player, spot).ConfigureAwait(false);
        }
    }
}
