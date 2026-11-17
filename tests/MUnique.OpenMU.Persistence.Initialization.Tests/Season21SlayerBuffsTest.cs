// <copyright file="Season21SlayerBuffsTest.cs" company="MUnique">
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
/// Tests the Slayer's buffs: Detection (295, effect 316; its Strengthener 794, 318) and Demolish (297, effect 317; 787/788,
/// 319/320), which the skills update creates without a duration. S21Emu <c>GameServerInfo - Skill.dat</c>: DetectionConstA
/// 45, DetectionTimeConstA 60; DemolishConstA 20, DemolishConstB 50, DemolishTimeConstA 60. Skill.xml: Detection Radio 0,
/// Demolish Radio 5.
/// </summary>
[TestFixture]
internal class Season21SlayerBuffsTest
{
    /// <summary>
    /// The effects get their duration and value, the skills become buffs: Detection on the caster, Demolish on the party.
    /// </summary>
    [Test]
    public async Task DetectionAndDemolishAreBuffsWithDurationAndValueAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);

        await new FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        foreach (var number in new short[] { 316, 318 })
        {
            var detection = gameConfiguration.MagicEffects.Single(e => e.Number == number);
            Assert.That(detection.Duration?.ConstantValue.Value, Is.EqualTo(60), $"effect {number}");
            Assert.That(detection.PowerUpDefinitions, Is.Empty, "no stat models sensing life forms");
        }

        foreach (var number in new short[] { 317, 319, 320 })
        {
            var demolish = gameConfiguration.MagicEffects.Single(e => e.Number == number);
            Assert.That(demolish.Duration?.ConstantValue.Value, Is.EqualTo(60), $"effect {number}");
            var ignore = demolish.PowerUpDefinitions.Single();
            Assert.That(ignore.TargetAttribute!.Id, Is.EqualTo(Stats.DefenseIgnoreChance.Id));
            Assert.That(ignore.Boost!.ConstantValue.Value, Is.EqualTo(0.2f).Within(1e-6));
            Assert.That(ignore.Boost.RelatedValues.Single().InputAttribute!.Id, Is.EqualTo(Stats.TotalEnergy.Id));
            Assert.That(ignore.Boost.RelatedValues.Single().InputOperand, Is.EqualTo(0.01f / 50f).Within(1e-9));
        }

        foreach (var number in new short[] { 295, 794 })
        {
            var skill = gameConfiguration.Skills.Single(s => s.Number == number);
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitPlayer));
        }

        foreach (var number in new short[] { 297, 787, 788 })
        {
            var skill = gameConfiguration.Skills.Single(s => s.Number == number);
            Assert.That(skill.SkillType, Is.EqualTo(SkillType.Buff), $"skill {number}");
            Assert.That(skill.Target, Is.EqualTo(SkillTarget.ImplicitParty));
            Assert.That(skill.ImplicitTargetRange, Is.EqualTo(5));
        }
    }

    /// <summary>
    /// An operator's own duration stays.
    /// </summary>
    [Test]
    public async Task AnOperatorsDurationStaysAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var demolish = gameConfiguration.MagicEffects.Single(e => e.Number == 317);
        demolish.Duration = context.CreateNew<PowerUpDefinitionValue>();
        demolish.Duration.ConstantValue.Value = 99;

        await new FixSeason21SlayerAndRageFighterBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(demolish.Duration.ConstantValue.Value, Is.EqualTo(99));
        Assert.That(demolish.PowerUpDefinitions, Is.Empty);
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
