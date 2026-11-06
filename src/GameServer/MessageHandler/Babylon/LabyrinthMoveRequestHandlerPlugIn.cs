// <copyright file="LabyrinthMoveRequestHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.MessageHandler.Babylon;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handler for the <see cref="LabyrinthRoomMoveRequest"/> of the Babylon client: moves the character through a portal of its
/// room in the Labyrinth of Dimensions.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.LabyrinthMoveRequestHandlerPlugIn_Name), Description = nameof(PlugInResources.LabyrinthMoveRequestHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("90D3CAEA-BD6A-4B8A-B337-F80768F34ED0")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
[BelongsToGroup(BabylonGroupHandlerPlugIn.GroupKey)]
internal class LabyrinthMoveRequestHandlerPlugIn : ISubPacketHandlerPlugIn
{
    private readonly LabyrinthMoveAction _moveAction = new();

    /// <inheritdoc/>
    public bool IsEncryptionExpected => false;

    /// <inheritdoc/>
    public byte Key => LabyrinthRoomMoveRequest.SubCode;

    /// <inheritdoc/>
    public async ValueTask HandlePacketAsync(Player player, Memory<byte> packet)
    {
        if (packet.Length < LabyrinthRoomMoveRequest.Length)
        {
            return;
        }

        LabyrinthRoomMoveRequest message = packet;
        await this._moveAction.MoveAsync(player, (LabyrinthDirection)message.Direction).ConfigureAwait(false);
    }
}
