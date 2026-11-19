// <copyright file="Season21MagicGladiatorBuffsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// The Magic Gladiator's Season 21 charges: buffs on the caster for 60 + energy / 10 seconds (S21Emu).
/// </summary>
[TestFixture]
internal class Season21MagicGladiatorBuffsTest
{
    private static readonly short[] Charges = [2014, 2015, 2016, 865, 866, 867, 868, 869, 870];

    /// <summary>
    /// Every charge is a buff on the caster whose effect lasts 60 seconds + total energy / 10, with no power-up.
    /// </summary>
    [Test]
    public async Task ChargesAreCasterBuffsLastingByEnergyAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        await new FixSeason21MagicGladiatorBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        foreach (var skill in Charges.Select(n => gameConfiguration.Skills.Single(s => s.Number == n)))
        {
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {skill.Number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitPlayer), $"skill {skill.Number}");
            Assert.That(skill.TargetRestriction, Is.EqualTo(SkillTargetRestriction.Self), $"skill {skill.Number}");
            var effect = skill.MagicEffectDef!;
            Assert.That(effect.Duration!.ConstantValue.Value, Is.EqualTo(60), $"skill {skill.Number}");
            var perEnergy = effect.Duration.RelatedValues.Single();
            Assert.That(perEnergy.InputAttribute!.Id, Is.EqualTo(Stats.TotalEnergy.Id));
            Assert.That(perEnergy.InputOperand, Is.EqualTo(0.1f).Within(1e-6));
            Assert.That(effect.PowerUpDefinitions, Is.Empty);
        }
    }

    /// <summary>
    /// An operator's own duration stays.
    /// </summary>
    [Test]
    public async Task AnOperatorsDurationStaysAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var effect = gameConfiguration.Skills.Single(s => s.Number == 2014).MagicEffectDef!;
        effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
        effect.Duration.ConstantValue.Value = 99;

        await new FixSeason21MagicGladiatorBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(effect.Duration.ConstantValue.Value, Is.EqualTo(99));
        Assert.That(effect.Duration.RelatedValues, Is.Empty);
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
