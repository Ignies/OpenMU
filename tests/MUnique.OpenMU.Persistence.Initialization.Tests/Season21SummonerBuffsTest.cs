// <copyright file="Season21SummonerBuffsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// The Summoner's Season 21 rows: the Weakness and Innovation tiers cast as those skills with their effects, and Blind a hit
/// that halves the target's attack rate.
/// </summary>
[TestFixture]
internal class Season21SummonerBuffsTest
{
    /// <summary>
    /// Each tier is cast as its root (type, target, area) and its effect copies the root's duration, chance and power-ups.
    /// </summary>
    [Test]
    public async Task TheCurseTiersAreCastAsWeaknessAndInnovationAsync()
    {
        var gameConfiguration = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var (root, tier) in new (short, short)[] { (221, 459), (221, 772), (222, 460), (222, 773) })
        {
            var original = Skill(gameConfiguration, root);
            var copy = Skill(gameConfiguration, tier);
            Assert.That(copy.SkillType, Is.EqualTo(original.SkillType), $"skill {tier}");
            Assert.That(copy.Target, Is.EqualTo(original.Target), $"skill {tier}");
            Assert.That(copy.AreaSkillSettings, Is.Not.Null.And.Not.SameAs(original.AreaSkillSettings), $"skill {tier}");
            Assert.That(copy.AreaSkillSettings!.TargetAreaDiameter, Is.EqualTo(original.AreaSkillSettings!.TargetAreaDiameter), $"skill {tier}");
            var effect = copy.MagicEffectDef!;
            var source = original.MagicEffectDef!;
            Assert.That(effect.Duration!.ConstantValue.Value, Is.EqualTo(source.Duration!.ConstantValue.Value), $"skill {tier}");
            Assert.That(effect.Chance?.ConstantValue.Value, Is.EqualTo(source.Chance?.ConstantValue.Value), $"skill {tier}");
            Assert.That(
                effect.PowerUpDefinitions.Select(p => p.TargetAttribute!.Id),
                Is.EquivalentTo(source.PowerUpDefinitions.Select(p => p.TargetAttribute!.Id)),
                $"skill {tier}");
        }
    }

    /// <summary>
    /// Blind is a hit whose effect lasts 5 seconds with a 20 % + energy / 25 chance and halves the attack rate.
    /// </summary>
    [Test]
    public async Task BlindIsAHitThatHalvesTheAttackRateAsync()
    {
        var gameConfiguration = await CreateFixedAsync().ConfigureAwait(false);

        foreach (var number in new short[] { 461, 463 })
        {
            var skill = Skill(gameConfiguration, number);
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.DirectHit), $"skill {number}");
            var effect = skill.MagicEffectDef!;
            Assert.That(effect.Duration!.ConstantValue.Value, Is.EqualTo(5), $"skill {number}");
            Assert.That(effect.Chance!.ConstantValue.Value, Is.EqualTo(0.2f).Within(1e-6), $"skill {number}");
            Assert.That(effect.Chance.RelatedValues.Single().InputOperand, Is.EqualTo(1f / 2500).Within(1e-7), $"skill {number}");
            var rate = effect.PowerUpDefinitions.Single();
            Assert.That(rate.TargetAttribute!.Id, Is.EqualTo(Stats.AttackRatePvm.Id));
            Assert.That(rate.Boost!.ConstantValue.AggregateType, Is.EqualTo(AggregateType.Multiplicate));
            Assert.That(rate.Boost.ConstantValue.Value, Is.EqualTo(0.5f).Within(1e-6));
        }
    }

    private static Skill Skill(GameConfiguration gameConfiguration, short number)
        => gameConfiguration.Skills.Single(s => s.Number == number);

    private static async Task<GameConfiguration> CreateFixedAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new FixSeason21SummonerBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return gameConfiguration;
    }
}
