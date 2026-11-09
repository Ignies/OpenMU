// <copyright file="GuideQuestRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.GuideQuests;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the guide quest requests of the Babylon client (C1 EE C1): show, accept and complete the step.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.GuideQuestRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.GuideQuestRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E61D3B48-9A72-4C05-8B3F-2D7E5A9C1F06")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class GuideQuestRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private const byte Accept = 1;

    private const byte Complete = 2;

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => GuideQuestRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < GuideQuestRequest.Length
            || GuideQuestPlugIn.Get(player.GameContext) is not { } plugIn)
        {
            return;
        }

        GuideQuestRequest message = packet;
        var number = (short)message.StepNumber;
        switch (message.Action)
        {
            case Accept:
                await plugIn.AcceptAsync(player, number).ConfigureAwait(false);
                break;
            case Complete:
                await plugIn.CompleteAsync(player, number).ConfigureAwait(false);
                break;
            default:
                await plugIn.ShowAsync(player).ConfigureAwait(false);
                break;
        }
    }
}
