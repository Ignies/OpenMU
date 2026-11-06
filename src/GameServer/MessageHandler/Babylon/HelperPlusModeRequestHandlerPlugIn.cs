// <copyright file="HelperPlusModeRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.HelperPlus;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="HelperPlusModeRequest"/> of the Babylon client: the plus mode of the helper on or off.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.HelperPlusModeRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.HelperPlusModeRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("C17E4A92-5D3B-4E06-B9F8-2A6D8C1E4F75")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class HelperPlusModeRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => HelperPlusModeRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < HelperPlusModeRequest.Length || HelperPlusPlugIn.Get(player.GameContext) is not { } plugIn)
        {
            return;
        }

        HelperPlusModeRequest message = packet;
        await plugIn.SetPlusModeAsync(player, message.Enabled).ConfigureAwait(false);
    }
}
