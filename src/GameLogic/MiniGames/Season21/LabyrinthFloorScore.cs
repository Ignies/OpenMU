// <copyright file="LabyrinthFloorScore.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The points and boxes of a floor of the Labyrinth of Dimensions.
/// </summary>
public class LabyrinthFloorScore
{
    /// <summary>
    /// Gets or sets the points of the first clear of the floor on a day.
    /// </summary>
    public int FirstPoints { get; set; }

    /// <summary>
    /// Gets or sets the points of each further clear of the floor on the same day.
    /// </summary>
    public int RepeatPoints { get; set; }

    /// <summary>
    /// Gets or sets the chances (0 to 1) of the boxes of <see cref="LabyrinthLeagueConfiguration.FloorBoxes"/>, in their order.
    /// </summary>
    public List<double> BoxChances { get; set; } = [];
}
