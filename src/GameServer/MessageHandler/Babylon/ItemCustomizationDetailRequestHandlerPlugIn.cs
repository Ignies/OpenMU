// <copyright file="ItemCustomizationDetailRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the item customisation detail request (EE 35): the lore of one item.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ItemCustomizationDetailRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.ItemCustomizationDetailRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("22382C55-4B05-4D0C-812E-4FAF0D17DAF8")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class ItemCustomizationDetailRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => ItemCustomizationDetailRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < ItemCustomizationDetailRequest.Length
            || ItemCustomizationFeaturePlugIn.Get(player.GameContext) is not { } channel)
        {
            return;
        }

        ItemCustomizationDetailRequest message = packet;
        if (Enum.IsDefined((ItemCustomizationStorage)message.Storage))
        {
            await channel.HandleDetailAsync(player, (ItemCustomizationStorage)message.Storage, message.ObjectId, message.Slot).ConfigureAwait(false);
        }
    }
}
