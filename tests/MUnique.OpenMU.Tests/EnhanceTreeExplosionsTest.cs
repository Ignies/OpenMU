// <copyright file="EnhanceTreeExplosionsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// Tests the explosion skills of the skill enhancement tree (<see cref="EnhanceTreeExplosions"/>): Poison Storm 1125,
/// Frozen Slayer 1126 and Bloodying Hit 1127 (S21Emu sheet 3 of every class; meanings from the muonlinefanz.com tree tool).
/// </summary>
[TestFixture]
public class EnhanceTreeExplosionsTest
{
    private const short PoisonStorm = 1125;
    private const short FrozenSlayer = 1126;
    private const short BloodyingHit = 1127;

    private readonly Dictionary<short, int> _levels = [];

    private readonly List<(IAttackable Target, double Factor)> _hits = [];

    /// <summary>
    /// Sets up the learned explosions.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._levels.Clear();
        this._hits.Clear();
        this._levels[PoisonStorm] = 10;
        this._levels[FrozenSlayer] = 10;
        this._levels[BloodyingHit] = 10;
    }

    /// <summary>
    /// The damage nodes of a line (1129 "Enhanced (1)", 1196 "Enhanced (2)") add to its explosion only, and its range node
    /// (1199, formula 218 = level / 10) to its range; the explosion's damage is its formula (139) at the character's total
    /// level: 25000 at 800, 25 more each level.
    /// </summary>
    [Test]
    public void LineNodesImproveTheirExplosionOnly()
    {
        var configuration = CreateConfiguration();
        this._levels[1129] = 1;
        this._levels[1196] = 1;
        this._levels[1199] = 20;
        this._levels[1130] = 1;

        Assert.That(EnhanceTreeEffects.GetExtraSkillDamage(configuration, BaseClassCode.DarkWizard, PoisonStorm, this.LevelOf), Is.EqualTo(2277 + 3019).Within(1));
        Assert.That(EnhanceTreeEffects.GetExtraSkillDamage(configuration, BaseClassCode.DarkWizard, FrozenSlayer, this.LevelOf), Is.EqualTo(2277).Within(1));
        Assert.That(EnhanceTreeEffects.GetReach(configuration, BaseClassCode.DarkWizard, PoisonStorm, this.LevelOf).Range, Is.EqualTo(2));
        Assert.That(EnhanceTreeExplosions.GetDamage(configuration, BaseClassCode.DarkWizard, PoisonStorm, this.LevelOf, 900), Is.EqualTo(27500));
        Assert.That(EnhanceTreeExplosions.GetDamage(configuration, BaseClassCode.DarkWizard, 724, this.LevelOf, 900), Is.Null, "not an explosion");
    }

    /// <summary>
    /// Poison Storm explodes Toxic: the target takes the hit and loses Toxic; up to 2 monsters around it (plus the target
    /// node 1133) get Poisoning, a poisoned one is upgraded to Toxic.
    /// </summary>
    [Test]
    public async ValueTask PoisonStormSpreadsThePoisonAsync()
    {
        var configuration = CreateConfiguration();
        var (attacker, target) = await CreatePlayersAsync().ConfigureAwait(false);
        var first = await PlayerTestHelper.CreatePlayerAsync(target.GameContext).ConfigureAwait(false);
        var second = await PlayerTestHelper.CreatePlayerAsync(target.GameContext).ConfigureAwait(false);
        var third = await PlayerTestHelper.CreatePlayerAsync(target.GameContext).ConfigureAwait(false);
        await PlaceAsync(configuration, EnhanceTreeDebuffKind.Poison, true, attacker, target).ConfigureAwait(false);
        await PlaceAsync(configuration, EnhanceTreeDebuffKind.Poison, false, attacker, first).ConfigureAwait(false);

        Assert.That(await this.ExplodeAsync(configuration, attacker, target, PoisonStorm).ConfigureAwait(false), Is.True);

        Assert.That(this._hits, Is.EqualTo(new[] { ((IAttackable)target, 1.0) }));
        Assert.That(target.MagicEffectList.ContainsEffect(277), Is.False, "Toxic exploded");
        Assert.That(first.MagicEffectList.ContainsEffect(277), Is.True, "Poisoning upgraded");
        Assert.That(new[] { second, third }.Count(p => p.MagicEffectList.ContainsEffect(273)), Is.EqualTo(1), "two targets in all");
    }

    /// <summary>
    /// Frozen Slayer explodes Frostbite and hits the monsters around the target with 30 % of the skill damage, plus the
    /// node 1134 (+1 % at 10).
    /// </summary>
    [Test]
    public async ValueTask FrozenSlayerSplashesAsync()
    {
        var configuration = CreateConfiguration();
        this._levels[1134] = 10;
        var (attacker, target) = await CreatePlayersAsync().ConfigureAwait(false);
        var near = await PlayerTestHelper.CreatePlayerAsync(target.GameContext).ConfigureAwait(false);
        await PlaceAsync(configuration, EnhanceTreeDebuffKind.Chill, true, attacker, target).ConfigureAwait(false);

        await this.ExplodeAsync(configuration, attacker, target, FrozenSlayer).ConfigureAwait(false);

        Assert.That(target.MagicEffectList.ContainsEffect(278), Is.False);
        Assert.That(this._hits, Has.Count.EqualTo(2));
        Assert.That(this._hits[1].Target, Is.SameAs(near));
        Assert.That(this._hits[1].Factor, Is.EqualTo(0.31).Within(0.0001));
    }

    /// <summary>
    /// Bloodying Hit explodes Bleeding Severe and decreases the target's defense by 15 %, plus the node 1135 (+1 % at 10).
    /// </summary>
    [Test]
    public async ValueTask BloodyingHitDecreasesTheDefenseAsync()
    {
        var configuration = CreateConfiguration();
        this._levels[1135] = 10;
        var (attacker, target) = await CreatePlayersAsync().ConfigureAwait(false);
        target.Attributes!.AddElement(new ConstantElement(1), Stats.DefenseDecrement);
        await PlaceAsync(configuration, EnhanceTreeDebuffKind.Bleed, true, attacker, target).ConfigureAwait(false);

        await this.ExplodeAsync(configuration, attacker, target, BloodyingHit).ConfigureAwait(false);

        Assert.That(target.MagicEffectList.ContainsEffect(279), Is.False);
        Assert.That(this._hits, Has.Count.EqualTo(1));
        Assert.That(target.Attributes[Stats.DefenseDecrement], Is.EqualTo(0.84).Within(0.0001));
    }

    /// <summary>
    /// Without the upgraded debuff of its kind there is nothing to explode: no hit.
    /// </summary>
    [Test]
    public async ValueTask NoExplosionWithoutTheDebuffAsync()
    {
        var configuration = CreateConfiguration();
        var (attacker, target) = await CreatePlayersAsync().ConfigureAwait(false);
        await PlaceAsync(configuration, EnhanceTreeDebuffKind.Poison, false, attacker, target).ConfigureAwait(false);

        Assert.That(await this.ExplodeAsync(configuration, attacker, target, PoisonStorm).ConfigureAwait(false), Is.True);
        Assert.That(this._hits, Is.Empty);
        Assert.That(await this.ExplodeAsync(configuration, attacker, target, 724).ConfigureAwait(false), Is.False, "not an explosion");
    }

    private static async ValueTask<(Player Attacker, Player Target)> CreatePlayersAsync()
    {
        var attacker = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        return (attacker, await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false));
    }

    private static ValueTask PlaceAsync(EnhanceTreeConfiguration configuration, EnhanceTreeDebuffKind kind, bool improved, IAttacker attacker, IAttackable target)
        => EnhanceTreeDebuffs.PlaceAsync(configuration, kind, improved, attacker, target);

    private static EnhanceTreeConfiguration CreateConfiguration()
    {
        var configuration = new EnhanceTreeConfiguration();
        void Line(byte start, short explosion, short range, short damage1, short effect, short damage2)
        {
            Add(start, explosion, explosion, 139);
            Add((byte)(start + 1), range, explosion, 218);
            Add((byte)(start + 2), damage1, explosion, 143);
            Add((byte)(start + 3), effect, explosion, 147);
            Add((byte)(start + 4), damage2, explosion, 215);
        }

        void Add(byte index, short skill, short related, short formula)
            => configuration.Nodes.Add(new() { BaseClass = (byte)BaseClassCode.DarkWizard, Group = 3, Index = index, Skill = skill, Color = 3, RelatedSkill = related, FormulaId = formula });

        Line(0, PoisonStorm, 1199, 1129, 1133, 1196);
        Line(10, FrozenSlayer, 1200, 1130, 1134, 1197);
        Line(20, BloodyingHit, 1201, 1131, 1135, 1198);
        configuration.Formulas[139] = "(25000+((level-800)*25))+((level+level+level)*0)";
        configuration.Formulas[143] = "(((15+level)^2.5))+1253";
        configuration.Formulas[147] = "10/level";
        configuration.Formulas[215] = "(((15+level)^2.55))+1843";
        configuration.Formulas[218] = "level/10";
        return configuration;
    }

    private ValueTask<bool> ExplodeAsync(EnhanceTreeConfiguration configuration, Player attacker, IAttackable target, short skill)
        => EnhanceTreeExplosions.TryExplodeAsync(configuration, BaseClassCode.DarkWizard, this.LevelOf, attacker, target, skill, this.HitAsync);

    private ValueTask HitAsync(IAttackable target, double factor)
    {
        this._hits.Add((target, factor));
        return ValueTask.CompletedTask;
    }

    private int? LevelOf(short skill) => this._levels.TryGetValue(skill, out var level) ? level : null;
}
