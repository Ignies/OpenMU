// <copyright file="EnhanceTreeExplosions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.NPC;
using org.mariuszgromada.math.mxparser;

/// <summary>
/// The explosion skills of the skill enhancement tree (Poison Storm 1125, Frozen Slayer 1126, Bloodying Hit 1127): a hit
/// on a target with the upgraded debuff of the skill's kind removes it and explodes; without it, the skill does nothing.
/// See Readme.md, "Explosions".
/// </summary>
public static class EnhanceTreeExplosions
{
    /// <summary>
    /// The damage nodes of the explosion lines ("Enhanced (1)" 1129-1131, "Enhanced (2)" 1196-1198): they add to the
    /// skill damage of their explosion like "Add Ext Skill Damage".
    /// </summary>
    internal static readonly short[] DamageNodes = [1129, 1130, 1131, 1196, 1197, 1198];

    /// <summary>
    /// The range nodes of the explosion lines (1199-1201, "Range Increase", +1 at 10 and 20).
    /// </summary>
    internal static readonly short[] RangeNodes = [1199, 1200, 1201];

    /// <summary>
    /// The effect nodes of the explosion lines (1133-1135): they add to <see cref="EnhanceTreeExplosion.Value"/>.
    /// </summary>
    private static readonly short[] EffectNodes = [1133, 1134, 1135];

    /// <summary>
    /// Gets the explosions of the tree (muonlinefanz.com tree tool, sheet "Debuff"): Poison Storm poisons up to 2 targets
    /// around, Frozen Slayer hits them with 30 % of the skill damage, Bloodying Hit decreases the target's defense by 15 %.
    /// No source has the radius around the target (2 tiles) nor the seconds of the defense decrease (10, the seconds of a
    /// debuff circle); the effect number of the decrease is the "Dec Def" debuff of S21Emu <c>Effect.txt</c> (313).
    /// </summary>
    /// <returns>The default explosions.</returns>
    public static List<EnhanceTreeExplosion> CreateDefaultExplosions() =>
    [
        new() { Skill = 1125, Kind = EnhanceTreeDebuffKind.Poison, Radius = 2, Value = 2 },
        new() { Skill = 1126, Kind = EnhanceTreeDebuffKind.Chill, Radius = 2, Value = 30 },
        new() { Skill = 1127, Kind = EnhanceTreeDebuffKind.Bleed, Value = 15, Seconds = 10, EffectNumber = 313 },
    ];

    /// <summary>
    /// Gets the damage of an explosion skill of a player, when the tree is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="skill">The used skill.</param>
    /// <returns>The damage, <c>null</c> when the skill isn't a learned explosion.</returns>
    public static float? GetDamage(Player player, Skill skill)
        => EnhanceTreeEffects.TryGetTree(player, out var configuration, out var baseClass, out var skills)
           && player.Attributes is { } attributes
            ? GetDamage(configuration, baseClass, (short)skill.Number, skills, EnhanceTreeEffects.LevelInList, (int)(attributes[Stats.Level] + attributes[Stats.MasterLevel]))
            : null;

    /// <summary>
    /// Gets the damage of an explosion skill: the formula of its node at the character's total level (S21Emu formulas
    /// 139-141: 25000 at 800, 25 more each level).
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the character.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="levelOf">The level of a learned tree skill, <c>null</c> when it isn't learned.</param>
    /// <param name="totalLevel">The level plus the master level of the character.</param>
    /// <returns>The damage, <c>null</c> when the skill isn't a learned explosion.</returns>
    public static float? GetDamage(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, Func<short, int?> levelOf, int totalLevel)
        => GetDamage(configuration, baseClass, skill, levelOf, EnhanceTreeEffects.LevelOfLookup, totalLevel);

    /// <summary>
    /// Explodes the upgraded debuff of an explosion skill of a player on its target, when the tree is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="target">The target.</param>
    /// <param name="skillEntry">The used skill.</param>
    /// <returns><c>true</c>, when the skill is an explosion (it exploded, or there was nothing to explode).</returns>
    public static ValueTask<bool> TryExplodeAsync(Player player, IAttackable target, SkillEntry skillEntry)
    {
        if (skillEntry.Skill is not { } skill
            || !EnhanceTreeEffects.TryGetTree(player, out var configuration, out var baseClass, out var skills)
            || FindExplosion(configuration, (short)skill.Number) is null)
        {
            return ValueTask.FromResult(false);
        }

        return TryExplodeAsync(
            configuration,
            baseClass,
            number => EnhanceTreeEffects.LevelInList(skills, number),
            player,
            target,
            (short)skill.Number,
            async (hitTarget, factor) => await hitTarget.AttackByAsync(player, skillEntry, false, factor).ConfigureAwait(false));
    }

