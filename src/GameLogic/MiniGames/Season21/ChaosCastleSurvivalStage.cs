// <copyright file="ChaosCastleSurvivalStage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The stages of the tournament of the Chaos Castle Survival of the Fittest.
/// </summary>
public enum ChaosCastleSurvivalStage
{
    /// <summary>
    /// No stage today.
    /// </summary>
    None,

    /// <summary>
    /// The try-outs, open to everybody; their points add up.
    /// </summary>
    TryOut,

    /// <summary>
    /// The semi-finals of the best of the try-outs.
    /// </summary>
    SemiFinal,

    /// <summary>
    /// The final of the best of the semi-finals.
    /// </summary>
    Final,
}
