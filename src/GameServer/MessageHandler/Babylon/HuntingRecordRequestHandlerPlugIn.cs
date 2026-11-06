// <copyright file="HuntingRecordRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.HuntingRecord;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="HuntingRecordRequest"/> of the Babylon client: the hunting record of a map.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.HuntingRecordRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.HuntingRecordRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("C48B2E57-1F9A-4D36-A7C0-9B3E5D1F8A62")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class HuntingRecordRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <summary>
    /// The map number which asks for the current map.
    /// </summary>
    private const ushort CurrentMap = 0xFFFF;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => HuntingRecordRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < HuntingRecordRequest.Length
            || HuntingRecordPlugIn.Get(player.GameContext) is not { } plugIn)
        {
            return;
        }

        HuntingRecordRequest message = packet;
        await plugIn.ShowHuntingRecordAsync(player, message.MapNumber == CurrentMap ? null : (short)message.MapNumber).ConfigureAwait(false);
    }
}
