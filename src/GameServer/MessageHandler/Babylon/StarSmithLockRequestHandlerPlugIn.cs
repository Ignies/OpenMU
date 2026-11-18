// <copyright file="StarSmithLockRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles the Star Smithing lock (EE 96).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithLockRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.StarSmithLockRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("30DD1215-1531-436D-A390-A89372BD166D")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class StarSmithLockRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => StarSmithLockRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < StarSmithLockRequest.Length || StarSmithingFeaturePlugIn.Get(player.GameContext) is not { } smithing)
        {
            return;
        }

        StarSmithLockRequest message = packet;
        await smithing.HandleAsync(player, StarSmithAction.Lock, message.Slot, ReadOnlyMemory<byte>.Empty).ConfigureAwait(false);
    }
}
