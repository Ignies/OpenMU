// <copyright file="MasterExperienceRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The Season 21 rule of the monster level which gives master experience: from a total level of 800 (the "enhanced"
/// levels, which feed the skill enhancement tree) the monster needs <see cref="ProgressionConfiguration.EnhancedMinimumMonsterLevel"/>.
/// </summary>
public static class MasterExperienceRules
{
    /// <summary>
    /// The total level (level plus master level) where the enhanced levels start. It's the Season 21 level cap 400 plus
    /// the 400 master levels of the master tree (S21Emu <c>Common.dat</c> "Monster EXP Enhanced 800+"); the modes which
    /// change the caps never reach it.
    /// </summary>
    internal const int EnhancedTotalLevel = 800;

    /// <summary>
    /// Gets the lowest monster level which gives master experience to a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The lowest monster level.</returns>
    public static int GetMinimumMonsterLevel(Player player)
    {
        var configuredMinimum = player.GameContext.Configuration.MinimumMonsterLevelForMasterExperience;
        return player.Attributes is { } attributes
            ? GetMinimumMonsterLevel((int)attributes[Stats.Level], (int)attributes[Stats.MasterLevel], configuredMinimum, ProgressionFeaturePlugIn.GetConfiguration(player.GameContext))
            : configuredMinimum;
    }

    /// <summary>
    /// Gets the lowest monster level which gives master experience.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="configuredMinimum">The minimum of the game configuration.</param>
    /// <param name="configuration">The progression configuration, or <c>null</c> when the progression is off.</param>
    /// <returns>The lowest monster level.</returns>
    public static int GetMinimumMonsterLevel(int level, int masterLevel, int configuredMinimum, ProgressionConfiguration? configuration)
    {
        return configuration is { EnhancedMinimumMonsterLevel: > 0 and var enhancedMinimum } && level + masterLevel >= EnhancedTotalLevel
            ? Math.Max(configuredMinimum, enhancedMinimum)
            : configuredMinimum;
    }
}
