// <copyright file="Season21FairyElfBuffsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// The Fairy Elf's Season 21 buffs: Bless, Elemental Damage and Defense Enhancement and the Greater Damage and Defense
/// enhancement rows, as buffs on a target with S21Emu's durations and values.
/// </summary>
[TestFixture]
internal class Season21FairyElfBuffsTest
{
    private static readonly short[] BlessSkills = [430, 433, 1230];
    private static readonly short[] ElementalDamageSkills = [2026, 1224];
    private static readonly short[] ElementalDefenseSkills = [2027, 1225];

    /// <summary>
    /// Every fixed row is a buff the elf casts on a player within its range (as Season 6's Greater Damage).
    /// </summary>
    [Test]
    public async Task TheRowsAreBuffsOnATargetAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var skill in Skills(gameConfiguration, [.. BlessSkills, .. ElementalDamageSkills, .. ElementalDefenseSkills, 1228, 1229]))
        {
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {skill.Number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.Explicit), $"skill {skill.Number}");
            Assert.That(skill.TargetRestriction, Is.EqualTo(SkillTargetRestriction.Player), $"skill {skill.Number}");
        }
    }

    /// <summary>
    /// Bless (<c>SkillBless</c>): 300 seconds, every stat + energy / 100, read from the base energy (a total would feed itself
    /// on a self cast).
    /// </summary>
    [Test]
    public async Task BlessRaisesEveryStatByEnergyAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);
        var stats = new[] { Stats.TotalStrength, Stats.TotalAgility, Stats.TotalVitality, Stats.TotalEnergy, Stats.TotalLeadership }.Select(s => s.Id);

        foreach (var skill in Skills(gameConfiguration, BlessSkills))
        {
            var effect = skill.MagicEffectDef!;
            Assert.That(effect.Duration?.ConstantValue.Value, Is.EqualTo(300), $"skill {skill.Number}");
            Assert.That(effect.PowerUpDefinitions.Select(p => p.TargetAttribute!.Id), Is.EquivalentTo(stats));
            foreach (var powerUp in effect.PowerUpDefinitions)
            {
                var perEnergy = powerUp.Boost!.RelatedValues.Single();
                Assert.That(perEnergy.InputAttribute!.Id, Is.EqualTo(Stats.BaseEnergy.Id));
                Assert.That(perEnergy.InputOperand, Is.EqualTo(0.01f).Within(1e-6));
            }
        }
    }

    /// <summary>
    /// Elemental Damage Enhancement: 60 seconds, elemental damage + 3 + energy / 10; Elemental Defense Enhancement: 60
    /// seconds, elemental defense + (2 + energy / 12) x 2.
    /// </summary>
    [Test]
    public async Task ElementalEnhancementsRaiseElementalDamageAndDefenseAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var (numbers, stat, constant, perEnergy) in new[] { (ElementalDamageSkills, Stats.ElementDamageBonus, 3f, 1f / 10), (ElementalDefenseSkills, Stats.ElementDefense, 4f, 1f / 6) })
        {
            foreach (var skill in Skills(gameConfiguration, numbers))
            {
                var effect = skill.MagicEffectDef!;
                Assert.That(effect.Duration?.ConstantValue.Value, Is.EqualTo(60), $"skill {skill.Number}");
                var powerUp = effect.PowerUpDefinitions.Single();
                Assert.That(powerUp.TargetAttribute!.Id, Is.EqualTo(stat.Id), $"skill {skill.Number}");
                Assert.That(powerUp.Boost!.ConstantValue.Value, Is.EqualTo(constant).Within(1e-6));
                var energy = powerUp.Boost.RelatedValues.Single();
                Assert.That(energy.InputAttribute!.Id, Is.EqualTo(Stats.TotalEnergy.Id));
                Assert.That(energy.InputOperand, Is.EqualTo(perEnergy).Within(1e-6));
            }
        }
    }

    /// <summary>
    /// The Greater Damage (1228) and Greater Defense (1229) enhancement rows are those buffs' tiers: effect 1's and 2's
    /// duration and power-ups.
    /// </summary>
    [Test]
    public async Task GreaterDamageAndDefenseRowsAreTheirSeasonSixBuffsAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var (number, season6) in new (short, short)[] { (1228, 1), (1229, 2) })
        {
            var effect = gameConfiguration.Skills.Single(s => s.Number == number).MagicEffectDef!;
            var original = gameConfiguration.MagicEffects.Single(e => e.Number == season6);
            Assert.That(effect.Duration!.ConstantValue.Value, Is.EqualTo(original.Duration!.ConstantValue.Value), $"skill {number}");
            Assert.That(
                effect.PowerUpDefinitions.Select(p => (p.TargetAttribute!.Id, p.Boost!.ConstantValue.Value, p.Boost.RelatedValues.Count)),
                Is.EquivalentTo(original.PowerUpDefinitions.Select(p => (p.TargetAttribute!.Id, p.Boost!.ConstantValue.Value, p.Boost.RelatedValues.Count))));
        }
    }

    /// <summary>
    /// An operator's own duration stays, and the effect then gets no power-up.
    /// </summary>
    [Test]
    public async Task AnOperatorsDurationStaysAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var bless = gameConfiguration.Skills.Single(s => s.Number == 430).MagicEffectDef!;
        bless.Duration = context.CreateNew<PowerUpDefinitionValue>();
        bless.Duration.ConstantValue.Value = 99;

        await new FixSeason21FairyElfBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(bless.Duration.ConstantValue.Value, Is.EqualTo(99));
        Assert.That(bless.PowerUpDefinitions, Is.Empty);
    }

    private static IEnumerable<Skill> Skills(GameConfiguration gameConfiguration, short[] numbers)
        => numbers.Select(n => gameConfiguration.Skills.Single(s => s.Number == n));

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateFixedAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        await new FixSeason21FairyElfBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateWithSkillsAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }
}
