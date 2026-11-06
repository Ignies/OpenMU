// <copyright file="RankingKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HuntingRecord;

/// <summary>
/// The kinds of the in-game ranking.
/// </summary>
public enum RankingKind
{
    /// <summary>
    /// The total level, level and master level.
    /// </summary>
    TotalLevel,

    /// <summary>
    /// The monsters killed since the week started.
    /// </summary>
    WeeklyKills,

    /// <summary>
    /// The hunting experience since the week started.
    /// </summary>
    WeeklyExperience,
}
