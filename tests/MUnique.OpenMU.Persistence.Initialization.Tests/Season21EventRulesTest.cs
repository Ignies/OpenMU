// <copyright file="Season21EventRulesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the <see cref="AddSeason21EventRulesUpdatePlugIn"/>: the event grades of Season 21 by total level and its
/// party experience bonus.
/// </summary>
[TestFixture]
internal class Season21EventRulesTest
{
    /// <summary>
    /// Blood Castle, Devil Square and Chaos Castle get the seven grades of Season 21 by total level, the same for
    /// every class and without the master class requirement; the eighth Blood Castle is closed.
    /// </summary>
    /// <param name="type">The mini game type.</param>
    /// <param name="level">The game level.</param>
    /// <param name="minimum">The expected minimum total level.</param>
    /// <param name="maximum">The expected maximum total level.</param>
    [TestCase(MiniGameType.BloodCastle, 1, 50, 399)]
    [TestCase(MiniGameType.BloodCastle, 2, 400, 799)]
    [TestCase(MiniGameType.BloodCastle, 3, 800, 999)]
    [TestCase(MiniGameType.DevilSquare, 6, 1400, 1599)]
    [TestCase(MiniGameType.ChaosCastle, 7, 1600, int.MaxValue)]
    [TestCase(MiniGameType.IllusionTemple, 3, 800, 1399)]
    [TestCase(MiniGameType.IllusionTemple, 5, 1600, int.MaxValue)]
    public async Task EventsGetTheSeason21GradesAsync(MiniGameType type, int level, int minimum, int maximum)
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21EventRulesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var definition = gameConfiguration.MiniGameDefinitions.Single(d => d.Type == type && d.GameLevel == level);
        Assert.That((definition.MinimumCharacterLevel, definition.MaximumCharacterLevel), Is.EqualTo((minimum, maximum)));
        Assert.That((definition.MinimumSpecialCharacterLevel, definition.MaximumSpecialCharacterLevel), Is.EqualTo((minimum, maximum)));
        Assert.That(definition.RequiresMasterClass, Is.False);
        Assert.That(gameConfiguration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.BloodCastle && d.GameLevel == 8).IsActive, Is.False);
        Assert.That(gameConfiguration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.IllusionTemple && d.GameLevel == 6).IsActive, Is.False);
    }

    /// <summary>
    /// The update configures the party experience bonus of Season 21, and keeps a grade the operator changed when it
    /// runs again.
    /// </summary>
    [Test]
    public async Task UpdateSetsThePartyBonusAndKeepsChangedGradesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var update = new AddSeason21EventRulesUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var grade = gameConfiguration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.BloodCastle && d.GameLevel == 4);
        grade.MaximumCharacterLevel = 1150;
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(grade.MaximumCharacterLevel, Is.EqualTo(1150));
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(PartyExperienceBonusPlugIn).GUID);
        Assert.That(plugIn.IsActive, Is.True);
        var configuration = plugIn.GetConfiguration<PartyExperienceBonusConfiguration>(null);
        Assert.That(configuration!.GetBonus(5, 3), Is.EqualTo(6.4).Within(1e-9));
    }

    /// <summary>
    /// The update sets the daily entry limits of S21Emu Event/EventEntryLimit.dat, and keeps limits the operator set.
    /// </summary>
    [Test]
    public async Task UpdateSetsTheDailyEntryLimitsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var update = new AddSeason21EventRulesUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(Season21EventsFeaturePlugIn).GUID);
        var limits = plugIn.GetConfiguration<Season21EventsConfiguration>(new IdIgnoringReferenceHandler())!.DailyEntryLimits;
        Assert.That(limits, Has.Count.EqualTo(8));
        Assert.That(limits.Single(l => l.Type == MiniGameType.BloodCastle).Limit, Is.EqualTo(15));
        Assert.That(limits.Single(l => l.Type == MiniGameType.LabyrinthOfDimensions).Limit, Is.EqualTo(10));

        var configuration = plugIn.GetConfiguration<Season21EventsConfiguration>(new IdIgnoringReferenceHandler())!;
        configuration.DailyEntryLimits = [new EventEntryLimit { Type = MiniGameType.BloodCastle, Limit = 3 }];
        plugIn.SetConfiguration(configuration, null);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That(plugIn.GetConfiguration<Season21EventsConfiguration>(new IdIgnoringReferenceHandler())!.DailyEntryLimits.Single().Limit, Is.EqualTo(3));
    }

    /// <summary>
    /// The update sets the Castle Siege registration of S21Emu (GameServerInfo - Event.dat: CastleSiegeMinMemberToReg=3,
    /// CastleSiegeMasterMinLevelToReg=400) over the Season 6 rules (20 members, level 200), which stay without it; a
    /// value the operator changed stays when the update runs again.
    /// </summary>
    [Test]
    public async Task UpdateSetsTheCastleSiegeRegistrationRulesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var siege = gameConfiguration.CastleSiegeConfiguration!;
        Assert.That((siege.RegisterMinMembers, siege.RegisterMinLevel), Is.EqualTo((20, 200)));
        var update = new AddSeason21EventRulesUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That((siege.RegisterMinMembers, siege.RegisterMinLevel), Is.EqualTo((3, 400)));

        siege.RegisterMinMembers = 5;
        siege.RegisterMinLevel = 600;
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That((siege.RegisterMinMembers, siege.RegisterMinLevel), Is.EqualTo((5, 600)));
        Assert.That(update.Version, Is.EqualTo(3));
        Assert.That(update.DependsOn.Select(d => d.Key), Does.Contain(new AddCastleSiegeDataUpdatePlugIn().Key));
    }

    /// <summary>
    /// The event rules need the total levels of Season 21, so they depend on its level caps.
    /// </summary>
    [Test]
    public void EventRulesDependOnTheLevelCaps()
    {
        Assert.That(new AddSeason21EventRulesUpdatePlugIn().DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21LevelCapsUpdatePlugIn().Key));
        Assert.That(new AddSeason21EventRulesUpdatePlugIn().IsMandatory, Is.False);
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
