// <copyright file="EnhanceTreeEffects.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MUnique.OpenMU.GameLogic.Classes;
using org.mariuszgromada.math.mxparser;

/// <summary>
/// The node effects of the skill enhancement tree which OpenMU carries: the skill damage of "Add Ext Skill Damage".
/// </summary>
public static class EnhanceTreeEffects
{
    /// <summary>
    /// The skill number of the node "Add Ext Skill Damage": its value adds to the damage of the skill which the extension
    /// node of its sheet improves.
    /// </summary>
    public const short ExtSkillDamage = 1001;

    private const byte ExtensionNodeIndex = 0;

    private static readonly ConcurrentDictionary<(string Formula, int Level), float> Values = new();

    private static readonly ConditionalWeakTable<EnhanceTreeConfiguration, Dictionary<(byte BaseClass, short Skill), (short Extension, (short Skill, string Formula)[] Damage)[]>> Indexes = new();

    /// <summary>
    /// Gets the damage which the learned nodes of a player add to a skill, when the tree is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skill">The used skill.</param>
    /// <returns>The added damage.</returns>
    public static float GetExtraSkillDamage(Player player, Skill skill)
        => player.SkillList is { } skills
           && player.SelectedCharacter?.CharacterClass is { } characterClass
           && ClassCodes.GetBaseClass(characterClass.Number) is { } baseClass
           && EnhanceTreeFeaturePlugIn.GetConfiguration(player.GameContext) is { } configuration
            ? GetExtraSkillDamage(configuration, baseClass, (short)skill.Number, skills)
            : 0;

    /// <summary>
    /// Gets the damage which the learned nodes of a player add to a skill.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="skills">The skills of the player.</param>
    /// <returns>The added damage.</returns>
    public static float GetExtraSkillDamage(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, ISkillList skills)
        => GetExtraSkillDamage(configuration, baseClass, skill, skills, static (list, number) => list.GetSkill((ushort)number)?.Level);

    /// <summary>
    /// Gets the damage which the learned nodes add to a skill.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <returns>The added damage.</returns>
    public static float GetExtraSkillDamage(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, Func<short, int?> levelOf)
        => GetExtraSkillDamage(configuration, baseClass, skill, levelOf, static (lookup, number) => lookup(number));

    private static float GetExtraSkillDamage<TState>(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, TState state, Func<TState, short, int?> levelOf)
    {
        if (!Indexes.GetValue(configuration, CreateIndex).TryGetValue(((byte)baseClass, skill), out var sheets))
        {
            return 0;
        }

        var total = 0f;
        foreach (var (extension, damageNodes) in sheets)
        {
            if (levelOf(state, extension) is null)
            {
                continue;
            }

            foreach (var (node, formula) in damageNodes)
            {
                if (levelOf(state, node) is { } level)
                {
                    total += Values.GetOrAdd((formula, level), static key => (float)new Expression(key.Formula, new Argument("level", key.Level)).calculate());
                }
            }
        }

        return total;
    }

    private static Dictionary<(byte BaseClass, short Skill), (short Extension, (short Skill, string Formula)[] Damage)[]> CreateIndex(EnhanceTreeConfiguration configuration)
        => configuration.Nodes
            .Where(n => n.Index == ExtensionNodeIndex && n.RelatedSkill != 0)
            .GroupBy(n => (n.BaseClass, n.RelatedSkill))
            .ToDictionary(
                g => g.Key,
                g => g.Select(extension => (extension.Skill, configuration.Nodes
                        .Where(n => n.BaseClass == extension.BaseClass && n.Group == extension.Group && n.Skill == ExtSkillDamage && configuration.Formulas.ContainsKey(n.FormulaId))
                        .Select(n => (n.Skill, configuration.Formulas[n.FormulaId]))
                        .ToArray()))
                    .ToArray());
}
