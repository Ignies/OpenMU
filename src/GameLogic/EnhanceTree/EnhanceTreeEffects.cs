// <copyright file="EnhanceTreeEffects.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MUnique.OpenMU.GameLogic.Classes;
using org.mariuszgromada.math.mxparser;

/// <summary>
/// The node effects of the skill enhancement tree which OpenMU carries: the skill damage of "Add Ext Skill Damage", the
/// hits of the damage count nodes and the splash hits. A node counts for the skill which the extension node of its sheet
/// improves, while that extension is learned.
/// </summary>
public static class EnhanceTreeEffects
{
    /// <summary>
    /// The skill number of the node "Add Ext Skill Damage": its value adds to the damage of the skill which the extension
    /// node of its sheet improves.
    /// </summary>
    public const short ExtSkillDamage = 1001;

    /// <summary>
    /// The skill number of "Add Ext Skill Damage Count 1": one more hit.
    /// </summary>
    public const short DamageCount1 = 1002;

    /// <summary>
    /// The skill number of "Add Ext Skill Damage Count 2": two more hits.
    /// </summary>
    public const short DamageCount2 = 1003;

    /// <summary>
    /// The skill number of "Add Ext Skill Damage Count 3": three more hits.
    /// </summary>
    public const short DamageCount3 = 1004;

    /// <summary>
    /// The skill number of "Add Ext Skill Splash Damage": one hit on the monsters within a tile of the target.
    /// </summary>
    public const short SplashDamage = 1010;

    /// <summary>
    /// The skill number of "Add Ext Skill Splash Radio": a tile more for the splash.
    /// </summary>
    public const short SplashRadius = 1153;

    /// <summary>
    /// The skill number of "Add Ext Skill Splash Count": one splash hit more.
    /// </summary>
    public const short SplashCount = 1154;

    private const byte ExtensionNodeIndex = 0;

    private const int BaseSplashRadius = 1;

    private static readonly short[] ExtSkillDamageNodes = [ExtSkillDamage];

    private static readonly short[] DamageCountNodes = [DamageCount1, DamageCount2, DamageCount3];

    private static readonly short[] SplashDamageNodes = [SplashDamage];

    private static readonly short[] SplashCountNodes = [SplashDamage, SplashCount];

    private static readonly short[] SplashRadiusNodes = [SplashRadius];

    private static readonly ConcurrentDictionary<(string Formula, int Level), float> Values = new();

    private static readonly ConditionalWeakTable<EnhanceTreeConfiguration, Dictionary<(byte BaseClass, short Skill), Sheet[]>> Indexes = new();

    /// <summary>
    /// Gets the damage which the learned nodes of a player add to a skill, when the tree is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skill">The used skill.</param>
    /// <returns>The added damage.</returns>
    public static float GetExtraSkillDamage(Player player, Skill skill)
        => GetValue(player, skill, ExtSkillDamageNodes);

    /// <summary>
    /// Gets the damage which the learned nodes of a player add to a skill.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="skills">The skills of the player.</param>
    /// <returns>The added damage.</returns>
    public static float GetExtraSkillDamage(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, ISkillList skills)
        => GetValue(configuration, baseClass, skill, skills, LevelInList, ExtSkillDamageNodes);

    /// <summary>
    /// Gets the damage which the learned nodes add to a skill.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <returns>The added damage.</returns>
    public static float GetExtraSkillDamage(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, Func<short, int?> levelOf)
        => GetValue(configuration, baseClass, skill, levelOf, LevelOfLookup, ExtSkillDamageNodes);

    /// <summary>
    /// Gets the hits which the damage count nodes of a player add to a skill, when the tree is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skill">The used skill.</param>
    /// <returns>The added hits.</returns>
    public static int GetExtraHits(Player player, Skill skill)
        => (int)GetValue(player, skill, DamageCountNodes);

    /// <summary>
    /// Gets the hits which the damage count nodes add to a skill.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <returns>The added hits.</returns>
    public static int GetExtraHits(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, Func<short, int?> levelOf)
        => (int)GetValue(configuration, baseClass, skill, levelOf, LevelOfLookup, DamageCountNodes);

