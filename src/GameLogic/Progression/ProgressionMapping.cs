// <copyright file="ProgressionMapping.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Maps Season 21 requirements into the normalized level range.
/// </summary>
/// <remarks>
/// One curve turns a Season 21 total-level requirement R into a power level T(R) = A1000 * (R / 1000) ^ k, where the two
/// anchors fix k: T(400) = A400 and T(1000) = A1000 (190 and 400 by default). The curve keeps Season 21's order and
/// compresses the top (T(1600) = 586). Normalized splits T into level (up to the level cap) and master level; stars
/// split it into stars (bands of Season 21 levels) and the level which is left after the stars' worth.
/// </remarks>
public static class ProgressionMapping
{
    /// <summary>
    /// The lowest warp zen of a normalized set.
    /// </summary>
    /// <remarks>Fitted to the Season 6 warp list (Gates.cs), which costs about 50 zen per entry level.</remarks>
    internal const int EntryZenMinimum = 2000;

    /// <summary>
    /// The warp zen base of a normalized set, see <see cref="EntryZenMinimum"/>.
    /// </summary>
    internal const int EntryZenBase = 1000;

    /// <summary>
    /// The step the warp zen is rounded to.
    /// </summary>
    internal const int EntryZenStep = 500;

    /// <summary>
    /// The factor of the anchor monster level, see <see cref="GetMonsterAnchorLevel"/>.
    /// </summary>
    internal const double AnchorFactor = 7.602;

    /// <summary>
    /// The exponent of the anchor monster level, see <see cref="GetMonsterAnchorLevel"/>.
    /// </summary>
    internal const double AnchorExponent = 0.5026;

    /// <summary>
    /// The Season 21 cap of the normal level, which splits an original total requirement into level and master level.
    /// </summary>
    internal const int Season21LevelCap = 400;

    /// <summary>
    /// Gets the power level T(R) of a Season 21 requirement.
    /// </summary>
    /// <param name="season21Requirement">The Season 21 total-level requirement.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The power level, unrounded.</returns>
    public static double GetPowerLevel(int season21Requirement, ProgressionConfiguration configuration)
    {
        var anchor400 = (double)Math.Clamp(configuration.Anchor400, 150, 250);
        var anchor1000 = (double)Math.Clamp(configuration.Anchor1000, 300, 400);
        var exponent = Math.Log(anchor400 / anchor1000) / Math.Log(0.4);
        return anchor1000 * Math.Pow(Math.Max(0, season21Requirement) / 1000.0, exponent);
    }

    /// <summary>
    /// Gets the entry requirement of a Season 21 map set.
    /// </summary>
    /// <param name="season21Requirement">The Season 21 total-level requirement.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The requirement in the configured mode.</returns>
    public static MapRequirement GetRequirement(int season21Requirement, ProgressionConfiguration configuration)
    {
        if (configuration.Mode == ProgressionMode.Original)
        {
            return new(Math.Min(Season21LevelCap, season21Requirement), Math.Max(0, season21Requirement - Season21LevelCap), 0);
        }

        var levelCap = GetLevelCap(configuration);
        var powerLevel = GetPowerLevel(season21Requirement, configuration);
        if (configuration.Mode == ProgressionMode.Normalized)
        {
            var rounded = Round(powerLevel, configuration);
            return new(Math.Min(levelCap, rounded), Math.Clamp(rounded - levelCap, 0, GetMasterLevelCap(configuration)), 0);
        }

        var stars = StarRules.GetStarsRequired(season21Requirement, configuration);
        var level = Round(powerLevel - StarRules.GetStarWorth(stars, configuration), configuration);
        return new(Math.Clamp(level, 1, levelCap), 0, stars);
    }

    /// <summary>
    /// Gets the warp zen of a Season 21 map set.
    /// </summary>
    /// <param name="season21Requirement">The Season 21 total-level requirement.</param>
    /// <param name="sourceZen">The zen of the source data.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The warp zen in the configured mode.</returns>
    public static int GetEntryZen(int season21Requirement, int sourceZen, ProgressionConfiguration configuration)
    {
        if (configuration.Mode == ProgressionMode.Original)
        {
            return sourceZen;
        }

        if (configuration.EntryZenFlat is { } flat)
        {
            return Math.Max(0, flat);
        }

        var perLevel = Math.Clamp(configuration.EntryZenPerLevel, 0, 500);
        var zen = EntryZenBase + (perLevel * GetPowerLevel(season21Requirement, configuration));
        return Math.Max(EntryZenMinimum, (int)Math.Round(zen / EntryZenStep) * EntryZenStep);
    }

