// <copyright file="Season21GrowLancerBuffsTest.cs" company="MUnique">
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
/// The Grow Lancer's Circle Shield, Obsidian and Wrath (and their master rows) as buffs with S21Emu's durations and values.
/// </summary>
[TestFixture]
internal class Season21GrowLancerBuffsTest
{
    private static readonly short[] CircleShieldSkills = [272, 703, 706];
    private static readonly short[] ObsidianSkills = [273, 693];
    private static readonly short[] WrathSkills = [278, 708, 709, 710, 895];

    /// <summary>
    /// Circle Shield: 30 seconds on the caster, no stat (its AG drain in PvP has no OpenMU mechanism).
    /// </summary>
    [Test]
    public async Task CircleShieldIsACasterBuffOfThirtySecondsAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var skill in Skills(gameConfiguration, CircleShieldSkills))
        {
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {skill.Number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitPlayer));
            Assert.That(skill.MagicEffectDef!.Duration?.ConstantValue.Value, Is.EqualTo(30));
            Assert.That(skill.MagicEffectDef.PowerUpDefinitions, Is.Empty);
        }
    }

    /// <summary>
    /// Obsidian: on the caster's party within 5 for 120 seconds, skill damage + strength / 20.
    /// </summary>
    [Test]
    public async Task ObsidianRaisesThePartysSkillDamageByStrengthAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var skill in Skills(gameConfiguration, ObsidianSkills))
        {
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {skill.Number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitParty));
            Assert.That(skill.ImplicitTargetRange, Is.EqualTo(5));
            var effect = skill.MagicEffectDef!;
            Assert.That(effect.Duration?.ConstantValue.Value, Is.EqualTo(120));
            var damage = effect.PowerUpDefinitions.Single();
            Assert.That(damage.TargetAttribute!.Id, Is.EqualTo(Stats.SkillDamageBonus.Id));
            var perStrength = damage.Boost!.RelatedValues.Single();
            Assert.That(perStrength.InputAttribute!.Id, Is.EqualTo(Stats.TotalStrength.Id));
            Assert.That(perStrength.InputOperand, Is.EqualTo(1f / 20).Within(1e-6));
        }
    }

    /// <summary>
    /// Obsidian lasts 360 seconds without a party and 120 on one (S21Emu SOLO_DURATION, PARTY_DURATION): 360 - 240 x
    /// "Is in party", a stat every class has. An operator's own duration stays.
    /// </summary>
    [Test]
    public async Task ObsidianLastsLongerWithoutAPartyAsync()
    {
        var (context, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);
        var improved = gameConfiguration.Skills.Single(s => s.Number == 693).MagicEffectDef!;
        improved.Duration!.ConstantValue.Value = 99;

        await new FixSeason21ObsidianSoloDurationUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var duration = gameConfiguration.Skills.Single(s => s.Number == 273).MagicEffectDef!.Duration!;
        Assert.That(duration.ConstantValue.Value, Is.EqualTo(360));
        var inParty = duration.RelatedValues.Single();
        Assert.That(inParty.InputAttribute!.Id, Is.EqualTo(Stats.IsInParty.Id));
        Assert.That(inParty.InputOperator, Is.EqualTo(InputOperator.Multiply));
        Assert.That(inParty.InputOperand, Is.EqualTo(-240));
        Assert.That(gameConfiguration.CharacterClasses.Where(c => c.StatAttributes.All(s => s.Attribute!.Id != Stats.IsInParty.Id)), Is.Empty);
        Assert.That(improved.Duration.ConstantValue.Value, Is.EqualTo(99));
        Assert.That(improved.Duration.RelatedValues, Is.Empty);
    }

    /// <summary>
    /// Wrath: on the caster for 15 seconds, 10 % more attack damage and 5 % less defense.
    /// </summary>
    [Test]
    public async Task WrathTradesDefenseForDamageAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var skill in Skills(gameConfiguration, WrathSkills))
        {
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {skill.Number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitPlayer));
            var effect = skill.MagicEffectDef!;
            Assert.That(effect.Duration?.ConstantValue.Value, Is.EqualTo(15));
            var damage = effect.PowerUpDefinitions.Single(p => p.TargetAttribute!.Id == Stats.AttackDamageIncrease.Id);
            Assert.That(damage.Boost!.ConstantValue.Value, Is.EqualTo(1.1f).Within(1e-6));
            Assert.That(damage.Boost.ConstantValue.AggregateType, Is.EqualTo(AggregateType.Multiplicate));
            var defense = effect.PowerUpDefinitions.Single(p => p.TargetAttribute!.Id == Stats.DefenseDecrement.Id);
            Assert.That(defense.Boost!.ConstantValue.Value, Is.EqualTo(0.95f).Within(1e-6));
            Assert.That(defense.Boost.ConstantValue.AggregateType, Is.EqualTo(AggregateType.Multiplicate));
        }
    }

    /// <summary>
    /// An operator's own duration stays, and the effect then gets no power-up.
    /// </summary>
    [Test]
    public async Task AnOperatorsDurationStaysAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var wrath = gameConfiguration.Skills.Single(s => s.Number == 278).MagicEffectDef!;
        wrath.Duration = context.CreateNew<PowerUpDefinitionValue>();
        wrath.Duration.ConstantValue.Value = 99;

        await new FixSeason21GrowLancerBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(wrath.Duration.ConstantValue.Value, Is.EqualTo(99));
        Assert.That(wrath.PowerUpDefinitions, Is.Empty);
    }

    private static IEnumerable<Skill> Skills(GameConfiguration gameConfiguration, short[] numbers)
        => numbers.Select(n => gameConfiguration.Skills.Single(s => s.Number == n));

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateFixedAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        await new FixSeason21GrowLancerBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
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
