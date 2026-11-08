// <copyright file="EnhanceTreeDebuffs.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// The debuff circles of the skill enhancement tree (S21Emu <c>EnhanceSkillTreeMagicField.xml</c> and the debuff nodes of
/// <c>EnhanceSkillTree.xml</c>). A skill hit casts the circle of a kind by the rate nodes of the skill's sheet; while it
/// lasts, the attacker's skill hits on targets inside it place the basic debuff and, with the upgrade node, upgrade it.
/// See Readme.md, "Debuffs".
/// </summary>
public static class EnhanceTreeDebuffs
{
    private static readonly KindNodes[] Nodes =
    [
        new(Rate: [1024, 1046], Value: [1028], Duration: [1032], ImprovedValue: [1036, 1058], Upgrade: [1040], Targets: [1166, 1190], ImprovedTargets: [1167], Range: [1172], Chance: [1178, 1184], ImprovedChance: [1179, 1185]),
        new(Rate: [1025, 1047], Value: [1029], Duration: [1033], ImprovedValue: [1037, 1059], Upgrade: [1041], Targets: [1170, 1192], ImprovedTargets: [1171], Range: [1176], Chance: [1182, 1188], ImprovedChance: [1183, 1189]),
        new(Rate: [1026, 1048], Value: [1030], Duration: [1034], ImprovedValue: [1038, 1060], Upgrade: [1042], Targets: [1168, 1191], ImprovedTargets: [1169], Range: [1174], Chance: [1180, 1186], ImprovedChance: [1181, 1187]),
    ];

    private static readonly ConditionalWeakTable<IAttacker, Circle[]> Circles = new();

    private static readonly ConcurrentDictionary<short, MagicEffectDefinition> Definitions = new();

    /// <summary>
    /// Gets the debuffs of S21Emu <c>EnhanceSkillTreeMagicField.xml</c> (MagicFieldBuffInfo).
    /// </summary>
    /// <returns>The default debuffs.</returns>
    public static List<EnhanceTreeDebuff> CreateDefaultDebuffs() =>
    [
        new() { Kind = EnhanceTreeDebuffKind.Poison, EffectNumber = 273, Seconds = 18, Count = 10, SuccessRate = 200, Value = 2000, ValueRange = 1000 },
        new() { Kind = EnhanceTreeDebuffKind.Poison, Improved = true, EffectNumber = 277, Seconds = 18, Count = 10, SuccessRate = 100, Multiplier = 2 },
        new() { Kind = EnhanceTreeDebuffKind.Chill, EffectNumber = 274, Seconds = 20, Count = 10, SuccessRate = 200, Value = 106 },
        new() { Kind = EnhanceTreeDebuffKind.Chill, Improved = true, EffectNumber = 278, Seconds = 20, Count = 10, SuccessRate = 100, Value = 106, AttackSpeed = 20 },
        new() { Kind = EnhanceTreeDebuffKind.Bleed, EffectNumber = 275, Seconds = 18, Count = 10, SuccessRate = 200, Value = 20 },
        new() { Kind = EnhanceTreeDebuffKind.Bleed, Improved = true, EffectNumber = 279, Seconds = 18, Count = 10, SuccessRate = 100, Multiplier = 2 },
    ];

    /// <summary>
    /// Applies the debuffs of the player's tree after a skill hit, and the bleeding the target carries, when the tree is on.
    /// </summary>
    /// <param name="attacker">The attacking player.</param>
    /// <param name="target">The target which was hit.</param>
    /// <param name="skillEntry">The skill of the hit, <c>null</c> for a normal hit.</param>
    /// <param name="hit">The hit.</param>
    /// <returns>The task.</returns>
    public static ValueTask ApplyAfterHitAsync(Player attacker, IAttackable target, SkillEntry? skillEntry, HitInfo hit)
    {
        if (skillEntry?.Skill is not { } skill || hit.TotalDamage == 0 || EnhanceTreeFeaturePlugIn.GetConfiguration(attacker.GameContext) is not { } configuration)
        {
            return ValueTask.CompletedTask;
        }

        return attacker.SkillList is { } skills
               && attacker.SelectedCharacter?.CharacterClass is { } characterClass
               && ClassCodes.GetBaseClass(characterClass.Number) is { } baseClass
            ? ApplyAfterHitAsync(configuration, baseClass, skills, EnhanceTreeEffects.LevelInList, attacker, target, (short)skill.Number, hit, DateTime.UtcNow)
            : BleedAsync(configuration, target, hit);
    }

    /// <summary>
    /// Applies the debuffs of a tree after a skill hit, and the bleeding the target carries.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="baseClass">The base class of the attacker.</param>
    /// <param name="levelOf">The level of a learned tree skill of the attacker, <c>null</c> when it isn't learned.</param>
    /// <param name="attacker">The attacker.</param>
    /// <param name="target">The target which was hit.</param>
    /// <param name="skill">The number of the skill of the hit.</param>
    /// <param name="hit">The hit.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The task.</returns>
    public static ValueTask ApplyAfterHitAsync(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, Func<short, int?> levelOf, IAttacker attacker, IAttackable target, short skill, HitInfo hit, DateTime now)
        => ApplyAfterHitAsync(configuration, baseClass, levelOf, EnhanceTreeEffects.LevelOfLookup, attacker, target, skill, hit, now);

