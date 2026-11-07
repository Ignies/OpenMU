// <copyright file="AbilityCoreRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="AbilityCoreRequest"/> of the Babylon client: an action of the Ability Enhancement Core window.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.AbilityCoreRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.AbilityCoreRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("B2F74C19-8D3E-4A65-9C07-1E6B5A8D3F42")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class AbilityCoreRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => AbilityCoreRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < AbilityCoreRequest.Length)
        {
            return;
        }

        AbilityCoreRequest message = packet;
        await AbilityCoreService.RequestAsync(player, (AbilityCoreAction)message.Action, message.CoreSlot, message.InventorySlot).ConfigureAwait(false);
    }
}
