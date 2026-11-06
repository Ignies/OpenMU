// <copyright file="GremoryCaseClaimHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="GremoryCaseClaim"/> of the Babylon client: a claim of a reward of a gremory case.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.GremoryCaseClaimHandlerPlugIn_Name), Description = nameof(PlugInResources.GremoryCaseClaimHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("4A8D2C61-9E3B-4F57-A0D4-6B1C3E8F2D95")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class GremoryCaseClaimHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => GremoryCaseClaim.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < GremoryCaseClaim.Length)
        {
            return;
        }

        GremoryCaseClaim message = packet;
        await GremoryCaseService.ClaimAsync(player, (GremoryCaseKind)message.Kind, message.Slot).ConfigureAwait(false);
    }
}
