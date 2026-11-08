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

    /// <summary>
    /// The skill number of "Add Ext Skill Extra Damage Rate": the chance in percent of a Mastery DMG hit.
    /// </summary>
    public const short ExtraDamageRate = 1007;

    /// <summary>
    /// The skill number of "Add Ext Skill Radio": tiles more of the skill's area.
    /// </summary>
    public const short SkillRadius = 1009;

    /// <summary>
    /// The skill number of "Add Ext Skill Range": tiles more of the skill's range.
    /// </summary>
    public const short SkillRange = 1011;

    /// <summary>
    /// The skill number of "Add Ext Skill Target Number": targets more of a skill which hits a limited number of them.
    /// </summary>
    public const short TargetNumber = 1012;

    /// <summary>
    /// The skill number of "Add Weapon Physi Damage": the weapon's physical damage rises by its percentage.
    /// </summary>
    public const short WeaponPhysicalDamage = 1020;

    /// <summary>
    /// The skill number of "Add Weapon Magic Damage": the staff rise rises by its percentage.
    /// </summary>
    public const short WeaponWizardryDamage = 1021;

    /// <summary>
    /// The skill number of "Add Weapon Curse Damage": the book rise rises by its percentage.
    /// </summary>
    public const short WeaponCurseDamage = 1156;

    private const byte ExtensionNodeIndex = 0;

    private const int BaseSplashRadius = 1;

    private static readonly short[] ExtSkillDamageNodes = [ExtSkillDamage];

    private static readonly short[] DamageCountNodes = [DamageCount1, DamageCount2, DamageCount3];

    private static readonly short[] SplashDamageNodes = [SplashDamage];

    private static readonly short[] SplashCountNodes = [SplashDamage, SplashCount];

    private static readonly short[] SplashRadiusNodes = [SplashRadius];

    private static readonly short[] ExtraDamageRateNodes = [ExtraDamageRate];

    private static readonly short[] RangeNodes = [SkillRange];

    private static readonly short[] RadiusNodes = [SkillRadius];

    private static readonly short[] TargetNodes = [TargetNumber];

    private static readonly short[] WeaponPhysicalNodes = [WeaponPhysicalDamage];

    private static readonly short[] WeaponWizardryNodes = [WeaponWizardryDamage];

    private static readonly short[] WeaponCurseNodes = [WeaponCurseDamage];

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
    /// Gets what the learned nodes of a player add to the shape of a skill, when the tree is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skill">The used skill.</param>
    /// <returns>The added range, radius and targets.</returns>
    public static EnhanceTreeReach GetReach(Player player, Skill skill)
        => player.SkillList is { } skills
           && player.SelectedCharacter?.CharacterClass is { } characterClass
           && ClassCodes.GetBaseClass(characterClass.Number) is { } baseClass
           && EnhanceTreeFeaturePlugIn.GetConfiguration(player.GameContext) is { } configuration
            ? GetReach(configuration, baseClass, (short)skill.Number, skills, LevelInList)
            : default;

    /// <summary>
    /// Gets what the learned nodes add to the shape of a skill.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <returns>The added range, radius and targets.</returns>
    public static EnhanceTreeReach GetReach(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, Func<short, int?> levelOf)
        => GetReach(configuration, baseClass, skill, levelOf, LevelOfLookup);

    /// <summary>
    /// Gets the percentage by which the learned weapon nodes of a player raise the weapon's damage of a damage type, when
    /// the tree is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skill">The used skill.</param>
    /// <returns>The percentage.</returns>
    public static float GetWeaponRise(Player player, Skill skill)
        => GetValue(player, skill, WeaponNodes(skill.DamageType));

    /// <summary>
    /// Gets the percentage by which the learned weapon nodes raise the weapon's damage of a damage type.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <param name="damageType">The damage type of the skill.</param>
    /// <returns>The percentage.</returns>
    public static float GetWeaponRise(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, Func<short, int?> levelOf, DamageType damageType)
        => GetValue(configuration, baseClass, skill, levelOf, LevelOfLookup, WeaponNodes(damageType));

    /// <summary>
    /// Gets the chance in percent of a Mastery DMG hit of a skill.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <returns>The chance in percent.</returns>
    public static float GetMasteryDamageChance(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, Func<short, int?> levelOf)
        => GetValue(configuration, baseClass, skill, levelOf, LevelOfLookup, ExtraDamageRateNodes);

    /// <summary>
    /// Gets the factor of the damage of a hit by its Mastery DMG.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="isMasteryHit">If set to <c>true</c>, the hit is a Mastery DMG hit.</param>
    /// <returns>The factor.</returns>
    public static double GetMasteryDamageFactor(EnhanceTreeConfiguration configuration, bool isMasteryHit)
        => isMasteryHit ? 1 + (configuration.MasteryDamagePercent / 100.0) : 1;

    /// <summary>
    /// Rolls the Mastery DMG of a skill hit of a player, when the tree is on and gives the Mastery DMG an amount.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skill">The used skill.</param>
    /// <returns>The factor of the hit's damage.</returns>
    public static double RollMasteryDamageFactor(Player player, Skill skill)
        => EnhanceTreeFeaturePlugIn.GetConfiguration(player.GameContext) is { MasteryDamagePercent: > 0 } configuration
            ? GetMasteryDamageFactor(configuration, Rand.NextRandomBool(GetValue(player, skill, ExtraDamageRateNodes) / 100.0))
            : 1;

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

    /// <summary>
    /// Gets the level of a learned tree skill of a skill list.
    /// </summary>
    /// <param name="list">The skill list.</param>
    /// <param name="number">The skill number.</param>
    /// <returns>The level, <c>null</c> when it isn't learned.</returns>
    internal static int? LevelInList(ISkillList list, short number) => list.GetSkill((ushort)number)?.Level;

    /// <summary>
    /// Gets the level of a learned tree skill by a lookup.
    /// </summary>
    /// <param name="lookup">The lookup.</param>
    /// <param name="number">The skill number.</param>
    /// <returns>The level, <c>null</c> when it isn't learned.</returns>
    internal static int? LevelOfLookup(Func<short, int?> lookup, short number) => lookup(number);

    /// <summary>
    /// Gets the summed values of nodes of the sheets whose extension improves a skill, for the learned extensions.
    /// </summary>
    /// <typeparam name="TState">The type of the state of <paramref name="levelOf"/>.</typeparam>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="state">The state of <paramref name="levelOf"/>.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <param name="nodes">The node skills to sum.</param>
    /// <returns>The value.</returns>
    internal static float GetValue<TState>(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, TState state, Func<TState, short, int?> levelOf, short[] nodes)
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

    private static float GetValue(Player player, Skill skill, short[] nodes)
        => player.SkillList is { } skills
           && player.SelectedCharacter?.CharacterClass is { } characterClass
           && ClassCodes.GetBaseClass(characterClass.Number) is { } baseClass
           && EnhanceTreeFeaturePlugIn.GetConfiguration(player.GameContext) is { } configuration
            ? GetValue(configuration, baseClass, (short)skill.Number, skills, LevelInList, nodes)
            : 0;

    private static short[] WeaponNodes(DamageType damageType) => damageType switch
    {
        DamageType.Physical => WeaponPhysicalNodes,
        DamageType.Wizardry => WeaponWizardryNodes,
        DamageType.Curse => WeaponCurseNodes,
        _ => [],
    };

    private static EnhanceTreeReach GetReach<TState>(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, TState state, Func<TState, short, int?> levelOf)
        => new(
            (int)GetValue(configuration, baseClass, skill, state, levelOf, RangeNodes),
            (int)GetValue(configuration, baseClass, skill, state, levelOf, RadiusNodes),
            (int)GetValue(configuration, baseClass, skill, state, levelOf, TargetNodes));

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
