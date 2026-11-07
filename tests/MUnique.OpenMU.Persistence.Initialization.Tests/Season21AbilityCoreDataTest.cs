// <copyright file="Season21AbilityCoreDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the <see cref="AddSeason21AbilityCoreUpdatePlugIn"/>: the cards, their options and prices of the Ability
/// Enhancement Core of Season 21.
/// </summary>
[TestFixture]
internal class Season21AbilityCoreDataTest
{
    private const byte CardGroup = 20;

    /// <summary>
    /// A new database has the core off and without cards, and the update stays pending.
    /// </summary>
    [Test]
    public async Task NewDatabaseLeavesTheUpdatePendingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        var plugIn = gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeof(AbilityCoreFeaturePlugIn).GUID);
        Assert.That(plugIn?.IsActive ?? false, Is.False);
        Assert.That(plugIn?.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())?.Cards ?? [], Is.Empty);
        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key);
        Assert.That(installed, Has.None.EqualTo(new AddSeason21AbilityCoreUpdatePlugIn().Key));
    }

    /// <summary>
    /// The update fills and turns on the core: 45 cards with their kinds, the skill cards with their skills, the prices.
    /// </summary>
    [Test]
    public async Task CoreIsConfiguredFromTheDataAsync()
    {
        var (_, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);

        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(AbilityCoreFeaturePlugIn).GUID);
        var configuration = plugIn.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())!;
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Cards, Has.Count.EqualTo(45));
        Assert.That(configuration.Cards.Count(c => c.Kind == AbilityCardKind.Skill), Is.EqualTo(24));
        Assert.That(configuration.Cards.Count(c => c.Kind == AbilityCardKind.Option), Is.EqualTo(15));
        var meteor = configuration.Cards.Single(c => c.ItemNumber == 165);
        Assert.That((meteor.Skill, meteor.RequiredSkill, meteor.IsFor(BaseClassCode.DarkWizard), meteor.IsFor(BaseClassCode.DarkKnight)), Is.EqualTo(((short)2037, (short)1076, true, false)));
        var blue = configuration.Cards.Single(c => c.ItemNumber == 213);
        Assert.That((blue.Kind, blue.Color, blue.Type), Is.EqualTo((AbilityCardKind.Option, AbilityCardColor.Blue, (byte)1)));
        Assert.That(blue.Options.Single(o => o.Number == 19).AppliesTo(BaseClassCode.DarkWizard), Is.False);
        Assert.That(configuration.LevelUps.Single(l => l.Kind == AbilityCardKind.Skill && l.Level == 4).Cards, Is.EqualTo(2));
        Assert.That(configuration.Reductions.Single(r => r.Kind == AbilityCardKind.Elite).Stones, Is.EqualTo(5));
    }

    /// <summary>
    /// A card carries its options as item options whose value follows the card level.
    /// </summary>
    [Test]
    public async Task CardsCarryTheirOptionsAsync()
    {
        var (_, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);

        var options = Options(gameConfiguration, 213);
        Assert.That(options, Has.Count.EqualTo(13));
        var minimum = options.Single(o => o.Number == 19);
        Assert.That(minimum.LevelType, Is.EqualTo(LevelType.ItemLevel));
        Assert.That(minimum.PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.MinimumPhysBaseDmg));
        Assert.That(minimum.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(108));
        var rate = Options(gameConfiguration, 228).Single(o => o.Number == 50);
        Assert.That(rate.PowerUpDefinition!.TargetAttribute, Is.EqualTo(Stats.CriticalDamageChance));
        Assert.That(rate.LevelDependentOptions.Single(l => l.Level == 0).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(0.02f).Within(0.0001));
        Assert.That(Options(gameConfiguration, 165).Select(o => o.PowerUpDefinition?.TargetAttribute), Is.EqualTo(new[] { Stats.MaximumHealth, Stats.MaximumMana, Stats.MaximumAbility }));
        Assert.That(Options(gameConfiguration, 213).Single(o => o.Number == 29).PowerUpDefinition, Is.Null);
    }

    /// <summary>
    /// Applying the update twice changes nothing, and an operator's changes are kept.
    /// </summary>
    [Test]
    public async Task UpdateIsIdempotentAndKeepsOperatorEditsAsync()
    {
        var (context, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(AbilityCoreFeaturePlugIn).GUID);
        var configuration = plugIn.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())!;
        configuration.SkillSlotLevels[0] = 1150;
        plugIn.SetConfiguration(configuration, null);
        plugIn.IsActive = false;
        var minimum = Options(gameConfiguration, 213).Single(o => o.Number == 19);
        minimum.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value = 99;
        var definitions = gameConfiguration.ItemOptions.Count;

        await new AddSeason21AbilityCoreUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.ItemOptions, Has.Count.EqualTo(definitions));
        Assert.That(plugIn.GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())!.SkillSlotLevels[0], Is.EqualTo(1150));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(minimum.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(99));
    }

    private static List<IncreasableItemOption> Options(GameConfiguration gameConfiguration, short number)
        => gameConfiguration.Items.Single(i => i.Group == CardGroup && i.Number == number).PossibleItemOptions
            .SelectMany(o => o.PossibleOptions)
            .Where(o => o.OptionType == Season21ItemOptionTypes.AbilityCardOption)
            .ToList();

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateWithCoreAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21AbilityCoreUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason6Async()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        return (context, gameConfiguration);
    }
}
