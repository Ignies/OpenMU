// <copyright file="LabyrinthPortal.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The portal of a side of a room of the Labyrinth of Dimensions.
/// </summary>
public enum LabyrinthPortal
{
    /// <summary>
    /// No portal.
    /// </summary>
    Closed,

    /// <summary>
    /// A portal which is always open.
    /// </summary>
    Open,

    /// <summary>
    /// A portal which opens with the mission of the room.
    /// </summary>
    Mission,

    /// <summary>
    /// A portal which needs a Dimensional Key; not used by the data, so it stays closed.
    /// </summary>
    Key,
}
