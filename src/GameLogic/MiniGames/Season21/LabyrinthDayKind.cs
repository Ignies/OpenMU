// <copyright file="LabyrinthDayKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The kind of a day of the month in the schedule of the labyrinth league.
/// </summary>
public enum LabyrinthDayKind
{
    /// <summary>
    /// The day isn't in the schedule: the labyrinth can be played, but no league points count.
    /// </summary>
    None,

    /// <summary>
    /// The first day of a league week: the week before is ranked.
    /// </summary>
    Start,

    /// <summary>
    /// A regular day of a league week.
    /// </summary>
    Play,

    /// <summary>
    /// The day after the last week: the characters get the reward of their tier.
    /// </summary>
    Reward,
}
