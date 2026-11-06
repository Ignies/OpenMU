// <copyright file="AddEnhanceTreePointAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Character;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.GameLogic.Views.Character;

/// <summary>
/// Adds a point to a node or passive of the skill enhancement tree.
/// </summary>
/// <remarks>
/// A node's first step costs its <see cref="EnhanceTreeNode.MinimumLevel"/> points, every further step one. Its
/// requirements are skills at a level (<c>-1</c>: learned, or available as a regular skill). A passive is unlocked in
/// stages by the points spent in the class's tree; each level costs the stage's points.
/// </remarks>
public class AddEnhanceTreePointAction
{
    /// <summary>
    /// Adds the point.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skillNumber">The skill number of the node.</param>
    /// <returns>The result.</returns>
    public async ValueTask<EnhanceTreePointResult> AddPointAsync(Player player, short skillNumber)
    {
        var (result, level) = await this.TryAddAsync(player, skillNumber).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IEnhanceTreeViewPlugIn>(p => p.ShowEnhanceTreePointResultAsync(result, skillNumber, level)).ConfigureAwait(false);
        return result;
    }

    private static bool IsMet(Player player, EnhanceTreeRequirement requirement)
    {
        if (player.SelectedCharacter!.LearnedSkills.FirstOrDefault(s => s.Skill?.Number == requirement.Skill) is { } learned)
        {
            return requirement.Points < 0 || learned.Level >= requirement.Points;
        }

        return requirement.Points <= 0 && player.SkillList?.ContainsSkill((ushort)requirement.Skill) is true;
    }

    private async ValueTask<(EnhanceTreePointResult Result, int Level)> TryAddAsync(Player player, short skillNumber)
    {
        if (EnhanceTreeFeaturePlugIn.GetConfiguration(player.GameContext) is not { } configuration
            || player.SelectedCharacter is not { CharacterClass: { } characterClass } character
            || characterClass.GetEvolution() is not (ClassEvolution.Fourth or ClassEvolution.Fifth)
            || ClassCodes.GetBaseClass(characterClass.Number) is not { } baseClass)
        {
            return (EnhanceTreePointResult.NotAvailable, 0);
        }

        var learned = character.LearnedSkills.FirstOrDefault(s => s.Skill?.Number == skillNumber);
        var level = learned?.Level ?? 0;
        var skill = player.GameContext.Configuration.Skills.FirstOrDefault(s => s.Number == skillNumber);
        int cost;
        int maximum;
        if (configuration.FindNode(baseClass, skillNumber) is { } node && skill is not null)
        {
            if (!node.Requirements.All(r => IsMet(player, r)))
            {
                return (EnhanceTreePointResult.RequirementMissing, level);
            }

            cost = level == 0 ? Math.Max(1, (int)node.MinimumLevel) : 1;
            maximum = node.MaximumLevel;
        }
        else if (configuration.FindPassive(baseClass, skillNumber) is { } passive && skill is not null)
        {
            var spent = EnhanceTreePoints.GetSpent(character.LearnedSkills, baseClass, configuration);
            if (passive.Stages.LastOrDefault(s => s.MaximumLevel > 0 && spent >= s.PointCondition) is not { } stage)
            {
                return (EnhanceTreePointResult.RequirementMissing, level);
            }

            cost = Math.Max(1, (int)stage.PointsPerLevel);
            maximum = stage.MaximumLevel;
        }
        else
        {
            return (EnhanceTreePointResult.UnknownNode, level);
        }

        if (level + cost > maximum)
        {
            return (EnhanceTreePointResult.MaximumReached, level);
        }

        if (EnhanceTreePoints.GetFree(player, configuration) < cost)
        {
            return (EnhanceTreePointResult.NotEnoughPoints, level);
        }

        if (learned is null)
        {
            await player.SkillList!.AddLearnedSkillAsync(skill).ConfigureAwait(false);
            learned = character.LearnedSkills.First(s => s.Skill == skill);
        }

        learned.Level += cost;
        return (EnhanceTreePointResult.Added, learned.Level);
    }
}
