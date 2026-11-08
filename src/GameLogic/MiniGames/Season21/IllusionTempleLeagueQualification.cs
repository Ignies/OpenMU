// <copyright file="IllusionTempleLeagueQualification.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// How far a guild came in the Illusion Temple League of the month.
/// </summary>
public enum IllusionTempleLeagueQualification
{
    /// <summary>
    /// Not one of the best four of the eliminations.
    /// </summary>
    None,

    /// <summary>
    /// Plays the semi-final in the first temple.
    /// </summary>
    SemiFinalTemple1,

    /// <summary>
    /// Plays the semi-final in the second temple.
    /// </summary>
    SemiFinalTemple2,

    /// <summary>
    /// Won its semi-final.
    /// </summary>
    Finalist,

    /// <summary>
    /// Won the final.
    /// </summary>
    Champion,
}
