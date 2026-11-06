// <copyright file="RankingRequestHandlerPlugIn.cs" company="MUnique">
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
/// Handles the <see cref="RankingRequest"/> of the Babylon client: a page of a ranking.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.RankingRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.RankingRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("7A1D4C92-B36E-4F05-9D81-2E6C8B4A0F17")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class RankingRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => RankingRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < RankingRequest.Length
            || HuntingRecordPlugIn.Get(player.GameContext) is not { } plugIn)
        {
            return;
        }

        RankingRequest message = packet;
        if (Enum.IsDefined((RankingKind)message.Kind))
        {
            await plugIn.ShowRankingAsync(player, (RankingKind)message.Kind, message.Page).ConfigureAwait(false);
        }
    }
}
