// <copyright file="HelperPlusMoveRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.HelperPlus;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="HelperPlusMoveRequest"/> of the Babylon client: the move of the plus mode into a hunting zone.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.HelperPlusMoveRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.HelperPlusMoveRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("A58D2F16-3C9E-4B74-8A01-7F4E6B2D9C83")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class HelperPlusMoveRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => HelperPlusMoveRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < HelperPlusMoveRequest.Length)
        {
            return;
        }

        HelperPlusMoveRequest message = packet;
        var zone = (short)message.ZoneIndex;
        var result = HelperPlusPlugIn.Get(player.GameContext) is { } plugIn
            ? await plugIn.MoveAsync(player, zone).ConfigureAwait(false)
            : GameLogic.HelperPlus.HelperPlusMoveResult.Off;
        await player.InvokeViewPlugInAsync<IHelperPlusViewPlugIn>(p => p.ShowHelperPlusMoveResultAsync(zone, result)).ConfigureAwait(false);
    }
}
