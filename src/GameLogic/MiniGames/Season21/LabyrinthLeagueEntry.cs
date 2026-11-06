// <copyright file="LabyrinthLeagueEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The state of a character in the labyrinth league, kept in the state file.
/// </summary>
public class LabyrinthLeagueEntry
{
    /// <summary>
    /// Gets or sets the tier.
    /// </summary>
    public LabyrinthLeagueTier Tier { get; set; }

    /// <summary>
    /// Gets or sets the first day of the month of the league the tier belongs to.
    /// </summary>
    public DateOnly Cycle { get; set; }

    /// <summary>
    /// Gets or sets the first day of the week the points belong to, or <c>null</c>.
    /// </summary>
    public DateOnly? Week { get; set; }

    /// <summary>
    /// Gets or sets the group of the week, among the characters of the tier.
    /// </summary>
    public int Group { get; set; }

    /// <summary>
    /// Gets or sets the points of the week.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets the time of the first points of the week; the earlier wins a tie.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Gets or sets the month of the last claimed reward.
    /// </summary>
    public DateOnly? RewardCycle { get; set; }

    /// <summary>
    /// Gets or sets the day of <see cref="ClearedFloors"/>.
    /// </summary>
    public DateOnly Day { get; set; }

    /// <summary>
    /// Gets or sets the floors cleared on <see cref="Day"/>.
    /// </summary>
    public List<int> ClearedFloors { get; set; } = [];
}
