// <copyright file="LabyrinthAreaType.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The kinds of rooms of the Labyrinth of Dimensions (<c>AreaType</c> of S21Emu <c>LabyrinthOfDimension_Path.xml</c>).
/// </summary>
public enum LabyrinthAreaType
{
    /// <summary>
    /// No room; it can not be entered.
    /// </summary>
    None,

    /// <summary>
    /// The room where a floor starts.
    /// </summary>
    Entrance,

    /// <summary>
    /// A room without a mission.
    /// </summary>
    Normal,

    /// <summary>
    /// A room with a mission, which opens its mission portals.
    /// </summary>
    Mission,

    /// <summary>
    /// The room whose mission clears the floor.
    /// </summary>
    MainMission,
}