    /// <summary>
    /// Gets the splash of a skill by the learned nodes of a player, when the tree is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skill">The used skill.</param>
    /// <returns>The hits on each monster around the target and the radius in tiles; no hits without the splash node.</returns>
    public static (int Hits, int Radius) GetSplash(Player player, Skill skill)
        => GetValue(player, skill, SplashDamageNodes) <= 0
            ? default
            : ((int)GetValue(player, skill, SplashCountNodes), BaseSplashRadius + (int)GetValue(player, skill, SplashRadiusNodes));

    /// <summary>
    /// Gets the splash of a skill by the learned nodes.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <returns>The hits on each monster around the target and the radius in tiles; no hits without the splash node.</returns>
    public static (int Hits, int Radius) GetSplash(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, Func<short, int?> levelOf)
        => GetValue(configuration, baseClass, skill, levelOf, LevelOfLookup, SplashDamageNodes) <= 0
            ? default
            : ((int)GetValue(configuration, baseClass, skill, levelOf, LevelOfLookup, SplashCountNodes),
                BaseSplashRadius + (int)GetValue(configuration, baseClass, skill, levelOf, LevelOfLookup, SplashRadiusNodes));

    /// <summary>
    /// Hits the monsters around the target of a skill with the splash of the player's tree.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="target">The target of the skill.</param>
    /// <param name="skillEntry">The skill entry.</param>
    /// <returns>The task.</returns>
    public static async ValueTask ApplySplashAsync(Player player, IAttackable target, SkillEntry skillEntry)
    {
        if (skillEntry.Skill is not { } skill
            || GetSplash(player, skill) is not { Hits: > 0 } splash
            || target.CurrentMap is not { } map)
        {
            return;
        }

        foreach (var monster in map.GetAttackablesInRange(target.Position, splash.Radius).OfType<NPC.Monster>())
        {
            if (monster == target || !monster.IsAlive || monster.IsAtSafezone())
            {
                continue;
            }

            for (var hit = 0; hit < splash.Hits && monster.IsAlive; hit++)
            {
                await monster.AttackByAsync(player, skillEntry, false).ConfigureAwait(false);
            }
        }
    }

    private static float GetValue(Player player, Skill skill, short[] nodes)
        => player.SkillList is { } skills
           && player.SelectedCharacter?.CharacterClass is { } characterClass
           && ClassCodes.GetBaseClass(characterClass.Number) is { } baseClass
           && EnhanceTreeFeaturePlugIn.GetConfiguration(player.GameContext) is { } configuration
            ? GetValue(configuration, baseClass, (short)skill.Number, skills, LevelInList, nodes)
            : 0;

    private static int? LevelInList(ISkillList list, short number) => list.GetSkill((ushort)number)?.Level;

    private static int? LevelOfLookup(Func<short, int?> lookup, short number) => lookup(number);

    private static float GetValue<TState>(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, TState state, Func<TState, short, int?> levelOf, short[] nodes)
    {
        if (!Indexes.GetValue(configuration, CreateIndex).TryGetValue(((byte)baseClass, skill), out var sheets))
        {
            return 0;
        }

        var total = 0f;
        foreach (var sheet in sheets)
        {
            if (levelOf(state, sheet.Extension) is null)
            {
                continue;
            }

            foreach (var (node, formula) in sheet.Nodes)
            {
                if (Array.IndexOf(nodes, node) >= 0 && levelOf(state, node) is { } level)
                {
                    total += Values.GetOrAdd((formula, level), static key => (float)new Expression(key.Formula, new Argument("level", key.Level)).calculate());
                }
            }
        }

        return total;
    }

    private static Dictionary<(byte BaseClass, short Skill), Sheet[]> CreateIndex(EnhanceTreeConfiguration configuration)
        => configuration.Nodes
            .Where(n => n.Index == ExtensionNodeIndex && n.RelatedSkill != 0)
            .GroupBy(n => (n.BaseClass, n.RelatedSkill))
            .ToDictionary(
                g => g.Key,
                g => g.Select(extension => new Sheet(extension.Skill, configuration.Nodes
                        .Where(n => n.BaseClass == extension.BaseClass && n.Group == extension.Group && n.Index != ExtensionNodeIndex && configuration.Formulas.ContainsKey(n.FormulaId))
                        .Select(n => (n.Skill, configuration.Formulas[n.FormulaId]))
                        .ToArray()))
                    .ToArray());

    private sealed record Sheet(short Extension, (short Skill, string Formula)[] Nodes);
}