    /// <summary>
    /// Gets the anchor monster level of a map with the specified power level.
    /// </summary>
    /// <param name="powerLevel">The power level T of the map.</param>
    /// <returns>The level of the map's typical normal monster.</returns>
    /// <remarks>
    /// The median level of the normal monsters of a Season 6 hunting map against its entry level, fitted on 14 Season 6
    /// maps (Gates.cs levels, the Season 6 map spawns): Tarkan 140 has 83, Icarus 170 has 86, Raklion 280 has 142.
    /// </remarks>
    public static double GetMonsterAnchorLevel(double powerLevel)
    {
        return AnchorFactor * Math.Pow(Math.Max(0, powerLevel), AnchorExponent);
    }

    /// <summary>
    /// Gets the effective level cap.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The level cap.</returns>
    public static int GetLevelCap(ProgressionConfiguration configuration)
    {
        return Math.Clamp(configuration.LevelCap, ProgressionConfiguration.MinLevelCap, ProgressionConfiguration.MaxLevelCap);
    }

    /// <summary>
    /// Gets the effective master level cap.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The master level cap.</returns>
    public static int GetMasterLevelCap(ProgressionConfiguration configuration)
    {
        var (fallback, maximum) = configuration.Mode switch
        {
            ProgressionMode.Original => (ProgressionConfiguration.OriginalMasterLevelCap, ProgressionConfiguration.MaxOriginalMasterLevelCap),
            ProgressionMode.Normalized => (ProgressionConfiguration.MaxNormalizedMasterLevelCap, ProgressionConfiguration.MaxNormalizedMasterLevelCap),
            _ => (0, ProgressionConfiguration.MaxNormalizedMasterLevelCap),
        };

        return Math.Clamp(configuration.MasterLevelCap ?? fallback, 0, maximum);
    }

    /// <summary>
    /// Gets the effective level the event brackets are chosen by.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The level key.</returns>
    public static ProgressionLevelKey GetEventBracketKey(ProgressionConfiguration configuration)
    {
        return configuration.EventBracketKey ?? configuration.Mode switch
        {
            ProgressionMode.Original => ProgressionLevelKey.Season21Total,
            ProgressionMode.Normalized => ProgressionLevelKey.LevelAndMasterLevel,
            _ => ProgressionLevelKey.PowerLevel,
        };
    }

    /// <summary>
    /// Gets the effective level the item level requirements are checked against.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The level key.</returns>
    public static ProgressionLevelKey GetItemLevelRequirementKey(ProgressionConfiguration configuration)
    {
        return configuration.ItemLevelRequirementKey
               ?? (configuration.Mode == ProgressionMode.Stars ? ProgressionLevelKey.PowerLevel : ProgressionLevelKey.Level);
    }

    /// <summary>
    /// Gets the entry level of a classic (Season 6) map in the configured mode.
    /// </summary>
    /// <param name="season6Level">The Season 6 entry level.</param>
    /// <param name="season21Level">The Season 21 entry level.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The entry level.</returns>
    public static int GetClassicEntryLevel(int season6Level, int season21Level, ProgressionConfiguration configuration)
    {
        if (configuration.Mode == ProgressionMode.Original)
        {
            return season21Level;
        }

        return configuration.ClassicMapEntryLevels switch
        {
            ClassicMapEntryLevels.Season21 => season21Level,
            ClassicMapEntryLevels.Season21Normalized => Math.Max(1, (int)Math.Round(GetPowerLevel(season21Level, configuration))),
            _ => season6Level,
        };
    }

    /// <summary>
    /// Gets the monster level item drops are rolled with.
    /// </summary>
    /// <param name="monsterLevel">The monster level.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The drop level.</returns>
    /// <remarks>
    /// The drop generator keeps items whose drop level is within 12 of the monster level, and the highest drop level is
    /// 150; so without a cap a monster above level 161 finds no item of the general pool.
    /// </remarks>
    public static int GetDropLevel(int monsterLevel, ProgressionConfiguration configuration)
    {
        if (configuration.Mode == ProgressionMode.Original || configuration.MonsterDropLevelCap <= 0)
        {
            return monsterLevel;
        }

        return Math.Min(monsterLevel, Math.Clamp(configuration.MonsterDropLevelCap, 100, 255));
    }

    /// <summary>
    /// Rounds a level to the configured requirement step.
    /// </summary>
    /// <param name="value">The level.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The rounded level.</returns>
    internal static int Round(double value, ProgressionConfiguration configuration)
    {
        var step = Math.Clamp(configuration.RequirementRounding, 1, 10);
        return (int)Math.Round(value / step) * step;
    }
}
