// <copyright file="Season21NewPowerDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.GameLogic.NewPower;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the <see cref="AddSeason21NewPowerUpdatePlugIn"/>: the fifth class "New Power" skills, effects, card options
/// and the trial steps of its quests.
/// </summary>
[TestFixture]
internal class Season21NewPowerDataTest
{
    private const byte CardGroup = 20;

    /// <summary>
    /// The update is mandatory and runs on a new database; a Season 6 world without the core stays as it is.
    /// </summary>
    [Test]
    public async Task NewSeasonSixDatabaseIsInstalledAndUnchangedAsync()
    {
        var update = new AddSeason21NewPowerUpdatePlugIn();
        Assert.That((update.IsMandatory, update.RunsOnNewDatabase), Is.EqualTo((true, true)));

        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key);
        Assert.That(installed, Has.Some.EqualTo(update.Key));
        Assert.That(gameConfiguration.Skills.Any(s => s.Number is >= 2086 and <= 2088), Is.False);
        Assert.That(gameConfiguration.MagicEffects.Any(e => e.Number is >= 421 and <= 423), Is.False);
    }

    /// <summary>
    /// With the core, the three New Power skills exist with their effects (client groups 334-336 don't fit the byte sub type), and each extra damage
    /// card gives its skill.
    /// </summary>
    /// <param name="card">The card number.</param>
    /// <param name="skill">The skill number.</param>
    /// <param name="effect">The effect number.</param>
    [TestCase((short)235, (short)2086, (short)421)]
    [TestCase((short)236, (short)2087, (short)422)]
    [TestCase((short)237, (short)2088, (short)423)]
    public async Task CardsGiveTheSkillsWithTheirEffectsAsync(short card, short skill, short effect)
    {
        var (_, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);

        var definition = gameConfiguration.Skills.Single(s => s.Number == skill);
        Assert.That(definition.SkillType, Is.EqualTo(SkillType.Other));
        Assert.That(definition.MagicEffectDef?.Number, Is.EqualTo(effect));
        Assert.That(definition.MagicEffectDef!.InformObservers, Is.True);
        Assert.That(CoreConfiguration(gameConfiguration).Cards.Single(c => c.ItemNumber == card).Skill, Is.EqualTo(skill));
    }

    /// <summary>
    /// The 18 options of the cards which need the New Power take effect on its attributes; rates are fractions.
    /// </summary>
    /// <param name="card">A card which carries the option.</param>
    /// <param name="option">The option.</param>
    /// <param name="attribute">The name of the attribute in <see cref="Stats"/>.</param>
    /// <param name="levelFive">The value at card level 5.</param>
    [TestCase((short)213, (short)29, "NukeAttackIncrease", 273.75f)]
    [TestCase((short)213, (short)30, "BoltAttackIncrease", 182.5f)]
    [TestCase((short)213, (short)31, "WideAttackIncrease", 91.25f)]
    [TestCase((short)214, (short)68, "NukeCriticalChance", 0.025f)]
    [TestCase((short)214, (short)69, "BoltCriticalChance", 0.01667f)]
    [TestCase((short)214, (short)70, "WideCriticalChance", 0.00833f)]
    [TestCase((short)228, (short)71, "NukeExcellentChance", 0.12f)]
    [TestCase((short)228, (short)72, "BoltExcellentChance", 0.095f)]
    [TestCase((short)228, (short)73, "WideExcellentChance", 0.08f)]
    [TestCase((short)229, (short)74, "NukeDoubleChance", 0.07f)]
    [TestCase((short)229, (short)75, "BoltDoubleChance", 0.05f)]
    [TestCase((short)229, (short)76, "WideDoubleChance", 0.04f)]
    [TestCase((short)230, (short)77, "NukeTripleChance", 0.05f)]
    [TestCase((short)230, (short)78, "BoltTripleChance", 0.04f)]
    [TestCase((short)230, (short)79, "WideTripleChance", 0.03f)]
    [TestCase((short)235, (short)80, "NukeExtraDamage", 2400f)]
    [TestCase((short)236, (short)81, "BoltExtraDamage", 2400f)]
    [TestCase((short)237, (short)82, "WideExtraDamage", 2400f)]
    public async Task NewPowerOptionsTakeEffectAsync(short card, short option, string attribute, float levelFive)
    {
        var (context, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);
        var target = typeof(Stats).GetProperty(attribute)?.GetValue(null);

        var mapped = Options(gameConfiguration, card).Single(o => o.Number == option);
        Assert.That(mapped.PowerUpDefinition?.TargetAttribute, Is.EqualTo(target));
        Assert.That(mapped.LevelDependentOptions.Single(l => l.Level == 5).PowerUpDefinition!.Boost!.ConstantValue.Value, Is.EqualTo(levelFive).Within(0.0001));

        // An installed world whose options have no effect gets them from this update.
        mapped.PowerUpDefinition = null;
        mapped.LevelDependentOptions.Clear();
        await new AddSeason21NewPowerUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That(mapped.PowerUpDefinition?.TargetAttribute, Is.EqualTo(target));
    }

    /// <summary>
    /// The quests 10-12 kill Eunice within 60 s (S21Emu <c>QuestInstanceManager.xml</c> <c>Time="60000"</c>) and grant
    /// their extra damage effect for the trial (Webzen guide 338) on a world saved before it; an operator's other time stays.
    /// </summary>
    [Test]
    public async Task FifthClassTrialStepsLastAMinuteWithTheirPowerAsync()
    {
        var (context, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.First(c => c.TypeId == typeof(EvolutionQuestFeaturePlugIn).GUID);
        var configuration = plugIn.GetConfiguration<EvolutionQuestConfiguration>(new IdIgnoringReferenceHandler()) ?? new EvolutionQuestConfiguration();
        foreach (var step in configuration.Steps.Where(s => s.QuestNumber >= 10))
        {
            step.TimeLimit = TimeSpan.FromMinutes(10);
            step.PowerEffect = 0;
        }

        configuration.Steps.Single(s => s.QuestNumber == 7).TimeLimit = TimeSpan.FromMinutes(3);
        plugIn.SetConfiguration(configuration, null);

        await new AddSeason21NewPowerUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var updated = plugIn.GetConfiguration<EvolutionQuestConfiguration>(new IdIgnoringReferenceHandler())!;
        Assert.That(updated.Steps.Where(s => s.QuestNumber >= 10).Select(s => (s.QuestNumber, s.TimeLimit, s.PowerEffect)), Is.EqualTo(new[]
        {
            ((short)10, TimeSpan.FromSeconds(60), (short)421),
            ((short)11, TimeSpan.FromSeconds(60), (short)422),
            ((short)12, TimeSpan.FromSeconds(60), (short)423),
        }));
        Assert.That(updated.Steps.Single(s => s.QuestNumber == 7).TimeLimit, Is.EqualTo(TimeSpan.FromMinutes(3)));
    }

    /// <summary>
    /// The feature stays off: the update fills the data, the operator turns it on (Admin > Systems).
    /// </summary>
    [Test]
    public async Task FeatureStaysOffAndUpdateIsIdempotentAsync()
    {
        var (context, gameConfiguration) = await CreateWithCoreAsync().ConfigureAwait(false);
        var skills = gameConfiguration.Skills.Count;
        var effects = gameConfiguration.MagicEffects.Count;

        await new AddSeason21NewPowerUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.Skills, Has.Count.EqualTo(skills));
        Assert.That(gameConfiguration.MagicEffects, Has.Count.EqualTo(effects));
        var plugIn = gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeof(NewPowerFeaturePlugIn).GUID);
        Assert.That(plugIn?.IsActive ?? false, Is.False);
        Assert.That(typeof(IDisabledByDefault).IsAssignableFrom(typeof(NewPowerFeaturePlugIn)), Is.True);
    }

    private static AbilityCoreConfiguration CoreConfiguration(GameConfiguration gameConfiguration)
        => gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(AbilityCoreFeaturePlugIn).GUID)
            .GetConfiguration<AbilityCoreConfiguration>(new IdIgnoringReferenceHandler())!;

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
