// <copyright file="LabyrinthArea.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// An area of another map which a room of the Labyrinth of Dimensions shows (<c>EventMaps</c> of S21Emu <c>LabyrinthOfDimension.xml</c>).
/// </summary>
public class LabyrinthArea
{
    /// <summary>
    /// Gets or sets the number of the area, which the rooms of the paths name.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the number of the map whose area it is.
    /// </summary>
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the first x coordinate.
    /// </summary>
    public byte StartX { get; set; }

    /// <summary>
    /// Gets or sets the first y coordinate.
    /// </summary>
    public byte StartY { get; set; }

    /// <summary>
    /// Gets or sets the last x coordinate.
    /// </summary>
    public byte EndX { get; set; }

    /// <summary>
    /// Gets or sets the last y coordinate.
    /// </summary>
    public byte EndY { get; set; }
}
