// <copyright file="EvolutionRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The level rules of the fourth and fifth evolution. Season 21 asks for a total level (level plus master level) of
/// 800 and 1200 (S21Emu <c>Quest/Quest.xml</c>, quests 7 and 10); the progression mode reads that total like a map
/// requirement: Original keeps it, Normalized asks for T(R) levels plus master levels and Stars for the power level
/// E = T(R), see <see cref="ProgressionMapping.GetPowerLevel(int, ProgressionConfiguration)"/>.
/// </summary>
/// <remarks>
/// The first three evolutions keep their quests and levels, these rules don't apply to them. In Stars, the fifth
/// evolution (E 465) always needs a star, as the level cap is 400.
/// </remarks>
public static class EvolutionRules
{
    /// <summary>
    /// Gets the Season 21 total level an evolution asks for.
    /// </summary>
    /// <param name="evolution">The evolution.</param>
    /// <returns>The total level, or <c>0</c> for an evolution without a level rule.</returns>
    public static int GetSeason21Requirement(ClassEvolution evolution) => evolution switch
    {
        ClassEvolution.Fourth => 800,
        ClassEvolution.Fifth => 1200,
        _ => 0,
    };

    /// <summary>
    /// Gets the key which the requirement is checked against.
    /// </summary>
    /// <param name="configuration">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <returns>The level key.</returns>
    public static ProgressionLevelKey GetLevelKey(ProgressionConfiguration? configuration) => configuration?.Mode switch
    {
        ProgressionMode.Stars => ProgressionLevelKey.PowerLevel,
        ProgressionMode.Normalized => ProgressionLevelKey.LevelAndMasterLevel,
        _ => ProgressionLevelKey.Season21Total,
    };

    /// <summary>
    /// Gets the level an evolution asks for, in the unit of <see cref="GetLevelKey"/>.
    /// </summary>
    /// <param name="evolution">The evolution.</param>
    /// <param name="configuration">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <returns>The required level, <c>0</c> for an evolution without a level rule.</returns>
    public static int GetRequiredLevel(ClassEvolution evolution, ProgressionConfiguration? configuration)
        => GetRequiredLevel(GetSeason21Requirement(evolution), configuration);

    /// <summary>
    /// Gets the level a Season 21 total level asks for in the world's mode, in the unit of <see cref="GetLevelKey"/>:
    /// the evolutions, the level rewards and the other Season 21 thresholds share this mapping.
    /// </summary>
    /// <param name="requirement">The Season 21 total level (level plus master level).</param>
    /// <param name="configuration">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <returns>The required level, <c>0</c> for no requirement.</returns>
    public static int GetRequiredLevel(int requirement, ProgressionConfiguration? configuration)
    {
        if (requirement <= 0 || GetLevelKey(configuration) == ProgressionLevelKey.Season21Total)
        {
            return requirement;
        }

        return ProgressionMapping.Round(ProgressionMapping.GetPowerLevel(requirement, configuration!), configuration!);
    }

    /// <summary>
    /// Determines whether a character meets the level rule of an evolution.
    /// </summary>
    /// <param name="evolution">The evolution.</param>
    /// <param name="level">The normal level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars (resets).</param>
    /// <param name="configuration">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <returns><c>true</c>, if the character meets the rule.</returns>
    public static bool IsMet(ClassEvolution evolution, int level, int masterLevel, int stars, ProgressionConfiguration? configuration)
        => IsMet(GetSeason21Requirement(evolution), level, masterLevel, stars, configuration);

    /// <summary>
    /// Determines whether a character meets a Season 21 total level in the world's mode: the total level (original),
    /// level plus master level against the mapped level (normalized), or the power level of level and stars (stars).
    /// </summary>
    /// <param name="requirement">The Season 21 total level.</param>
    /// <param name="level">The normal level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars (resets).</param>
    /// <param name="configuration">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <returns><c>true</c>, if the character meets it.</returns>
    public static bool IsMet(int requirement, int level, int masterLevel, int stars, ProgressionConfiguration? configuration)
        => GetLevelsMissing(requirement, level, masterLevel, stars, configuration) <= 0;

    /// <summary>
    /// Gets how many levels of the world's mode a character is below a Season 21 total level, in the unit of
    /// <see cref="GetLevelKey"/>: the requirement maps like in <see cref="IsMet(int, int, int, int, ProgressionConfiguration?)"/>.
    /// </summary>
    /// <param name="requirement">The Season 21 total level.</param>
    /// <param name="level">The normal level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars (resets).</param>
    /// <param name="configuration">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <returns>The missing levels; zero or less when the character meets it.</returns>
    public static int GetLevelsMissing(int requirement, int level, int masterLevel, int stars, ProgressionConfiguration? configuration)
    {
        var actual = GetLevelKey(configuration) == ProgressionLevelKey.PowerLevel
            ? StarRules.GetPowerLevel(level, stars, configuration!)
            : level + masterLevel;
        return GetRequiredLevel(requirement, configuration) - actual;
    }
}
