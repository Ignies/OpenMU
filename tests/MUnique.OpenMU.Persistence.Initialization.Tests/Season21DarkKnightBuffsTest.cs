// <copyright file="Season21DarkKnightBuffsTest.cs" company="MUnique">
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
/// The Dark Knight's Season 21 buffs: Greater Life Mastered, Sword's Fury, Firm Belief and Strong Blessing (and their
/// master and enhancement rows) as buffs with S21Emu's durations and values.
/// </summary>
[TestFixture]
internal class Season21DarkKnightBuffsTest
{
    private static readonly short[] SwordsFurySkills = [1500, 801, 802];
    private static readonly short[] FirmBeliefSkills = [1502, 810];
    private static readonly short[] StrongBlessingSkills = [1503, 803, 804, 805, 806, 1204];

    /// <summary>
    /// Greater Life Mastered (363) is Greater Fortitude's Season 21 tier (Effect.txt group 8): the same party buff, duration
    /// and power-ups as effect 8.
    /// </summary>
    [Test]
    public async Task GreaterLifeMasteredIsGreaterFortitudesTierAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);
        var skill = gameConfiguration.Skills.Single(s => s.Number == 363);
        var fortitude = gameConfiguration.MagicEffects.Single(e => e.Number == 8);

        Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff));
        Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitParty));
        var effect = skill.MagicEffectDef!;
        Assert.That(effect.Number, Is.EqualTo(136));
        Assert.That(effect.Duration!.ConstantValue.Value, Is.EqualTo(fortitude.Duration!.ConstantValue.Value));
        Assert.That(effect.Duration.RelatedValues.Count, Is.EqualTo(fortitude.Duration.RelatedValues.Count));
        Assert.That(
            effect.PowerUpDefinitions.Select(p => (p.TargetAttribute!.Id, p.Boost!.ConstantValue.Value, p.Boost.RelatedValues.Count)),
            Is.EquivalentTo(fortitude.PowerUpDefinitions.Select(p => (p.TargetAttribute!.Id, p.Boost!.ConstantValue.Value, p.Boost.RelatedValues.Count))));
        Assert.That(effect.PowerUpDefinitions.Select(p => p.Boost), Is.Not.EquivalentTo(fortitude.PowerUpDefinitions.Select(p => p.Boost)), "copied, not shared");
    }

    /// <summary>
    /// Sword's Fury: 60 seconds on the caster, attack skills reach 1 tile further (SwordFuryConstA = 1).
    /// </summary>
    [Test]
    public async Task SwordsFuryLengthensTheAttackSkillsAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var skill in Skills(gameConfiguration, SwordsFurySkills))
        {
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {skill.Number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitPlayer));
            var effect = skill.MagicEffectDef!;
            Assert.That(effect.Duration?.ConstantValue.Value, Is.EqualTo(60));
            var range = effect.PowerUpDefinitions.Single();
            Assert.That(range.TargetAttribute!.Id, Is.EqualTo(Stats.AttackSkillRangeBonus.Id));
            Assert.That(range.Boost!.ConstantValue.Value, Is.EqualTo(1));
        }
    }

    /// <summary>
    /// Firm Belief: 120 seconds on the caster, base defense + 35 and elemental defense + (30 + dexterity / 60) %.
    /// </summary>
    [Test]
    public async Task FirmBeliefRaisesTheDefensesAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var skill in Skills(gameConfiguration, FirmBeliefSkills))
        {
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {skill.Number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitPlayer));
            var effect = skill.MagicEffectDef!;
            Assert.That(effect.Duration?.ConstantValue.Value, Is.EqualTo(120));
            var defense = effect.PowerUpDefinitions.Single(p => p.TargetAttribute!.Id == Stats.DefenseBase.Id);
            Assert.That(defense.Boost!.ConstantValue.Value, Is.EqualTo(35));
            var elemental = effect.PowerUpDefinitions.Single(p => p.TargetAttribute!.Id == Stats.ElementDefenseIncrease.Id);
            Assert.That(elemental.Boost!.ConstantValue.Value, Is.EqualTo(0.3f).Within(1e-6));
            var perDexterity = elemental.Boost.RelatedValues.Single();
            Assert.That(perDexterity.InputAttribute!.Id, Is.EqualTo(Stats.TotalAgility.Id));
            Assert.That(perDexterity.InputOperand, Is.EqualTo(1f / 6000).Within(1e-9));
        }
    }

    /// <summary>
    /// Strong Blessing: 300 seconds on the caster's party within 10, base damage + 250 (SolidProtectionConstA).
    /// </summary>
    [Test]
    public async Task StrongBlessingRaisesThePartysDamageAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var skill in Skills(gameConfiguration, StrongBlessingSkills))
        {
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {skill.Number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitParty));
            Assert.That(skill.ImplicitTargetRange, Is.EqualTo(10));
            var effect = skill.MagicEffectDef!;
            Assert.That(effect.Duration?.ConstantValue.Value, Is.EqualTo(300));
            var damage = effect.PowerUpDefinitions.Single();
            Assert.That(damage.TargetAttribute!.Id, Is.EqualTo(Stats.BaseDamageBonus.Id));
            Assert.That(damage.Boost!.ConstantValue.Value, Is.EqualTo(250));
        }
    }

    /// <summary>
    /// An operator's own duration stays, and the effect then gets no power-up.
    /// </summary>
    [Test]
    public async Task AnOperatorsDurationStaysAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var fury = gameConfiguration.Skills.Single(s => s.Number == 1500).MagicEffectDef!;
        fury.Duration = context.CreateNew<PowerUpDefinitionValue>();
        fury.Duration.ConstantValue.Value = 99;

        await new FixSeason21DarkKnightBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(fury.Duration.ConstantValue.Value, Is.EqualTo(99));
        Assert.That(fury.PowerUpDefinitions, Is.Empty);
    }

    private static IEnumerable<Skill> Skills(GameConfiguration gameConfiguration, short[] numbers)
        => numbers.Select(n => gameConfiguration.Skills.Single(s => s.Number == n));

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateFixedAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        await new FixSeason21DarkKnightBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
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
