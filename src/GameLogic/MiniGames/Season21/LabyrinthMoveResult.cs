// <copyright file="LabyrinthMoveResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The result of a move to another room of the Labyrinth of Dimensions.
/// </summary>
public enum LabyrinthMoveResult
{
    /// <summary>
    /// The character is in the next room.
    /// </summary>
    Moved,

    /// <summary>
    /// The side has no open portal, or no room behind it.
    /// </summary>
    NoPortal,

    /// <summary>
    /// The portal opens with the mission of the room, which isn't done.
    /// </summary>
    MissionNotDone,

    /// <summary>
    /// The character doesn't play the rooms of a labyrinth.
    /// </summary>
    NotInRooms,
}
