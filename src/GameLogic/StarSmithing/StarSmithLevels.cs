// <copyright file="StarSmithLevels.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The smith's levels (Readme.md): 1 to 100, round(100 * L^1.6) experience from a level to the next.
/// </summary>
public static class StarSmithLevels
{
    /// <summary>The highest level.</summary>
    public const int Maximum = 100;

    /// <summary>
    /// The experience a level needs to reach the next one; 0 at the highest level.
    /// </summary>
    /// <param name="level">The level (below 1 counts as 1).</param>
    /// <returns>The experience.</returns>
    public static long ToNext(int level) => level >= Maximum ? 0 : (long)Math.Round(100 * Math.Pow(Math.Max(1, level), 1.6));

    /// <summary>
    /// Adds earned experience: the level rises as far as it reaches, the rest is kept; at the highest level nothing is kept.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="experience">The experience into the level.</param>
    /// <param name="earned">The earned experience.</param>
    /// <returns>The new level and the experience into it.</returns>
    public static (int Level, long Experience) Add(int level, long experience, long earned)
    {
        level = Math.Clamp(level, 1, Maximum);
        experience = Math.Max(0, experience) + Math.Max(0, earned);
        while (level < Maximum && experience >= ToNext(level))
        {
            experience -= ToNext(level);
            level++;
        }

        return (level, level >= Maximum ? 0 : experience);
    }
}
