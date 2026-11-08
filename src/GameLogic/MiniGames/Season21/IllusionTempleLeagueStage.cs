// <copyright file="IllusionTempleLeagueStage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The stages of the Illusion Temple League, by the day of the month.
/// </summary>
public enum IllusionTempleLeagueStage
{
    /// <summary>
    /// No league day.
    /// </summary>
    None,

    /// <summary>
    /// A day of the eliminations: every guild party may play, the results add up.
    /// </summary>
    Elimination,

    /// <summary>
    /// The semi-finals of the best four guilds of the eliminations.
    /// </summary>
    SemiFinal,

    /// <summary>
    /// The final of the winners of the semi-finals.
    /// </summary>
    Final,

    /// <summary>
    /// The reward day: the players of the champion get their reward.
    /// </summary>
    Reward,
}
