// <copyright file="IllusionTempleLeagueGuildEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The league of a guild in a month, kept in the guild state file.
/// </summary>
public class IllusionTempleLeagueGuildEntry
{
    /// <summary>
    /// Gets or sets the first day of the month of the league.
    /// </summary>
    public DateOnly Month { get; set; }

    /// <summary>
    /// Gets or sets the name of the guild.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of elimination matches the guild played.
    /// </summary>
    public int Entries { get; set; }

    /// <summary>
    /// Gets or sets the occupied stones of the eliminations.
    /// </summary>
    public int Stones { get; set; }

    /// <summary>
    /// Gets or sets the kills of the eliminations.
    /// </summary>
    public int Kills { get; set; }

    /// <summary>
    /// Gets or sets the level sums of the eliminations.
    /// </summary>
    public int LevelSum { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the guild won its semi-final.
    /// </summary>
    public bool IsFinalist { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the guild won the final.
    /// </summary>
    public bool IsChampion { get; set; }
}
