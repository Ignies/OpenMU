// <copyright file="ArcaWarRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.ArcaWar;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handler for the <see cref="ArcaWarRequest"/> of the Babylon client: registers the guild or the member for the Arca War,
/// or enters it.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ArcaWarRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.ArcaWarRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5B0E7D93-2C4A-4F61-8E05-9D1A3B6C7F28")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class ArcaWarRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => ArcaWarRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < ArcaWarRequest.Length)
        {
            return;
        }

        ArcaWarRequest message = packet;
        if (message.Kind > (byte)ArcaWarRequestKind.Enter)
        {
            return;
        }

        if (ArcaWarPlugIn.Get(player.GameContext) is { } plugIn)
        {
            await plugIn.RequestAsync(player, (ArcaWarRequestKind)message.Kind).ConfigureAwait(false);
        }
        else
        {
            await player.InvokeViewPlugInAsync<IArcaWarViewPlugIn>(p => p.ShowRequestResultAsync((ArcaWarRequestKind)message.Kind, ArcaWarRequestResult.NotOpen)).ConfigureAwait(false);
        }
    }
}
