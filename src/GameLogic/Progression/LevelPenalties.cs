// <copyright file="LevelPenalties.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The damage and experience penalties of S21Emu for a character below a monster's level plus an offset
/// (<c>DamagePenalty.lua</c>, <c>ExperienciePenalty.lua</c>).
/// </summary>
/// <remarks>
/// The threshold (monster level + offset) and the labyrinth's total level are Season 21 total levels. They map through
/// the world's progression mode like every Season 21 total (<see cref="EvolutionRules.GetLevelsMissing"/>), so the
/// levels below count in the mode's unit: total level (original), level + master level (normalized), power level (stars).
/// </remarks>
public static class LevelPenalties
{
    /// <summary>
    /// Gets the damage penalty in percent.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="progression">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <param name="mapNumber">The number of the character's map.</param>
    /// <param name="evolution">The evolution of the character's class.</param>
    /// <param name="level">The level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars.</param>
    /// <param name="monsterLevel">The stored level of the monster.</param>
    /// <returns>The percent.</returns>
    public static int GetDamagePenaltyPercent(LevelPenaltyConfiguration configuration, ProgressionConfiguration? progression, short mapNumber, ClassEvolution? evolution, int level, int masterLevel, int stars, int monsterLevel)
    {
        if (!configuration.DamagePenaltyActive || configuration.PenaltyFreeMaps.Contains(mapNumber))
        {
            return 0;
        }

        var applies = configuration.DamageLevelMaps.Contains(mapNumber)
            ? EvolutionRules.IsMet(configuration.DamageLevelMapMinimumLevel, level, masterLevel, stars, progression)
            : evolution >= configuration.DamageMinimumEvolution;
        return applies ? GetPercent(configuration, progression, mapNumber, level, masterLevel, stars, monsterLevel) : 0;
    }

    /// <summary>
    /// Gets the experience penalty in percent.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="progression">The progression configuration, or <c>null</c> if the progression is off.</param>
    /// <param name="monsterMapNumber">The number of the monster's map.</param>
    /// <param name="level">The level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars.</param>
    /// <param name="monsterLevel">The stored level of the monster.</param>
    /// <returns>The percent.</returns>
    public static int GetExperiencePenaltyPercent(LevelPenaltyConfiguration configuration, ProgressionConfiguration? progression, short monsterMapNumber, int level, int masterLevel, int stars, int monsterLevel)
    {
        return configuration.ExperiencePenaltyActive && configuration.ExperienceMaps.Contains(monsterMapNumber)
            ? GetPercent(configuration, progression, monsterMapNumber, level, masterLevel, stars, monsterLevel)
            : 0;
    }

    /// <summary>
    /// Applies a penalty to a damage.
    /// </summary>
    /// <param name="damage">The damage.</param>
    /// <param name="percent">The percent.</param>
    /// <param name="decrease">If set to <c>true</c>, the damage decreases (the character hits), else it increases.</param>
    /// <returns>The damage after the penalty.</returns>
    public static int ApplyToDamage(int damage, int percent, bool decrease)
    {
        if (percent <= 0 || damage <= 0)
        {
            return damage;
        }

        var change = (long)damage * percent;
        return decrease ? damage - (int)((change + 99) / 100) : damage + (int)(change / 100);
    }

    private static int GetPercent(LevelPenaltyConfiguration configuration, ProgressionConfiguration? progression, short mapNumber, int level, int masterLevel, int stars, int monsterLevel)
    {
        var penaltyLevel = configuration.MapPenaltyLevels.TryGetValue(mapNumber, out var mapLevel) ? mapLevel : monsterLevel + configuration.MonsterLevelOffset;
        var below = EvolutionRules.GetLevelsMissing(penaltyLevel, level, masterLevel, stars, progression);
        var levels = Math.Clamp(below, 0, Math.Max(0, configuration.MaximumLevels));
        return Math.Clamp(levels * configuration.PercentPerLevel, 0, 100);
    }
}
