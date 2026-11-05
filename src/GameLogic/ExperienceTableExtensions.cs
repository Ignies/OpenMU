// <copyright file="ExperienceTableExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

/// <summary>
/// Extensions for the experience tables of the <see cref="IGameContext"/>.
/// </summary>
public static class ExperienceTableExtensions
{
    /// <summary>
    /// Gets the experience which is required for the next level.
    /// </summary>
    /// <param name="experienceTable">The experience table, e.g. <see cref="IGameContext.ExperienceTable"/>.</param>
    /// <param name="currentLevel">The current level.</param>
    /// <returns>The experience which is required for the next level.</returns>
    /// <remarks>
    /// The tables are as long as the level caps of the configuration require. A character can be above a cap when it
    /// was lowered after the character reached the level; it gets the last entry instead of an exception, so it can
    /// still enter the game. It gains no further levels, because the game logic stops at the cap.
    /// </remarks>
    public static long GetExperienceOfNextLevel(this long[] experienceTable, int currentLevel)
    {
        return experienceTable[Math.Clamp(currentLevel + 1, 0, experienceTable.Length - 1)];
    }
}
