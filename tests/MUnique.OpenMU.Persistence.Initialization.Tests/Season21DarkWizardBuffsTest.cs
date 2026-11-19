// <copyright file="Season21DarkWizardBuffsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// The Dark Wizard's Season 21 buffs: Mana Shield Mastered (406, effect 140) is Mana Shield's tier (Effect.txt group 4).
/// </summary>
[TestFixture]
internal class Season21DarkWizardBuffsTest
{
    private const short ManaShield = 16;
    private const short ManaShieldMastered = 406;

    /// <summary>
    /// Mana Shield Mastered is cast as Mana Shield: the same skill type, target and restriction.
    /// </summary>
    [Test]
    public async Task ManaShieldMasteredIsCastAsManaShieldAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);
        var manaShield = Skill(gameConfiguration, ManaShield);
        var mastered = Skill(gameConfiguration, ManaShieldMastered);

        Assert.That(mastered.SkillType, Is.EqualTo(SkillType.Buff));
        Assert.That(mastered.Target, Is.EqualTo(manaShield.Target));
        Assert.That(mastered.TargetRestriction, Is.EqualTo(manaShield.TargetRestriction));
        Assert.That(mastered.ImplicitTargetRange, Is.EqualTo(manaShield.ImplicitTargetRange));
    }

    /// <summary>
    /// Effect 140 gets Mana Shield's duration and power-ups (<c>SkillManaShield</c> computes one value for every tier).
    /// </summary>
    [Test]
    public async Task ManaShieldMasteredHasManaShieldsEffectAsync()
    {
        var (_, gameConfiguration) = await CreateFixedAsync().ConfigureAwait(false);
        var original = Skill(gameConfiguration, ManaShield).MagicEffectDef!;
        var effect = Skill(gameConfiguration, ManaShieldMastered).MagicEffectDef!;

        Assert.That(effect.Number, Is.EqualTo(140));
        Assert.That(effect.Duration, Is.Not.Null);
        Assert.That(effect.Duration!.ConstantValue.Value, Is.EqualTo(original.Duration!.ConstantValue.Value));
        Assert.That(effect.Duration.RelatedValues.Count, Is.EqualTo(original.Duration.RelatedValues.Count));
        Assert.That(
            effect.PowerUpDefinitions.Select(p => (p.TargetAttribute!.Id, p.Boost!.ConstantValue.Value, p.Boost.RelatedValues.Count)),
            Is.EquivalentTo(original.PowerUpDefinitions.Select(p => (p.TargetAttribute!.Id, p.Boost!.ConstantValue.Value, p.Boost.RelatedValues.Count))));
    }

    /// <summary>
    /// An operator's own duration stays, and the effect then gets no power-up.
    /// </summary>
    [Test]
    public async Task AnOperatorsDurationStaysAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        var effect = Skill(gameConfiguration, ManaShieldMastered).MagicEffectDef!;
        effect.Duration = context.CreateNew<PowerUpDefinitionValue>();
        effect.Duration.ConstantValue.Value = 99;

        await new FixSeason21DarkWizardBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(effect.Duration.ConstantValue.Value, Is.EqualTo(99));
        Assert.That(effect.PowerUpDefinitions, Is.Empty);
    }

    private static Skill Skill(GameConfiguration gameConfiguration, short number)
        => gameConfiguration.Skills.Single(s => s.Number == number);

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateFixedAsync()
    {
        var (context, gameConfiguration) = await CreateWithSkillsAsync().ConfigureAwait(false);
        await new FixSeason21DarkWizardBuffsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
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
