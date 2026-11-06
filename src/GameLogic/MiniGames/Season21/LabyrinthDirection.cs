// <copyright file="LabyrinthDirection.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The sides of a room of the Labyrinth of Dimensions, in the order of the portals of the data.
/// </summary>
public enum LabyrinthDirection
{
    /// <summary>
    /// To the room of the row above.
    /// </summary>
    Top,

    /// <summary>
    /// To the room of the row below.
    /// </summary>
    Bottom,

    /// <summary>
    /// To the room of the column on the left.
    /// </summary>
    Left,

    /// <summary>
    /// To the room of the column on the right.
    /// </summary>
    Right,
}
