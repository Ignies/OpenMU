// <copyright file="RuudShopBuyHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="RuudShopBuy"/> of the Babylon client: a purchase in the Ruud shop.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.RuudShopBuyHandlerPlugIn_Name), Description = nameof(PlugInResources.RuudShopBuyHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("6F3B9D24-8C1E-4A75-9D02-3E7B5A1C4F86")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class RuudShopBuyHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly BuyRuudShopItemAction _action = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => RuudShopBuy.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < RuudShopBuy.Length)
        {
            return;
        }

        RuudShopBuy message = packet;
        await this._action.BuyAsync(player, message.Index).ConfigureAwait(false);
    }
}
