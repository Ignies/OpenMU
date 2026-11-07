// <copyright file="StarRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using MUnique.OpenMU.GameLogic.Resets;

/// <summary>
/// The rules of the stars. A star is one reset of the reset feature (<see cref="ResetFeaturePlugIn"/>): the star count is
/// the character's reset count.
/// </summary>
/// <remarks>
/// A star stands for 100 power levels (50 after the first tier), so the power level E = level + the stars' worth, and a
/// character at a star band's entry has the map's power: 1 star level 200 is E 300 = T(700), 2 stars level 200 is
/// E 400 = T(1000). E is used for item and event requirements, never shown as a level.
/// Rewards per star: stat points of 100 levels (50 after the first tier) of the class, master tree points, and a capped
/// damage and defense bonus against monsters. Each star lowers the experience a little, down to a floor.
/// A star clears level, experience and the stat distribution, and keeps everything else, including the points of the
/// finished quests.
/// </remarks>
public static class StarRules
{
    private static readonly int[] DefaultTierThresholds = [1, 4, 7, 10];

    /// <summary>
    /// Gets the stars a Season 21 requirement asks for in the stars mode: one per band above <see cref="ProgressionConfiguration.StarBandStart"/>.
    /// </summary>
    /// <param name="season21Requirement">The Season 21 total-level requirement.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The required stars.</returns>
    public static int GetStarsRequired(int season21Requirement, ProgressionConfiguration configuration)
    {
        var start = Math.Clamp(configuration.StarBandStart, 0, 1000);
        var width = Math.Clamp(configuration.StarBandWidth, 100, 600);
        return season21Requirement < start + width ? 0 : (season21Requirement - start) / width;
    }

    /// <summary>
    /// Gets the power levels a star count is worth.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The worth in power levels.</returns>
    public static int GetStarWorth(int stars, ProgressionConfiguration configuration)
    {
        return GetTiered(stars, Math.Clamp(configuration.StarWorthFirstTier, 0, 400), Math.Clamp(configuration.StarWorthSecondTier, 0, 400), configuration);
    }

    /// <summary>
    /// Gets the power level E: the level plus the stars' worth.
    /// </summary>
    /// <param name="level">The normal level.</param>
    /// <param name="stars">The stars.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The power level, uncapped.</returns>
    public static int GetPowerLevel(int level, int stars, ProgressionConfiguration configuration)
    {
        return level + GetStarWorth(stars, configuration);
    }

    /// <summary>
    /// Gets the level a requirement (event bracket, item level) is checked against.
    /// </summary>
    /// <param name="key">The level key, see <see cref="ProgressionMapping.GetEventBracketKey"/> and <see cref="ProgressionMapping.GetItemLevelRequirementKey"/>.</param>
    /// <param name="level">The normal level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The level to check.</returns>
    public static int GetRequirementLevel(ProgressionLevelKey key, int level, int masterLevel, int stars, ProgressionConfiguration configuration)
    {
        var levelCap = ProgressionMapping.GetLevelCap(configuration);
        return key switch
        {
            ProgressionLevelKey.LevelAndMasterLevel => Math.Min(levelCap, level + masterLevel),
            ProgressionLevelKey.PowerLevel => Math.Min(levelCap, GetPowerLevel(level, stars, configuration)),
            ProgressionLevelKey.Season21Total => level + masterLevel,
            _ => level,
        };
    }

    /// <summary>
    /// Gets the stat points one star gives.
    /// </summary>
    /// <param name="star">The star (1 for the first one).</param>
    /// <param name="pointsPerLevel">The level-up points per level of the character class.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The stat points.</returns>
    public static int GetPointsForStar(int star, int pointsPerLevel, ProgressionConfiguration configuration)
    {
        if (star <= 0)
        {
            return 0;
        }

        var levels = star <= GetFirstTierSize(configuration) ? configuration.StarPointsFirstTier : configuration.StarPointsSecondTier;
        return Math.Clamp(levels, 0, 400) * Math.Max(0, pointsPerLevel);
    }

