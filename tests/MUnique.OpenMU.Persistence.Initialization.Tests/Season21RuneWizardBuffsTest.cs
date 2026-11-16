// <copyright file="Season21RuneWizardBuffsTest.cs" company="MUnique">
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
/// Tests the Rune Wizard's buffs: Burst (286, effect 293 Might) and Haste (287, effect 294) and their masters, which the
/// skills update creates without a duration. S21Emu <c>GameServerInfo - Skill.dat</c>: Might and Haste ConstA 30,
/// ConstB 20, TimeConstA 15, TimeConstB 10 (value = A + energy / B, as its Greater Damage 3/7 is OpenMU's 3 + energy / 7).
/// </summary>
[TestFixture]
internal class Season21RuneWizardBuffsTest
{
    /// <summary>
    /// The effects get their duration and value, the skills become buffs: Burst on the caster, Haste on the party.
    /// </summary>
    [Test]
    public async Task BurstAndHasteAreBuffsWithDurationAndValueAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);

        await new FixSeason21RuneWizardBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var might = gameConfiguration.MagicEffects.Single(e => e.Number == 293);
        var haste = gameConfiguration.MagicEffects.Single(e => e.Number == 294);
        Assert.That(might.Duration?.ConstantValue.Value, Is.EqualTo(15));
        Assert.That(might.Duration!.RelatedValues.Single().InputOperand, Is.EqualTo(1f / 10f));
        Assert.That(might.PowerUpDefinitions.Select(p => p.TargetAttribute!.Id), Is.EquivalentTo(new[] { Stats.PhysicalBaseDmg.Id, Stats.WizardryBaseDmg.Id, Stats.CurseBaseDmg.Id }));
        Assert.That(might.PowerUpDefinitions.First().Boost!.ConstantValue.Value, Is.EqualTo(30));
        Assert.That(might.PowerUpDefinitions.First().Boost!.RelatedValues.Single().InputOperand, Is.EqualTo(1f / 20f));
        Assert.That(haste.PowerUpDefinitions.Single().TargetAttribute!.Id, Is.EqualTo(Stats.AttackSpeedAny.Id));

        var burst = gameConfiguration.Skills.Single(s => s.Number == 286);
        var hasteSkill = gameConfiguration.Skills.Single(s => s.Number == 287);
        Assert.That(burst.SkillType, Is.EqualTo(SkillType.Buff));
        Assert.That(burst.Target, Is.EqualTo(SkillTarget.ImplicitPlayer));
        Assert.That(hasteSkill.SkillType, Is.EqualTo(SkillType.Buff));
        Assert.That(hasteSkill.Target, Is.EqualTo(SkillTarget.ImplicitParty));
        Assert.That(gameConfiguration.Skills.Single(s => s.Number == 766).MagicEffectDef?.Duration, Is.Not.Null, "a master's effect");
    }

    /// <summary>
    /// An operator's own duration stays.
    /// </summary>
    [Test]
    public async Task AnOperatorsDurationStaysAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var might = gameConfiguration.MagicEffects.Single(e => e.Number == 293);
        might.Duration = context.CreateNew<PowerUpDefinitionValue>();
        might.Duration.ConstantValue.Value = 99;

        await new FixSeason21RuneWizardBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(might.Duration.ConstantValue.Value, Is.EqualTo(99));
        Assert.That(might.PowerUpDefinitions, Is.Empty);
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