    /// <summary>
    /// Explodes the upgraded debuff of an explosion skill's kind on its target.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the attacker.</param>
    /// <param name="levelOf">The level of a learned tree skill of the attacker, <c>null</c> when it isn't learned.</param>
    /// <param name="attacker">The attacker.</param>
    /// <param name="target">The target.</param>
    /// <param name="skill">The number of the used skill.</param>
    /// <param name="hitAsync">Hits a target with the skill, with a factor of its damage.</param>
    /// <returns><c>true</c>, when the skill is an explosion (it exploded, or there was nothing to explode).</returns>
    public static async ValueTask<bool> TryExplodeAsync(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, Func<short, int?> levelOf, IAttacker attacker, IAttackable target, short skill, Func<IAttackable, double, ValueTask> hitAsync)
    {
        if (FindExplosion(configuration, skill) is not { } explosion)
        {
            return false;
        }

        if (EnhanceTreeDebuffs.FindDebuff(configuration, explosion.Kind, true) is not { } improved
            || !target.MagicEffectList.TryGetEffect(improved.EffectNumber, out var debuff))
        {
            return true;
        }

        await debuff.DisposeAsync().ConfigureAwait(false);
        await hitAsync(target, 1).ConfigureAwait(false);
        var value = explosion.Value + EnhanceTreeEffects.GetValue(configuration, baseClass, skill, levelOf, EnhanceTreeEffects.LevelOfLookup, EffectNodes);
        switch (explosion.Kind)
        {
            case EnhanceTreeDebuffKind.Poison:
                await SpreadPoisonAsync(configuration, explosion, (int)value, attacker, target).ConfigureAwait(false);
                break;
            case EnhanceTreeDebuffKind.Chill:
                foreach (var around in GetTargetsAround(explosion, attacker, target))
                {
                    await hitAsync(around, value / 100.0).ConfigureAwait(false);
                }

                break;
            default:
                var decrease = new ConstantElement(Math.Max(0, 1 - (value / 100f)), AggregateType.Multiplicate);
                var effect = new MagicEffect(TimeSpan.FromSeconds(explosion.Seconds), EnhanceTreeDebuffs.GetEffectDefinition(explosion.EffectNumber), new MagicEffect.ElementWithTarget(decrease, Stats.DefenseDecrement));
                await target.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
                break;
        }

        return true;
    }

    private static float? GetDamage<TState>(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, short skill, TState state, Func<TState, short, int?> levelOf, int totalLevel)
    {
        if (levelOf(state, skill) is null
            || configuration.FindNode(baseClass, skill) is not { } node
            || node.RelatedSkill != skill
            || !configuration.Formulas.TryGetValue(node.FormulaId, out var formula))
        {
            return null;
        }

        return (float)new Expression(formula, new Argument("level", totalLevel)).calculate();
    }

    private static async ValueTask SpreadPoisonAsync(EnhanceTreeConfiguration configuration, EnhanceTreeExplosion explosion, int targets, IAttacker attacker, IAttackable target)
    {
        if (EnhanceTreeDebuffs.FindDebuff(configuration, EnhanceTreeDebuffKind.Poison, false) is not { } basic
            || EnhanceTreeDebuffs.FindDebuff(configuration, EnhanceTreeDebuffKind.Poison, true) is not { } improved)
        {
            return;
        }

        // A poisoned target around is upgraded first, as the upgrade is what the explosion adds to it.
        var candidates = GetTargetsAround(explosion, attacker, target)
            .Where(a => !a.MagicEffectList.ContainsEffect(improved.EffectNumber))
            .OrderByDescending(a => a.MagicEffectList.ContainsEffect(basic.EffectNumber))
            .ThenBy(a => a.GetDistanceTo(target))
            .Take(targets)
            .ToList();
        foreach (var candidate in candidates)
        {
            await EnhanceTreeDebuffs.PlaceAsync(configuration, EnhanceTreeDebuffKind.Poison, candidate.MagicEffectList.ContainsEffect(basic.EffectNumber), attacker, candidate).ConfigureAwait(false);
        }
    }

    private static IEnumerable<IAttackable> GetTargetsAround(EnhanceTreeExplosion explosion, IAttacker attacker, IAttackable target)
        => target.CurrentMap?.GetAttackablesInRange(target.Position, explosion.Radius)
               .Where(a => a != target && a != attacker && a.IsAlive && !a.IsAtSafezone())
               .Where(a => target is Monster ? a is Monster { SummonedBy: null } : a is Player)
               .ToList()
           ?? [];

    private static EnhanceTreeExplosion? FindExplosion(EnhanceTreeConfiguration configuration, short skill)
    {
        foreach (var explosion in configuration.Explosions)
        {
            if (explosion.Skill == skill)
            {
                return explosion;
            }
        }

        return null;
    }
}
