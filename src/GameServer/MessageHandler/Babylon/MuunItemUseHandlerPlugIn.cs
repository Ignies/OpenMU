// <copyright file="MuunItemUseHandlerPlugIn.cs" company="MUnique">
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
/// Handles the <see cref="MuunItemUse"/> of the Babylon client: an item dropped on a Muun item: level up, evolution, energy, restore.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MuunItemUseHandlerPlugIn_Name), Description = nameof(PlugInResources.MuunItemUseHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3F9B7E24-1A6C-4E83-9D52-8B4F0C6A2E17")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class MuunItemUseHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => MuunItemUse.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < MuunItemUse.Length)
        {
            return;
        }

        MuunItemUse message = packet;
        await MuunService.UseItemAsync(player, (MuunAction)message.Action, (MuunStorageKind)message.SourceStorage, message.SourceSlot, (MuunStorageKind)message.TargetStorage, message.TargetSlot).ConfigureAwait(false);
    }
}
