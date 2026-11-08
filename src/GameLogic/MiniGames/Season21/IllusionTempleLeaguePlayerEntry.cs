// <copyright file="IllusionTempleLeaguePlayerEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The league of a character in a month, kept in the player state file.
/// </summary>
public class IllusionTempleLeaguePlayerEntry
{
    /// <summary>
    /// Gets or sets the first day of the month of the league.
    /// </summary>
    public DateOnly Month { get; set; }

    /// <summary>
    /// Gets or sets the number of elimination matches the character played.
    /// </summary>
    public int Entries { get; set; }

    /// <summary>
    /// Gets or sets the guild for which the character played the final.
    /// </summary>
    public Guid? FinalGuild { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the character got the reward of the champion.
    /// </summary>
    public bool IsRewarded { get; set; }
}
