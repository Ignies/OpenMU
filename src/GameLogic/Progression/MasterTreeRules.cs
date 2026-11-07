// <copyright file="MasterTreeRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The Season 21 rules of the master tree points: they stop at <see cref="ProgressionConfiguration.MasterTreePointsCap"/>
/// (400), the later levels feed the skill enhancement tree.
/// </summary>
/// <remarks>
/// The master levels and the stars are capped on their own; in the stars mode the master levels stop at 200, so a
/// character can't fill the whole Season 6 tree either way.
/// </remarks>
public static class MasterTreeRules
{
    /// <summary>
    /// Gets the progression configuration of a game context, also in the original mode.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The configuration, or <c>null</c> when the progression is off.</returns>
    public static ProgressionConfiguration? GetConfiguration(IGameContext gameContext)
        => gameContext.FeaturePlugIns.GetPlugIn<ProgressionFeaturePlugIn>()?.Configuration;

    /// <summary>
    /// Caps the master tree points earned in total.
    /// </summary>
    /// <param name="points">The uncapped points.</param>
    /// <param name="configuration">The progression configuration, or <c>null</c> when the progression is off.</param>
    /// <returns>The capped points.</returns>
    public static int Cap(int points, ProgressionConfiguration? configuration)
        => configuration is { MasterTreePointsCap: > 0 and var cap } ? Math.Min(points, cap) : points;

    /// <summary>
    /// Gets the master tree points a master level up gives.
    /// </summary>
    /// <param name="masterLevel">The reached master level.</param>
    /// <param name="pointsPerLevel">The points per master level.</param>
    /// <param name="configuration">The progression configuration, or <c>null</c> when the progression is off.</param>
    /// <returns>The points.</returns>
    public static int GetPointsForMasterLevel(int masterLevel, int pointsPerLevel, ProgressionConfiguration? configuration)
    {
        if (pointsPerLevel <= 0 || masterLevel < 1)
        {
            return pointsPerLevel;
        }

        return Cap(masterLevel * pointsPerLevel, configuration) - Cap((masterLevel - 1) * pointsPerLevel, configuration);
    }
}
