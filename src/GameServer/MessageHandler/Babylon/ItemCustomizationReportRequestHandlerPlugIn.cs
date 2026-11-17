// <copyright file="ItemCustomizationReportRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles the item customisation report (EE 37): a player reports the name or lore of one item.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ItemCustomizationReportRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.ItemCustomizationReportRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("AD8AB987-AE58-486B-B257-CE6794526E46")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class ItemCustomizationReportRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => ItemCustomizationReportRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < ItemCustomizationReportRequest.Length
            || ItemCustomizationFeaturePlugIn.Get(player.GameContext) is not { } channel)
        {
            return;
        }

        ItemCustomizationReportRequest message = packet;
        if (Enum.IsDefined((ItemCustomizationStorage)message.Storage))
        {
            await channel.HandleReportAsync(player, (ItemCustomizationStorage)message.Storage, message.ObjectId, message.Slot).ConfigureAwait(false);
        }
    }
}
