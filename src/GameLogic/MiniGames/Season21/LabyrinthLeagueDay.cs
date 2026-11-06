// <copyright file="LabyrinthLeagueDay.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// A day of the month in the schedule of the labyrinth league.
/// </summary>
public class LabyrinthLeagueDay
{
    /// <summary>
    /// Gets or sets the day of the month, 1-31.
    /// </summary>
    public byte Day { get; set; }

    /// <summary>
    /// Gets or sets the kind of the day.
    /// </summary>
    public LabyrinthDayKind Kind { get; set; }
}
