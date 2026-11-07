// <copyright file="CharacterEventEntries.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The event entries of a character on a day, kept in the state file of the daily entry limits.
/// </summary>
public sealed class CharacterEventEntries
{
    /// <summary>
    /// Gets or sets the day (UTC) of the counts.
    /// </summary>
    public DateOnly Day { get; set; }

    /// <summary>
    /// Gets or sets the entries of the day by event.
    /// </summary>
    public Dictionary<MiniGameType, int> Counts { get; set; } = [];
}
