// <copyright file="ChaosCastleSurvivalEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The points of a character in the tournament of a month, kept in the state file.
/// </summary>
public class ChaosCastleSurvivalEntry
{
    /// <summary>
    /// Gets or sets the first day of the month of the points.
    /// </summary>
    public DateOnly Month { get; set; }

    /// <summary>
    /// Gets or sets the points of the try-outs.
    /// </summary>
    public int TryOutPoints { get; set; }

    /// <summary>
    /// Gets or sets the points of the semi-finals.
    /// </summary>
    public int SemiFinalPoints { get; set; }

    /// <summary>
    /// Gets or sets the points of the final.
    /// </summary>
    public int FinalPoints { get; set; }
}
