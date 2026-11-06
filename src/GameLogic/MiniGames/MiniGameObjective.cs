// <copyright file="MiniGameObjective.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

/// <summary>
/// An objective which a player reached in a mini game, see <see cref="PlugIns.IMiniGameObjectiveReachedPlugIn"/>.
/// </summary>
public enum MiniGameObjective
{
    /// <summary>
    /// The player got a rank at the end of a mini game with a ranking (devil square); the value is the rank, 1 the best.
    /// </summary>
    Rank,

    /// <summary>
    /// The player took the relic of a statue (illusion temple).
    /// </summary>
    RelicClaimed,

    /// <summary>
    /// The player brought a relic home and scored for the team (illusion temple).
    /// </summary>
    RelicScored,
}
