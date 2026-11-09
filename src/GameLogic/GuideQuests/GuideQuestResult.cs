// <copyright file="GuideQuestResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GuideQuests;

/// <summary>
/// The answer to a request of the player, sent with the state.
/// </summary>
public enum GuideQuestResult : byte
{
    /// <summary>
    /// No request; the state changed or was asked for.
    /// </summary>
    None = 0,

    /// <summary>
    /// The step was accepted.
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// The step was completed and its rewards given.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The completion was refused: the objective isn't met.
    /// </summary>
    NotReady = 3,

    /// <summary>
    /// The step was refused: the character's level is too low.
    /// </summary>
    LevelTooLow = 4,
}
