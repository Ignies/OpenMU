// <copyright file="MuunRideRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Muuns;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the <see cref="MuunRideRequest"/> of the Babylon client: mounting or dismounting the Muun of the main slot.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.MuunRideRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.MuunRideRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("A84E2C17-5F3B-4D96-B1E0-3C7A9F2D6E51")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class MuunRideRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => MuunRideRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < MuunRideRequest.Length)
        {
            return;
        }

        MuunRideRequest message = packet;
        await MuunService.RideAsync(player, message.Ride).ConfigureAwait(false);
    }
}
