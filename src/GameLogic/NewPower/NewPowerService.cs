// <copyright file="NewPowerService.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.NewPower;

using System.Runtime.CompilerServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Views.World;

/// <summary>
/// The New Power strikes (Readme.md): after a hit of a fourth class or card skill, the active kind strikes its targets.
/// </summary>
public static class NewPowerService
{
    private const float ExcellentMultiplier = 1.2f;

    private static readonly ConditionalWeakTable<Player, TriggerState> States = new();

    private static readonly ConditionalWeakTable<EnhanceTreeConfiguration, HashSet<short>> TreeSkills = new();

    /// <summary>
    /// Strikes after a skill hit of the player, when the New Power is on, a kind is active and the skill triggers it.
    /// </summary>
    /// <param name="attacker">The attacking player.</param>
    /// <param name="target">The target which was hit.</param>
    /// <param name="skillEntry">The skill of the hit, <c>null</c> for a normal hit.</param>
    /// <param name="hit">The hit.</param>
    /// <returns>The task.</returns>
    public static async ValueTask ApplyAfterHitAsync(Player attacker, IAttackable target, SkillEntry? skillEntry, HitInfo hit)
    {
        if (skillEntry?.Skill is not { } skill
            || hit.TotalDamage == 0
            || NewPowerFeaturePlugIn.Get(attacker.GameContext) is not { } plugIn
            || attacker.Attributes is not { } attributes
            || attacker.CurrentMap is not { } map)
        {
            return;
        }

        var configuration = plugIn.GetConfiguration();
        if (GetActiveKind(configuration, s => attacker.SkillList?.ContainsSkill((ushort)s) == true, e => attacker.MagicEffectList.ContainsEffect(e)) is not { } kind
            || !IsTriggerSkill(EnhanceTreeFeaturePlugIn.GetConfiguration(attacker.GameContext), skill.Number, AbilityCoreService.IsCardSkill(attacker, skill))
            || !States.GetValue(attacker, _ => new TriggerState()).TryTrigger(DateTime.UtcNow, TimeSpan.FromMilliseconds(configuration.CooldownMilliseconds)))
        {
            return;
        }

        var around = kind.Targets > 1 ? map.GetAttackablesInRange(target.Position, kind.Range) : [];
        foreach (var struck in SelectTargets(target, around, kind))
        {
            var (damage, _) = RollDamage(attributes, kind, Rand.NextRandomBool);
            if (damage == 0)
            {
                return;
            }

            await struck.ApplyBleedingDamageAsync(attacker, damage).ConfigureAwait(false);
            await attacker.ForEachWorldObserverAsync<IShowSkillAnimationPlugIn>(p => p.ShowSkillAnimationAsync(attacker, struck, kind.Skill, true), true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gives the power of the kind of a magic effect for a while (the trial of a fifth class quest), when the New Power is on.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="effect">The number of the effect.</param>
    /// <param name="duration">The duration.</param>
    /// <returns>The task.</returns>
    public static async ValueTask GrantAsync(Player player, short effect, TimeSpan duration)
    {
        if (NewPowerFeaturePlugIn.Get(player.GameContext)?.GetConfiguration() is not { } configuration
            || configuration.Kinds.FirstOrDefault(k => k.Effect == effect) is not { } kind
            || player.GameContext.Configuration.MagicEffects.FirstOrDefault(e => e.Number == effect) is not { } definition)
        {
            return;
        }

        await RemoveAsync(player, effect).ConfigureAwait(false);
        var power = new MagicEffect.ElementWithTarget(new ConstantElement(configuration.TrialExtraDamage), NewPowerStats.Of(kind.Kind).ExtraDamage);
        await player.MagicEffectList.AddEffectAsync(new MagicEffect(duration, definition, power)).ConfigureAwait(false);
    }

    /// <summary>
    /// Takes away a magic effect of the New Power.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="effect">The number of the effect.</param>
    /// <returns>The task.</returns>
    public static async ValueTask RemoveAsync(Player player, short effect)
    {
        if (player.MagicEffectList.TryGetEffect(effect, out var active))
        {
            await active.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the active kind: the one of the extra damage card's skill, else of an active effect.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="hasSkill">Whether the player has a skill.</param>
    /// <param name="hasEffect">Whether the player has an active effect.</param>
    /// <returns>The kind, or <c>null</c>.</returns>
    public static NewPowerKindSettings? GetActiveKind(NewPowerConfiguration configuration, Func<short, bool> hasSkill, Func<short, bool> hasEffect)
        => configuration.Kinds.FirstOrDefault(k => hasSkill(k.Skill)) ?? configuration.Kinds.FirstOrDefault(k => hasEffect(k.Effect));

    /// <summary>
    /// Determines whether a skill triggers the New Power: a skill of the skill enhancement tree (a fourth class skill) or
    /// a card skill (Webzen guide 340).
    /// </summary>
    /// <param name="tree">The configuration of the tree, <c>null</c> when it's off.</param>
    /// <param name="skill">The skill number.</param>
    /// <param name="isCardSkill">If set to <c>true</c>, the skill is the skill of an active skill card.</param>
    /// <returns><c>true</c>, if it triggers.</returns>
    public static bool IsTriggerSkill(EnhanceTreeConfiguration? tree, short skill, bool isCardSkill)
        => isCardSkill || (tree is not null && TreeSkills.GetValue(tree, t => t.Nodes.Select(n => n.Skill).ToHashSet()).Contains(skill));

    /// <summary>
    /// Selects the targets of a strike: the hit target, then the nearest living monsters within the kind's range.
    /// </summary>
    /// <param name="first">The hit target.</param>
    /// <param name="around">The attackables around it.</param>
    /// <param name="kind">The kind.</param>
    /// <returns>The targets.</returns>
    public static IReadOnlyList<IAttackable> SelectTargets(IAttackable first, IEnumerable<IAttackable> around, NewPowerKindSettings kind)
    {
        var targets = new List<IAttackable> { first };
        targets.AddRange(around
            .Where(a => !ReferenceEquals(a, first) && a.IsAlive && a is not Player && (a as Monster)?.SummonedBy is null && first.Position.EuclideanDistanceTo(a.Position) <= kind.Range)
            .OrderBy(a => first.Position.EuclideanDistanceTo(a.Position))
            .Take(kind.Targets - 1));
        return targets;
    }

    /// <summary>
    /// Rolls the damage of a strike: the extra damage at the kind's rate plus the attack increase; excellent (x1.2 and
    /// the excellent bonus) or critical (the critical bonus); then triple or double.
    /// </summary>
    /// <param name="attributes">The attributes of the attacker.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="roll">Rolls a chance (0..1).</param>
    /// <returns>The damage and its kind of hit.</returns>
    public static (uint Damage, DamageAttributes Attributes) RollDamage(IAttributeSystem attributes, NewPowerKindSettings kind, Func<double, bool> roll)
    {
        var stats = NewPowerStats.Of(kind.Kind);
        var damage = (attributes[stats.ExtraDamage] * kind.DamageRate) + attributes[stats.AttackIncrease];
        if (damage <= 0)
        {
            return (0, DamageAttributes.Undefined);
        }

        var flags = DamageAttributes.Undefined;
        if (roll(attributes[stats.ExcellentChance]))
        {
            damage = (damage * ExcellentMultiplier) + attributes[Stats.ExcellentDamageBonus];
            flags = DamageAttributes.Excellent;
        }
        else if (roll(attributes[stats.CriticalChance]))
        {
            damage += attributes[Stats.CriticalDamageBonus];
            flags = DamageAttributes.Critical;
        }

        if (roll(attributes[stats.TripleChance]))
        {
            damage *= 3;
            flags |= DamageAttributes.Triple;
        }
        else if (roll(attributes[stats.DoubleChance]))
        {
            damage *= 2;
            flags |= DamageAttributes.Double;
        }

        return ((uint)Math.Round(damage), flags);
    }

    /// <summary>
    /// The time a player's next strike may come.
    /// </summary>
    public sealed class TriggerState
    {
        private readonly object _lock = new();

        private DateTime _next;

        /// <summary>
        /// Strikes when the cooldown of the last strike passed.
        /// </summary>
        /// <param name="now">The current time.</param>
        /// <param name="cooldown">The cooldown.</param>
        /// <returns><c>true</c>, if it strikes.</returns>
        public bool TryTrigger(DateTime now, TimeSpan cooldown)
        {
            lock (this._lock)
            {
                if (now < this._next)
                {
                    return false;
                }

                this._next = now + cooldown;
                return true;
            }
        }
    }
}
