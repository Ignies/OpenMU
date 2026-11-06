// <copyright file="PentagramErrtelsRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="PentagramErrtelsRequest"/> of the Babylon client: it answers with the errtels of the pentagram.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.PentagramErrtelsRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.PentagramErrtelsRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("96C95231-9351-458A-AAB9-9BF187DBFF75")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class PentagramErrtelsRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => PentagramErrtelsRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < PentagramErrtelsRequest.Length)
        {
            return;
        }

        PentagramErrtelsRequest message = packet;
        if (player.Inventory?.GetItem(message.PentagramSlot) is { } pentagram && pentagram.IsPentagram())
        {
            await player.InvokeViewPlugInAsync<IPentagramViewPlugIn>(p => p.ShowPentagramErrtelsAsync(pentagram)).ConfigureAwait(false);
        }
    }
}
