// <copyright file="ResetPointsExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Resets;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Quests;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Applies the stat and point part of a reset, for players and bots alike.
/// </summary>
public static class ResetPointsExtensions
{
    /// <summary>
    /// Resets the stats (when configured) and sets or adds the level-up points of the reset.
    /// </summary>
    /// <param name="player">The player whose selected character is reset.</param>
    /// <param name="configuration">The reset configuration.</param>
    /// <param name="progression">The progression of this reset.</param>
    /// <remarks>
    /// When the stats go back to their base values and the points are replaced, the points of the finished quests are
    /// added again: the quests stay finished and can't be repeated, so without this every reset would lose them
    /// (80 points in Season 6).
    /// In the stars mode of the progression, a reset is a star: its points are levels' worth of the class's points per
    /// level (<see cref="StarRules.GetPointsForStar"/>), and it gives master tree points.
    /// </remarks>
    public static void ApplyResetPoints(this Player player, ResetConfiguration configuration, ResetProgression progression)
    {
        if (player.SelectedCharacter is not { CharacterClass: { } characterClass } character || player.Attributes is not { } attributes)
        {
            return;
        }

        if (configuration.ResetStats)
        {
            characterClass.StatAttributes
                .Where(s => s.IncreasableByPlayer)
                .ForEach(s => attributes[s.Attribute] = s.BaseValue);
        }

        var stars = ProgressionFeaturePlugIn.GetWorld(player.GameContext) is { Configuration: { Mode: ProgressionMode.Stars } starConfiguration }
            ? starConfiguration
            : null;
        var pointsPerLevel = (int)(characterClass.StatAttributes.FirstOrDefault(s => s.Attribute == Stats.PointsPerLevelUp)?.BaseValue ?? 0);
        if (stars is not null)
        {
            character.MasterLevelUpPoints += StarRules.GetTreePoints(1, stars);
        }

        if (!configuration.ReplacePointsPerReset)
        {
            character.LevelUpPoints += stars is null
                ? progression.PointsForReset
                : StarRules.GetPointsForStar(progression.NextResetCount, pointsPerLevel, stars);
            return;
        }

        var questPoints = configuration.ResetStats
            ? character.GetEarnedLevelUpPoints(player.GameContext.Configuration.Monsters.SelectMany(m => m.Quests))
            : 0;
        character.LevelUpPoints = questPoints + (stars is null
            ? progression.TotalPointsAfterReset
            : StarRules.GetTotalStarPoints(progression.NextResetCount, pointsPerLevel, stars));
    }
}