    /// <summary>
    /// Gets the stat points of all stars up to a star count.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="pointsPerLevel">The level-up points per level of the character class.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The stat points.</returns>
    public static int GetTotalStarPoints(int stars, int pointsPerLevel, ProgressionConfiguration configuration)
    {
        var levels = GetTiered(stars, Math.Clamp(configuration.StarPointsFirstTier, 0, 400), Math.Clamp(configuration.StarPointsSecondTier, 0, 400), configuration);
        return levels * Math.Max(0, pointsPerLevel);
    }

    /// <summary>
    /// Gets the free stat points after a star: the points of all stars plus the points of the finished quests.
    /// </summary>
    /// <param name="stars">The stars after the reset.</param>
    /// <param name="pointsPerLevel">The level-up points per level of the character class.</param>
    /// <param name="questPoints">The level-up points of the finished quests.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The free stat points.</returns>
    public static int GetFreePointsAfterStar(int stars, int pointsPerLevel, int questPoints, ProgressionConfiguration configuration)
    {
        return GetTotalStarPoints(stars, pointsPerLevel, configuration) + Math.Max(0, questPoints);
    }

    /// <summary>
    /// Gets the master tree points of a star count, at most <see cref="ProgressionConfiguration.MasterTreePointsCap"/>.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The master tree points.</returns>
    public static int GetTreePoints(int stars, ProgressionConfiguration configuration)
    {
        return MasterTreeRules.Cap(Math.Max(0, stars) * Math.Clamp(configuration.StarTreePoints, 0, 50), configuration);
    }

    /// <summary>
    /// Gets the damage and defense bonus of a star count.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="againstPlayer">If set to <c>true</c>, the opponent is a player.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The bonus in percent.</returns>
    public static int GetPowerBonusPercent(int stars, bool againstPlayer, ProgressionConfiguration configuration)
    {
        if (againstPlayer && !configuration.StarPowerInPvp)
        {
            return 0;
        }

        return Math.Min(Math.Clamp(configuration.StarPowerCapPercent, 0, 50), Math.Max(0, stars) * Math.Clamp(configuration.StarPowerPercent, 0, 5));
    }

    /// <summary>
    /// Gets the experience factor of a star count.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The factor the experience is multiplied with.</returns>
    public static double GetExperienceFactor(int stars, ProgressionConfiguration configuration)
    {
        var penalty = Math.Clamp(configuration.StarExperiencePenaltyPercent, 0, 20);
        var floor = Math.Clamp(configuration.StarExperienceFloorPercent, 10, 100);
        return Math.Max(floor, 100 - (Math.Max(0, stars) * penalty)) / 100.0;
    }

    /// <summary>
    /// Gets the cosmetic tier of a star count.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <returns>The tier.</returns>
    /// <remarks>The website owns the tier thresholds and names; these are its defaults.</remarks>
    public static StarTier GetTier(int stars)
    {
        var tier = StarTier.None;
        for (var i = 0; i < DefaultTierThresholds.Length && stars >= DefaultTierThresholds[i]; i++)
        {
            tier = (StarTier)(i + 1);
        }

        return tier;
    }

    /// <summary>
    /// Gets what the next star costs and asks for, from the reset configuration.
    /// </summary>
    /// <param name="currentStars">The current stars.</param>
    /// <param name="resetConfiguration">The reset configuration.</param>
    /// <param name="configuration">The progression configuration.</param>
    /// <returns>The requirement of the next star.</returns>
    public static StarRequirement GetRequirement(int currentStars, ResetConfiguration resetConfiguration, ProgressionConfiguration configuration)
    {
        var progression = ResetProgressionCalculator.Calculate(currentStars, 0, resetConfiguration);
        return new StarRequirement(
            progression.NextResetCount,
            resetConfiguration.RequiredLevel,
            progression.RequiredZen,
            progression.RequiredItemAmount,
            configuration.StarRequiresMasterClass,
            resetConfiguration.ResetLimit > 0 && progression.NextResetCount > resetConfiguration.ResetLimit);
    }

    private static int GetFirstTierSize(ProgressionConfiguration configuration) => Math.Clamp(configuration.StarFirstTierSize, 0, 50);

    private static int GetTiered(int stars, int firstTier, int secondTier, ProgressionConfiguration configuration)
    {
        var count = Math.Max(0, stars);
        var firstTierStars = Math.Min(count, GetFirstTierSize(configuration));
        return (firstTierStars * firstTier) + ((count - firstTierStars) * secondTier);
    }
}
