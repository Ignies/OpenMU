// <copyright file="EnhanceTreePoints.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The points of the skill enhancement tree. Season 21 gives one point per level above a total level of 800 (level plus
/// master level), up to 950; the progression mode reads the character's level as that total, like a map requirement.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Original (or the progression off): the total is level plus master level.</item>
/// <item>Normalized: level plus master level is the power level T; the total is its Season 21 requirement R(T), so the
/// master levels feed the tree.</item>
/// <item>Stars: the power level E (level plus the stars' worth) is read the same way, so each star feeds the tree.</item>
/// </list>
/// Only a fourth or fifth evolution has the tree. The points are computed, never stored: the free points are the total
/// minus the levels of the learned tree skills.
/// </remarks>
public static class EnhanceTreePoints
{
    /// <summary>
    /// Gets the Season 21 total level which a character's level stands for.
    /// </summary>
    /// <param name="level">The normal level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars.</param>
    /// <param name="progression">The progression configuration, or <c>null</c> in the original mode or when it is off.</param>
    /// <returns>The Season 21 total level.</returns>
    public static int GetSeason21Total(int level, int masterLevel, int stars, ProgressionConfiguration? progression)
    {
        return progression?.Mode switch
        {
            ProgressionMode.Normalized => (int)Math.Round(ProgressionMapping.GetSeason21Requirement(level + masterLevel, progression)),
            ProgressionMode.Stars => (int)Math.Round(ProgressionMapping.GetSeason21Requirement(StarRules.GetPowerLevel(level, stars, progression), progression)),
            _ => level + masterLevel,
        };
    }

    /// <summary>
    /// Gets the points a character has in total.
    /// </summary>
    /// <param name="evolution">The evolution of the character's class.</param>
    /// <param name="level">The normal level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="stars">The stars.</param>
    /// <param name="progression">The progression configuration, or <c>null</c> in the original mode or when it is off.</param>
    /// <param name="configuration">The tree configuration.</param>
    /// <returns>The total points.</returns>
    public static int GetTotal(ClassEvolution? evolution, int level, int masterLevel, int stars, ProgressionConfiguration? progression, EnhanceTreeConfiguration configuration)
    {
        if (evolution is not (ClassEvolution.Fourth or ClassEvolution.Fifth))
        {
            return 0;
        }

        var total = GetSeason21Total(level, masterLevel, stars, progression);
        return Math.Clamp(total - configuration.PointsStartAt, 0, Math.Max(0, configuration.MaximumPoints));
    }

    /// <summary>
    /// Gets the points spent in the tree: the levels of the learned skills which are nodes or passives of the class's tree.
    /// </summary>
    /// <param name="learnedSkills">The learned skills.</param>
    /// <param name="baseClass">The base class.</param>
    /// <param name="configuration">The tree configuration.</param>
    /// <returns>The spent points.</returns>
    public static int GetSpent(IEnumerable<SkillEntry> learnedSkills, BaseClassCode baseClass, EnhanceTreeConfiguration configuration)
    {
        var treeSkills = configuration.GetTreeSkills(baseClass);
        return learnedSkills.Where(entry => entry.Skill is { } skill && treeSkills.Contains(skill.Number)).Sum(entry => entry.Level);
    }

    /// <summary>
    /// Gets the free points of a player's character.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="configuration">The tree configuration.</param>
    /// <returns>The free points, <c>0</c> for a class without the tree.</returns>
    public static int GetFree(Player player, EnhanceTreeConfiguration configuration)
    {
        if (player.SelectedCharacter is not { CharacterClass: { } characterClass } character
            || player.Attributes is not { } attributes
            || ClassCodes.GetBaseClass(characterClass.Number) is not { } baseClass)
        {
            return 0;
        }

        var total = GetTotal(
            characterClass.GetEvolution(),
            (int)attributes[Stats.Level],
            (int)attributes[Stats.MasterLevel],
            (int)attributes[Stats.Resets],
            ProgressionFeaturePlugIn.GetWorld(player.GameContext)?.Configuration,
            configuration);
        return Math.Max(0, total - GetSpent(character.LearnedSkills, baseClass, configuration));
    }
}
