// <copyright file="EnhanceTreeDebuffsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// Tests the debuff circles of the skill enhancement tree (<see cref="EnhanceTreeDebuffs"/>): S21Emu
/// <c>EnhanceSkillTreeMagicField.xml</c> and the debuff nodes of <c>EnhanceSkillTree.xml</c>.
/// </summary>
[TestFixture]
public class EnhanceTreeDebuffsTest
{
    private const short MeteorStrike = 724;
    private const short MeteorStrikeExtended = 1075;

    private static readonly DateTime Start = new(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc);

    private readonly Dictionary<short, int> _levels = [];

    /// <summary>
    /// Sets up the learned extension of the sheet.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._levels.Clear();
        this._levels[MeteorStrikeExtended] = 1;
    }

    /// <summary>
    /// The chill rate node casts the circle on a hit; the target in it gets Chilling (274): the defense decreases by 106
    /// (S21Emu <c>Defense</c>) plus the value node (1029). A second hit doesn't stack it.
    /// </summary>
    [Test]
    public async ValueTask ChillCircleDecreasesTheDefenseAsync()
    {
        var configuration = CreateConfiguration((1025, "100"), (1029, "level*10"));
        this._levels[1025] = 1;
        this._levels[1029] = 5;
        var (attacker, target) = await CreatePlayersAsync().ConfigureAwait(false);

        await this.HitAsync(configuration, attacker, target, Start).ConfigureAwait(false);
        await this.HitAsync(configuration, attacker, target, Start.AddSeconds(1)).ConfigureAwait(false);

        Assert.That(target.MagicEffectList.ContainsEffect(274), Is.True);
        Assert.That(target.Attributes![Stats.DefenseBase], Is.EqualTo(-156));
    }

    /// <summary>
    /// With the upgrade node (1041), a hit on a chilled target in the circle upgrades it to Frostbite (278), which also
    /// decreases the attack speed by 20 % (S21Emu <c>AttackSpeed</c>) plus the improved value node (1037).
    /// </summary>
    [Test]
    public async ValueTask ChillUpgradesToFrostbiteAsync()
    {
        var configuration = CreateConfiguration((1025, "100"), (1041, "level/10"), (1037, "level"));
        this._levels[1025] = 1;
        this._levels[1041] = 10;
        this._levels[1037] = 10;
        var (attacker, target) = await CreatePlayersAsync().ConfigureAwait(false);
        target.Attributes!.AddElement(new ConstantElement(100), Stats.AttackSpeed);

        await this.HitAsync(configuration, attacker, target, Start).ConfigureAwait(false);
        Assert.That(target.MagicEffectList.ContainsEffect(274), Is.True);
        await this.HitAsync(configuration, attacker, target, Start.AddSeconds(1)).ConfigureAwait(false);

        Assert.That(target.MagicEffectList.ContainsEffect(274), Is.False);
        Assert.That(target.MagicEffectList.ContainsEffect(278), Is.True);
        Assert.That(target.Attributes[Stats.DefenseBase], Is.EqualTo(-106));
        Assert.That(target.Attributes[Stats.AttackSpeed], Is.EqualTo(70).Within(0.01));
    }

    /// <summary>
    /// The circle lasts 10 seconds and is cast again after its cooldown of 20 seconds (S21Emu <c>DisappearSec</c>,
    /// <c>CooldownSec</c>); between them a hit places nothing.
    /// </summary>
    [Test]
    public async ValueTask CircleWaitsForItsCooldownAsync()
    {
        var configuration = CreateConfiguration((1025, "100"));
        this._levels[1025] = 1;
        var (attacker, first) = await CreatePlayersAsync().ConfigureAwait(false);
        var second = await PlayerTestHelper.CreatePlayerAsync(first.GameContext).ConfigureAwait(false);
        var third = await PlayerTestHelper.CreatePlayerAsync(first.GameContext).ConfigureAwait(false);

        await this.HitAsync(configuration, attacker, first, Start).ConfigureAwait(false);
        await this.HitAsync(configuration, attacker, second, Start.AddSeconds(15)).ConfigureAwait(false);
        await this.HitAsync(configuration, attacker, third, Start.AddSeconds(21)).ConfigureAwait(false);

        Assert.That(first.MagicEffectList.ContainsEffect(274), Is.True);
        Assert.That(second.MagicEffectList.ContainsEffect(274), Is.False);
        Assert.That(third.MagicEffectList.ContainsEffect(274), Is.True);
    }

    /// <summary>
    /// The circle debuffs at most its targets: one (<see cref="EnhanceTreeConfiguration.DebuffCircleTargets"/>), plus the
    /// target nodes (1170).
    /// </summary>
    [Test]
    public async ValueTask CircleDebuffsItsTargetsOnlyAsync()
    {
        var configuration = CreateConfiguration((1025, "100"), (1170, "level/10"));
        this._levels[1025] = 1;
        var (attacker, first) = await CreatePlayersAsync().ConfigureAwait(false);
        var second = await PlayerTestHelper.CreatePlayerAsync(first.GameContext).ConfigureAwait(false);
        Assert.That(second.CurrentMap, Is.SameAs(first.CurrentMap));

        await this.HitAsync(configuration, attacker, first, Start).ConfigureAwait(false);
        await this.HitAsync(configuration, attacker, second, Start.AddSeconds(1)).ConfigureAwait(false);

        Assert.That(first.MagicEffectList.ContainsEffect(274), Is.True);
        Assert.That(second.MagicEffectList.ContainsEffect(274), Is.False);

        this._levels[1170] = 10;
        var third = await PlayerTestHelper.CreatePlayerAsync(first.GameContext).ConfigureAwait(false);
        await this.HitAsync(configuration, attacker, third, Start.AddSeconds(30)).ConfigureAwait(false);
        await this.HitAsync(configuration, attacker, second, Start.AddSeconds(31)).ConfigureAwait(false);

        Assert.That(second.MagicEffectList.ContainsEffect(274), Is.True, "two targets with the target node");
    }

    /// <summary>
    /// Bleeding (275) adds one damage to each later skill hit: 20 % of it (S21Emu <c>DamagePer</c>) plus the value node
    /// (1030); the hit which places it doesn't bleed.
    /// </summary>
    [Test]
    public async ValueTask BleedingAddsDamageToTheNextSkillHitsAsync()
    {
        var configuration = CreateConfiguration((1026, "100"), (1030, "level"));
        this._levels[1026] = 1;
        this._levels[1030] = 10;
        var (attacker, target) = await CreatePlayersAsync().ConfigureAwait(false);
        target.Attributes![Stats.CurrentHealth] = 100;
        target.Attributes[Stats.CurrentShield] = 0;

        await this.HitAsync(configuration, attacker, target, Start).ConfigureAwait(false);
        Assert.That(target.MagicEffectList.ContainsEffect(275), Is.True);
        Assert.That(target.Attributes[Stats.CurrentHealth], Is.EqualTo(100));

        await this.HitAsync(configuration, attacker, target, Start.AddSeconds(1), 100).ConfigureAwait(false);

        Assert.That(target.Attributes[Stats.CurrentHealth], Is.EqualTo(70));
    }

    /// <summary>
    /// Poisoning (273) lasts 18 seconds plus the duration node (1032); it deals 2000 to 3000 (S21Emu <c>PoisonDamage</c>,
    /// <c>PoisonDamageRange</c>) plus the value node (1028) ten times over the 18 seconds (<c>EffectCountBase</c>).
    /// </summary>
    [Test]
    public async ValueTask PoisoningDealsItsDamageOverTimeAsync()
    {
        var configuration = CreateConfiguration((1024, "100"), (1028, "level"), (1032, "level"));
        this._levels[1024] = 1;
        this._levels[1028] = 50;
        this._levels[1032] = 3;
        var (attacker, target) = await CreatePlayersAsync().ConfigureAwait(false);

        await this.HitAsync(configuration, attacker, target, Start).ConfigureAwait(false);

        Assert.That(target.MagicEffectList.TryGetEffect(273, out var effect), Is.True);
        var poison = (EnhanceTreeDebuffEffect)effect!;
        Assert.That(poison.Duration, Is.EqualTo(TimeSpan.FromSeconds(21)));
        Assert.That(poison.Interval, Is.EqualTo(TimeSpan.FromSeconds(1.8)));
        Assert.That(poison.Damage, Is.InRange(2050, 3050));
    }

    /// <summary>
    /// The default rows are the debuffs of S21Emu <c>EnhanceSkillTreeMagicField.xml</c> (MagicFieldBuffInfo).
    /// </summary>
    [Test]
    public void DefaultDebuffsAreTheS21EmuRows()
    {
        var rows = new EnhanceTreeConfiguration().Debuffs
            .Select(d => (d.Kind, d.Improved, d.EffectNumber, d.Seconds, d.Count, d.SuccessRate, d.Value, d.ValueRange, d.Multiplier, d.AttackSpeed))
            .ToList();

        Assert.That(rows, Is.EqualTo(new[]
        {
            (EnhanceTreeDebuffKind.Poison, false, (short)273, 18, 10, 200, 2000f, 1000f, 0f, 0f),
            (EnhanceTreeDebuffKind.Poison, true, (short)277, 18, 10, 100, 0f, 0f, 2f, 0f),
            (EnhanceTreeDebuffKind.Chill, false, (short)274, 20, 10, 200, 106f, 0f, 0f, 0f),
            (EnhanceTreeDebuffKind.Chill, true, (short)278, 20, 10, 100, 106f, 0f, 0f, 20f),
            (EnhanceTreeDebuffKind.Bleed, false, (short)275, 18, 10, 200, 20f, 0f, 0f, 0f),
            (EnhanceTreeDebuffKind.Bleed, true, (short)279, 18, 10, 100, 0f, 0f, 2f, 0f),
        }));
    }

    private static async ValueTask<(Player Attacker, Player Target)> CreatePlayersAsync()
        => (await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false), await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false));

    private static EnhanceTreeConfiguration CreateConfiguration(params (short Skill, string Formula)[] nodes)
    {
        var configuration = new EnhanceTreeConfiguration
        {
            Nodes = [new() { BaseClass = (byte)BaseClassCode.DarkWizard, Group = 0, Index = 0, Skill = MeteorStrikeExtended, RelatedSkill = MeteorStrike, FormulaId = 115 }],
            Formulas = { [115] = "1" },
            MagicFields =
            [
                new() { Type = 0, Seconds = 10, CooldownSeconds = 20 },
                new() { Type = 1, Seconds = 10, CooldownSeconds = 20 },
                new() { Type = 2, Seconds = 10, CooldownSeconds = 20 },
            ],

            // Every success roll succeeds: the rates of the rows are 200 and 100.
            DebuffRateScale = 100,
        };
        short formulaId = 1;
        foreach (var (skill, formula) in nodes)
        {
            configuration.Nodes.Add(new() { BaseClass = (byte)BaseClassCode.DarkWizard, Group = 0, Index = (byte)(formulaId + 10), Skill = skill, FormulaId = formulaId });
            configuration.Formulas[formulaId++] = formula;
        }

        return configuration;
    }

    private ValueTask HitAsync(EnhanceTreeConfiguration configuration, Player attacker, Player target, DateTime now, uint damage = 10)
        => EnhanceTreeDebuffs.ApplyAfterHitAsync(configuration, BaseClassCode.DarkWizard, this.LevelOf, attacker, target, MeteorStrike, new HitInfo(damage, 0, DamageAttributes.Undefined), now);

    private int? LevelOf(short skill) => this._levels.TryGetValue(skill, out var level) ? level : null;
}
