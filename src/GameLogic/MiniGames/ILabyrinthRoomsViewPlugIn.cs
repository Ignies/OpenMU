// <copyright file="ILabyrinthRoomsViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.Views;

/// <summary>
/// Interface for view plugins which show the rooms of the Labyrinth of Dimensions. A client without it plays the labyrinth
/// without rooms, because only a client which draws the area of another map can show them.
/// </summary>
public interface ILabyrinthRoomsViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the board of the floor and the room the character is in.
    /// </summary>
    /// <param name="board">The board.</param>
    /// <returns>A task.</returns>
    ValueTask ShowBoardAsync(LabyrinthBoard board);

    /// <summary>
    /// Shows the mission of the room and its progress.
    /// </summary>
    /// <param name="mission">The mission.</param>
    /// <returns>A task.</returns>
    ValueTask ShowMissionAsync(LabyrinthMissionView mission);

    /// <summary>
    /// Shows the result of a requested move to another room.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="result">The result.</param>
    /// <returns>A task.</returns>
    ValueTask ShowMoveResultAsync(LabyrinthDirection direction, LabyrinthMoveResult result);
}
