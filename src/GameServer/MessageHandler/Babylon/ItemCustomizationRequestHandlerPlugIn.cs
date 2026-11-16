// <copyright file="ItemCustomizationRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles the item customisation request (EE 33): which items of the asked places are customised.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ItemCustomizationRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.ItemCustomizationRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5ADFD988-4D3F-47A6-B1F1-74B521B5B9CE")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class ItemCustomizationRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => ItemCustomizationRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < 5 || ItemCustomizationFeaturePlugIn.Get(player.GameContext) is not { } channel)
        {
            return;
        }

        ItemCustomizationRequest message = packet;
        var count = Math.Min((int)message.Count, ItemCustomizationFeaturePlugIn.MaximumAsks);
        if (packet.Length < ItemCustomizationRequest.GetRequiredSize(count))
        {
            return;
        }

        var asks = new List<(ItemCustomizationStorage, ushort)>(count);
        for (var i = 0; i < count; i++)
        {
            var ask = message[i];
            if (Enum.IsDefined((ItemCustomizationStorage)ask.Storage))
            {
                asks.Add(((ItemCustomizationStorage)ask.Storage, ask.ObjectId));
            }
        }

        await channel.HandleRequestAsync(player, asks).ConfigureAwait(false);
    }
}
