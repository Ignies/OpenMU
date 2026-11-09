// <copyright file="GuideQuestStatus.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GuideQuests;

/// <summary>
/// Where a character stands with its guide quest step.
/// </summary>
public enum GuideQuestStatus : byte
{
    /// <summary>
    /// The class has no guide quests.
    /// </summary>
    None = 0,

    /// <summary>
    /// The next step can be accepted.
    /// </summary>
    Offered = 1,

    /// <summary>
    /// The step is accepted and its objective isn't met yet.
    /// </summary>
    Active = 2,

    /// <summary>
    /// The step is accepted and its objective is met; it can be completed.
    /// </summary>
    Ready = 3,

    /// <summary>
    /// The next step asks for a higher level than the character has.
    /// </summary>
    LevelTooLow = 4,

    /// <summary>
    /// Every step of the class is done.
    /// </summary>
    Finished = 5,
}