    /// <summary>
    /// Places a debuff without a roll and without node values, replacing the basic debuff of its kind.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="improved">If set to <c>true</c>, the upgraded debuff of the kind.</param>
    /// <param name="attacker">The attacker.</param>
    /// <param name="target">The target.</param>
    /// <returns>The task.</returns>
    public static async ValueTask PlaceAsync(EnhanceTreeConfiguration configuration, EnhanceTreeDebuffKind kind, bool improved, IAttacker attacker, IAttackable target)
    {
        if (FindDebuff(configuration, kind, false) is not { } basic
            || (improved ? FindDebuff(configuration, kind, true) : basic) is not { } debuff)
        {
            return;
        }

        if (target.MagicEffectList.TryGetEffect(basic.EffectNumber, out var basicEffect))
        {
            await basicEffect.DisposeAsync().ConfigureAwait(false);
        }

        var noNodes = new SheetValues<int>(configuration, default, 0, 0, static (_, _) => null);
        await target.MagicEffectList.AddEffectAsync(CreateEffect(noNodes, Nodes[(int)kind], basic, debuff, attacker, target)).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the definition of a magic effect of the tree, which only its number identifies.
    /// </summary>
    /// <param name="number">The number of the effect.</param>
    /// <returns>The definition.</returns>
    internal static MagicEffectDefinition GetEffectDefinition(short number)
        => Definitions.GetOrAdd(number, static n => new MagicEffectDefinition { Number = n, StopByDeath = true });

    /// <summary>
    /// Gets the debuff of a kind.
    /// </summary>
    /// <param name="configuration">The configuration of the tree.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="improved">If set to <c>true</c>, the upgraded debuff of the kind.</param>
    /// <returns>The debuff, or <c>null</c>.</returns>
    internal static EnhanceTreeDebuff? FindDebuff(EnhanceTreeConfiguration configuration, EnhanceTreeDebuffKind kind, bool improved)
    {
        foreach (var debuff in configuration.Debuffs)
        {
            if (debuff.Kind == kind && debuff.Improved == improved)
            {
                return debuff;
            }
        }

        return null;
    }

    private static async ValueTask ApplyAfterHitAsync<TState>(EnhanceTreeConfiguration configuration, BaseClassCode baseClass, TState state, Func<TState, short, int?> levelOf, IAttacker attacker, IAttackable target, short skill, HitInfo hit, DateTime now)
    {
        if (hit.TotalDamage == 0)
        {
            return;
        }

        await BleedAsync(configuration, target, hit).ConfigureAwait(false);
        var sheet = new SheetValues<TState>(configuration, baseClass, skill, state, levelOf);
        for (var kind = 0; kind < Nodes.Length && target.IsAlive; kind++)
        {
            await TryDebuffAsync(configuration, sheet, (EnhanceTreeDebuffKind)kind, attacker, target, now).ConfigureAwait(false);
        }
    }

    private static async ValueTask BleedAsync(EnhanceTreeConfiguration configuration, IAttackable target, HitInfo hit)
    {
        foreach (var debuff in configuration.Debuffs)
        {
            if (debuff.Kind == EnhanceTreeDebuffKind.Bleed
                && target.MagicEffectList.TryGetEffect(debuff.EffectNumber, out var effect)
                && effect is EnhanceTreeDebuffEffect bleeding)
            {
                await bleeding.BleedAsync(hit.TotalDamage).ConfigureAwait(false);
            }
        }
    }

    private static async ValueTask TryDebuffAsync<TState>(EnhanceTreeConfiguration configuration, SheetValues<TState> sheet, EnhanceTreeDebuffKind kind, IAttacker attacker, IAttackable target, DateTime now)
    {
        var nodes = Nodes[(int)kind];
        var rate = sheet.Get(nodes.Rate);
        if (rate <= 0
            || FindField(configuration, kind) is not { } field
            || FindDebuff(configuration, kind, false) is not { } basic
            || target.CurrentMap is not { } map)
        {
            return;
        }

        var improved = FindDebuff(configuration, kind, true);
        var circle = Circles.GetValue(attacker, static _ => [new(), new(), new()])[(int)kind];
        var hasImproved = improved is not null && target.MagicEffectList.ContainsEffect(improved.EffectNumber);
        target.MagicEffectList.TryGetEffect(basic.EffectNumber, out var basicEffect);
        EnhanceTreeDebuff? place = null;
        lock (circle)
        {
            if (now >= circle.Until)
            {
                if (now < circle.CooldownUntil || !Rand.NextRandomBool(rate / 100.0))
                {
                    return;
                }

                circle.Cast(map, target.Position, now.AddSeconds(field.Seconds), now.AddSeconds(field.CooldownSeconds));
            }

            if (hasImproved || circle.Map != map || target.GetDistanceTo(circle.Center) > configuration.DebuffCircleRange + sheet.Get(nodes.Range))
            {
                return;
            }

            if (basicEffect is not null)
            {
                if (improved is not null
                    && sheet.Get(nodes.Upgrade) > 0
                    && circle.Upgraded < configuration.DebuffCircleTargets + sheet.Get(nodes.ImprovedTargets)
                    && Roll(configuration, improved.SuccessRate + sheet.Get(nodes.ImprovedChance)))
                {
                    circle.Upgraded++;
                    place = improved;
                }
            }
            else if (circle.Debuffed < configuration.DebuffCircleTargets + sheet.Get(nodes.Targets)
                     && Roll(configuration, basic.SuccessRate + sheet.Get(nodes.Chance)))
            {
                circle.Debuffed++;
                place = basic;
            }
        }

        if (place is null)
        {
            return;
        }

        if (basicEffect is not null)
        {
            await basicEffect.DisposeAsync().ConfigureAwait(false);
        }

        await target.MagicEffectList.AddEffectAsync(CreateEffect(sheet, nodes, basic, place, attacker, target)).ConfigureAwait(false);
    }

    private static EnhanceTreeDebuffEffect CreateEffect<TState>(SheetValues<TState> sheet, KindNodes nodes, EnhanceTreeDebuff basic, EnhanceTreeDebuff debuff, IAttacker attacker, IAttackable target)
    {
        var duration = TimeSpan.FromSeconds(debuff.Seconds + sheet.Get(nodes.Duration));
        var definition = GetEffectDefinition(debuff.EffectNumber);
        var value = sheet.Get(nodes.Value);
        var improvedValue = debuff.Improved ? sheet.Get(nodes.ImprovedValue) : 0;
        var multiplier = debuff.Improved ? debuff.Multiplier : 1;
        switch (debuff.Kind)
        {
            case EnhanceTreeDebuffKind.Poison:
                var tick = basic.Value + Rand.NextInt(0, (int)basic.ValueRange + 1) + value;
                return new EnhanceTreeDebuffEffect(definition, duration, debuff, attacker, target, (tick * multiplier) + improvedValue, 0);
            case EnhanceTreeDebuffKind.Bleed:
                return new EnhanceTreeDebuffEffect(definition, duration, debuff, attacker, target, (value * multiplier) + improvedValue, basic.Value * multiplier);
            default:
                var defense = new MagicEffect.ElementWithTarget(new ConstantElement(-(debuff.Value + value)), Stats.DefenseBase);
                var slow = new MagicEffect.ElementWithTarget(SlowBy(sheet.Configuration.ChillMovementSlow + improvedValue), Stats.MovementSpeedFactor);
                if (!debuff.Improved)
                {
                    return new EnhanceTreeDebuffEffect(definition, duration, debuff, attacker, target, 0, 0, defense, slow);
                }

                var speed = SlowBy(debuff.AttackSpeed + improvedValue);
                return new EnhanceTreeDebuffEffect(definition, duration, debuff, attacker, target, 0, 0, defense, slow, new(speed, Stats.AttackSpeed), new(speed, Stats.MagicSpeed));
        }
    }

    private static ConstantElement SlowBy(float percent) => new(Math.Max(0, 1 - (percent / 100f)), AggregateType.Multiplicate);

    private static bool Roll(EnhanceTreeConfiguration configuration, float rate)
        => Rand.NextRandomBool(rate / Math.Max(1, configuration.DebuffRateScale));

    private static EnhanceTreeMagicField? FindField(EnhanceTreeConfiguration configuration, EnhanceTreeDebuffKind kind)
    {
        foreach (var field in configuration.MagicFields)
        {
            if (field.Type == (byte)kind)
            {
                return field;
            }
        }

        return null;
    }

    /// <summary>
    /// The values of the nodes of the used skill's sheet.
    /// </summary>
    private readonly record struct SheetValues<TState>(EnhanceTreeConfiguration Configuration, BaseClassCode BaseClass, short Skill, TState State, Func<TState, short, int?> LevelOf)
    {
        public float Get(short[] nodes) => EnhanceTreeEffects.GetValue(this.Configuration, this.BaseClass, this.Skill, this.State, this.LevelOf, nodes);
    }

    /// <summary>
    /// The node skills of a debuff kind, of both columns of a sheet.
    /// </summary>
    private sealed record KindNodes(short[] Rate, short[] Value, short[] Duration, short[] ImprovedValue, short[] Upgrade, short[] Targets, short[] ImprovedTargets, short[] Range, short[] Chance, short[] ImprovedChance);

    /// <summary>
    /// The circle of a kind which an attacker cast last.
    /// </summary>
    private sealed class Circle
    {
        public GameMap? Map { get; private set; }

        public Point Center { get; private set; }

        public DateTime Until { get; private set; }

        public DateTime CooldownUntil { get; private set; }

        public int Debuffed { get; set; }

        public int Upgraded { get; set; }

        public void Cast(GameMap map, Point center, DateTime until, DateTime cooldownUntil)
        {
            this.Map = map;
            this.Center = center;
            this.Until = until;
            this.CooldownUntil = cooldownUntil;
            this.Debuffed = 0;
            this.Upgraded = 0;
        }
    }
}
