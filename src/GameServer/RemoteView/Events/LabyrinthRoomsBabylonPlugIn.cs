// <copyright file="LabyrinthRoomsBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Events;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The implementation of the <see cref="ILabyrinthRoomsViewPlugIn"/> for the Babylon client: the rooms of the Labyrinth of
/// Dimensions, <c>EE B6</c>, <c>EE B7</c> and <c>EE B9</c>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.LabyrinthRoomsBabylonPlugIn_Name), Description = nameof(PlugInResources.LabyrinthRoomsBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("F3A2F9E9-8D87-4283-8F10-2658F76D8F41")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class LabyrinthRoomsBabylonPlugIn : ILabyrinthRoomsViewPlugIn
{
    private const int RoomBitsLength = (LabyrinthPath.RoomCount + 7) / 8;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabyrinthRoomsBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public LabyrinthRoomsBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowBoardAsync(LabyrinthBoard board)
    {
        var area = board.Area;
        await this._player.Connection.SendLabyrinthRoomBoardAsync(
            (byte)Math.Clamp(board.Path, 0, byte.MaxValue),
            (byte)board.Room,
            (ushort)area.MapNumber,
            area.StartX,
            area.StartY,
            area.EndX,
            area.EndY,
            (byte)board.Type,
            (byte)Math.Clamp(area.Number, 0, byte.MaxValue),
            (byte)board.Portals[(int)LabyrinthDirection.Top],
            (byte)board.Portals[(int)LabyrinthDirection.Bottom],
            (byte)board.Portals[(int)LabyrinthDirection.Left],
            (byte)board.Portals[(int)LabyrinthDirection.Right],
            ToBits(board.Discovered),
            ToBits(board.Cleared)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowMissionAsync(LabyrinthMissionView mission)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var goals = mission.Goals;

        int Write()
        {
            var size = LabyrinthRoomMissionRef.GetRequiredSize(goals.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new LabyrinthRoomMissionRef(span)
            {
                AreaType = (byte)mission.Type,
                MissionIndex = (byte)Math.Clamp(mission.Index, 0, byte.MaxValue),
                Completed = mission.Completed,
                GoalCount = (byte)goals.Count,
            };
            for (var i = 0; i < goals.Count; i++)
            {
                var goal = packet[i];
                goal.Kind = (byte)goals[i].Kind;
                goal.Target = (uint)Math.Clamp(goals[i].Target, 0, uint.MaxValue);
                goal.Progress = (uint)Math.Clamp(goals[i].Progress, 0, uint.MaxValue);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowMoveResultAsync(LabyrinthDirection direction, LabyrinthMoveResult result)
    {
        await this._player.Connection.SendLabyrinthRoomMoveResultAsync((byte)direction, (byte)result).ConfigureAwait(false);
    }

    private static Memory<byte> ToBits(IReadOnlyList<bool> rooms)
    {
        var bits = new byte[RoomBitsLength];
        for (var i = 0; i < rooms.Count && i < LabyrinthPath.RoomCount; i++)
        {
            if (rooms[i])
            {
                bits[i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return bits;
    }
}
