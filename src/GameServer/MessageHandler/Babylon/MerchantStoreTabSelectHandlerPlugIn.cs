// <copyright file="MerchantStoreTabSelectHandlerPlugIn.cs" company="MUnique">
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
/// Handles the <see cref="MerchantStoreTabSelect"/> of the Babylon client: the selection of another tab of the store of the opened merchant.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MerchantStoreTabSelectHandlerPlugIn_Name), Description = nameof(PlugInResources.MerchantStoreTabSelectHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("D5A27E83-3F4B-4C19-8E6A-9B2D4F1C7A38")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class MerchantStoreTabSelectHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly SelectMerchantStoreTabAction _action = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => MerchantStoreTabSelect.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < MerchantStoreTabSelect.Length)
        {
            return;
        }

        MerchantStoreTabSelect message = packet;
        await this._action.SelectTabAsync(player, message.Tab).ConfigureAwait(false);
    }
}
