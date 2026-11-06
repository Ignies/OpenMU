// <copyright file="Season21EventsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

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
    /// The Doppelganger is open to every level from 50, needs two players, and has the seven rooms with their chests.
    /// </summary>
    [Test]
    public void DoppelgangerHasTheSeason21Rooms()
    {
        var definitions = this._gameConfiguration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.Doppelganger).ToList();
        Assert.That(definitions.Select(d => (d.MinimumCharacterLevel, d.MaximumCharacterLevel, d.MinimumPlayerCount)), Has.All.EqualTo((50, int.MaxValue, 2)));
        var json = this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(GameLogic.MiniGames.Doppelganger.DoppelgangerFeaturePlugIn).GUID).CustomConfiguration;
        Assert.That(json, Does.Contain("240000").And.Contain("FinalChestItems"));
        var otherEventBags = this._gameConfiguration.MiniGameDefinitions.Where(d => d.Type != MiniGameType.Doppelganger).SelectMany(d => d.Rewards).Select(r => r.ItemReward).ToList();
        Assert.That(this._gameConfiguration.DropItemGroups.Count(g => g.Description.ValueInNeutralLanguage == "Event reward of Season 21" && !otherEventBags.Contains(g)), Is.EqualTo(14));
    }

    /// <summary>
    /// The Tormented Square is on map 102 with its fifteen rounds, and its timetable is switched on.
    /// </summary>
    [Test]
    public void TormentedSquareIsAdded()
    {
        var definition = this._gameConfiguration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.TormentedSquare);
        Assert.That(definition.Entrance?.Map?.Number, Is.EqualTo(102));
        Assert.That(definition.Entrance!.Map!.TerrainData, Has.Length.EqualTo(65539));
        Assert.That(definition.MaximumPlayerCount, Is.EqualTo(20));
        var events = this.Events();
        Assert.That(events.TormentedSquare?.Rounds, Has.Count.EqualTo(15));
        Assert.That(this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(GameLogic.MiniGames.Season21.TormentedSquareStartPlugIn).GUID).IsActive, Is.True);
    }

    /// <summary>
    /// The Instance Zone holds the entrances; the town NPCs of the trials lead there, Cent and Eunice into the trial, the
    /// Portal of Dimension into the labyrinth.
    /// </summary>
    [Test]
    public void InstanceZoneHoldsTheEntrances()
    {
        var zone = this._gameConfiguration.Maps.Single(m => m.Number == 114);
        Assert.That(zone.MonsterSpawns.Select(s => s.MonsterDefinition?.Number), Is.SupersetOf(new short?[] { 758, 765, 522, 940 }));
        var quests = this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(GameLogic.EvolutionQuests.EvolutionQuestFeaturePlugIn).GUID)
            .GetConfiguration<GameLogic.EvolutionQuests.EvolutionQuestConfiguration>(new IdIgnoringReferenceHandler())!;
        Assert.That(quests.Entrances.Single(e => e.NpcNumber == 764).Map, Is.EqualTo(114));
        Assert.That(quests.Entrances.Single(e => e.NpcNumber == 765).Map, Is.EqualTo(121));
        Assert.That(quests.Entrances.Single(e => e.NpcNumber == 940).Map, Is.EqualTo(121));
        var labyrinth = this._gameConfiguration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.LabyrinthOfDimensions);
        Assert.That((labyrinth.Entrance?.Map?.Number, labyrinth.MapCreationPolicy, labyrinth.MaximumPlayerCount), Is.EqualTo(((short?)115, MiniGameMapCreationPolicy.OnePerPlayer, 1)));
        Assert.That(this.Events().Labyrinth?.Rounds, Has.Count.EqualTo(41));
    }

    /// <summary>
    /// The Boss Battle is on map 139 with its bosses, the Protector of Acheron on the event copies 92 and 96 with its
    /// reward bag, both scheduled; every day of the Imperial Guardian starts at total level 1000.
    /// </summary>
    [Test]
    public void BossBattleAcheronAndImperialGuardianAreSet()
    {
        var boss = this._gameConfiguration.MiniGameDefinitions.Single(d => d.Type == MiniGameType.BossBattle);
        Assert.That((boss.Entrance?.Map?.Number, boss.EntranceFee, boss.MinimumCharacterLevel), Is.EqualTo(((short?)139, 10_000_000, 800)));
        Assert.That(this._gameConfiguration.Monsters.Where(m => m.Number is >= 919 and <= 924).Select(m => m.Number), Is.EquivalentTo(new short[] { 919, 921, 922, 923, 924 }));
        Assert.That(this.Events().BossBattle?.Bosses, Has.Count.EqualTo(5));
        var acheron = this._gameConfiguration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.AcheronGuardian).OrderBy(d => d.GameLevel).ToList();
        Assert.That(acheron.Select(d => d.Entrance?.Map?.Number), Is.EqualTo(new short?[] { 92, 96 }));
        Assert.That(acheron.Select(d => d.Entrance!.Map!.TerrainData?.Length), Has.All.EqualTo(65539));
        Assert.That(acheron.Select(d => d.Rewards.Single().ItemReward?.PossibleItems.Count), Has.All.GreaterThan(0));
        Assert.That(this.Events().AcheronGuardian, Has.Count.EqualTo(2));
        foreach (var typeId in new[] { typeof(GameLogic.MiniGames.Season21.BossBattleStartPlugIn).GUID, typeof(GameLogic.MiniGames.Season21.AcheronGuardianStartPlugIn).GUID })
        {
            Assert.That(this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeId).IsActive, Is.True);
        }

        Assert.That(this._gameConfiguration.MiniGameDefinitions.Where(d => d.Type == MiniGameType.ImperialGuardian).Select(d => d.MinimumCharacterLevel), Has.All.EqualTo(1000));
        var crywolf = this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(GameLogic.Crywolf.CrywolfPlugIn).GUID)
            .GetConfiguration<GameLogic.Crywolf.CrywolfEventDefinition>(new GameConfigurationReferenceHandler(this._gameConfiguration))!;
        Assert.That(crywolf.WeeklyElements, Has.Count.EqualTo(5));
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
        var groups = this._gameConfiguration.DropItemGroups.Count;

        foreach (var update in Updates())
        {
            await update.ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
        }

        Assert.That(this._gameConfiguration.MiniGameDefinitions, Has.Count.EqualTo(games));
        Assert.That(this._gameConfiguration.Maps.Sum(m => m.ExitGates.Count), Is.EqualTo(gates));
        Assert.That(this._gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count), Is.EqualTo(spawns));
        Assert.That(this._gameConfiguration.Maps, Has.Count.EqualTo(maps));
        Assert.That(this._gameConfiguration.Monsters, Has.Count.EqualTo(monsters));
        Assert.That(this._gameConfiguration.DropItemGroups, Has.Count.EqualTo(groups));
    }

    private GameLogic.MiniGames.Season21.Season21EventsConfiguration Events()
        => this._gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(GameLogic.MiniGames.Season21.Season21EventsFeaturePlugIn).GUID)
            .GetConfiguration<GameLogic.MiniGames.Season21.Season21EventsConfiguration>(new IdIgnoringReferenceHandler())!;

    private static IEnumerable<UpdatePlugInBase> Updates() =>
    [
        new AddSeason21EvolutionQuestsUpdatePlugIn(),
        new AddSeason21InstancesUpdatePlugIn(),
        new AddSeason21DoppelgangerUpdatePlugIn(),
        new AddSeason21TormentedSquareUpdatePlugIn(),
        new AddSeason21InstanceZoneUpdatePlugIn(),
        new AddSeason21BossBattleUpdatePlugIn(),
        new AddSeason21AcheronGuardianUpdatePlugIn(),
        new AddSeason21ImperialGuardianReworkUpdatePlugIn(),
        new AddSeason21CrywolfUpdatePlugIn(),
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
