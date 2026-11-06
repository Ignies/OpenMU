// <copyright file="Season21EventsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the updates of the party instances and events of Season 21: pending on a new database, the data after the
/// update, and a second run which changes nothing.
/// </summary>
[TestFixture]
internal class Season21EventsDataTest
{
    private IContext _context = null!;

    private GameConfiguration _gameConfiguration = null!;

    /// <summary>
    /// Creates a Season 6 database with the Season 21 data, then applies the updates of the events.
    /// </summary>
    [OneTimeSetUp]
    public async Task SetupAsync()
    {
        (this._context, this._gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await Season21EvolutionQuestsDataTest.ApplySeason21Async(this._context, this._gameConfiguration).ConfigureAwait(false);
        foreach (var update in Updates())
        {
            await update.ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// A new database leaves the updates pending: no event of Season 21.
    /// </summary>
    [Test]
    public async Task NewDatabaseLeavesTheUpdatesPendingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key).ToList();
        Assert.That(installed, Has.None.AnyOf(Updates().Select(u => u.Key).ToArray()));
        Assert.That(gameConfiguration.MiniGameDefinitions.Where(d => d.Type >= MiniGameType.PartyInstance), Is.Empty);
    }

    /// <summary>
    /// The trial of the class quests is a party instance of the Trial Area, entered at gate 540 of the data.
    /// </summary>
    [Test]
    public void TrialIsAPartyInstance()
    {
        var trial = this._gameConfiguration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.PartyInstance && d.GameLevel == 2);
        Assert.That(trial.Entrance?.Map?.Number, Is.EqualTo(EvolutionQuestConfiguration.Season21TrialMap));
        Assert.That((trial.Entrance!.X1, trial.Entrance.Y1, trial.Entrance.X2, trial.Entrance.Y2), Is.EqualTo(((byte)147, (byte)27, (byte)148, (byte)28)));
        Assert.That(trial.MapCreationPolicy, Is.EqualTo(MiniGameMapCreationPolicy.OnePerParty));
        Assert.That(trial.MaximumPlayerCount, Is.EqualTo(5));
        Assert.That(trial.Entrance.Map!.ExitGates, Does.Contain(trial.Entrance));
    }

    /// <summary>
    /// Applying the updates again changes nothing.
    /// </summary>
    [Test]
    public async Task SecondRunChangesNothingAsync()
    {
        var games = this._gameConfiguration.MiniGameDefinitions.Count;
        var gates = this._gameConfiguration.Maps.Sum(m => m.ExitGates.Count);
        var spawns = this._gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count);
        var maps = this._gameConfiguration.Maps.Count;
        var monsters = this._gameConfiguration.Monsters.Count;

        foreach (var update in Updates())
        {
            await update.ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        }

        Assert.That(this._gameConfiguration.MiniGameDefinitions, Has.Count.EqualTo(games));
        Assert.That(this._gameConfiguration.Maps.Sum(m => m.ExitGates.Count), Is.EqualTo(gates));
        Assert.That(this._gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count), Is.EqualTo(spawns));
        Assert.That(this._gameConfiguration.Maps, Has.Count.EqualTo(maps));
        Assert.That(this._gameConfiguration.Monsters, Has.Count.EqualTo(monsters));
    }

    private static IEnumerable<UpdatePlugInBase> Updates() =>
    [
        new AddSeason21EvolutionQuestsUpdatePlugIn(),
        new AddSeason21InstancesUpdatePlugIn(),
    ];

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
