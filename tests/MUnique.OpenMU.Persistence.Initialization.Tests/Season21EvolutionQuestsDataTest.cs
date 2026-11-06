// <copyright file="Season21EvolutionQuestsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the <see cref="AddSeason21EvolutionQuestsUpdatePlugIn"/>: the Trial Area, Sent and Eunice and the fourth and
/// fifth class quests of every class.
/// </summary>
[TestFixture]
internal class Season21EvolutionQuestsDataTest
{
    private IContext _context = null!;

    private GameConfiguration _gameConfiguration = null!;

    /// <summary>
    /// Creates a Season 6 database with the Season 21 maps, classes and items, then applies the update.
    /// </summary>
    [OneTimeSetUp]
    public async Task SetupAsync()
    {
        (this._context, this._gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await ApplySeason21Async(this._context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21EvolutionQuestsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);
    }

    /// <summary>
    /// A new database leaves the update pending: no quest of the group, no Trial Area.
    /// </summary>
    [Test]
    public async Task NewDatabaseLeavesTheUpdatePendingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key);
        Assert.That(installed, Has.None.EqualTo(new AddSeason21EvolutionQuestsUpdatePlugIn().Key));
        Assert.That(gameConfiguration.Monsters.SelectMany(m => m.Quests).Where(q => q.Group == EvolutionQuestConfiguration.Season21QuestGroup), Is.Empty);
        Assert.That(gameConfiguration.Maps.Where(m => m.Number == EvolutionQuestConfiguration.Season21TrialMap), Is.Empty);
    }

    /// <summary>
    /// The update depends on the maps, classes and items of Season 21.
    /// </summary>
    [Test]
    public void UpdateDependsOnTheSeason21Data()
    {
        Assert.That(
            new AddSeason21EvolutionQuestsUpdatePlugIn().DependsOn.Select(d => d.Key),
            Is.EquivalentTo(new[] { typeof(AddSeason21MapsUpdatePlugIn).GUID, typeof(AddSeason21ClassesUpdatePlugIn).GUID, typeof(AddSeason21ItemCatalogUpdatePlugIn).GUID }));
    }

    /// <summary>
    /// The Trial Area is added with its terrain, hosted with Lorencia, and Sent and Eunice stand on it and in their towns.
    /// </summary>
    [Test]
    public async Task TrialAreaAndNpcsAreAddedAsync()
    {
        var trialArea = this._gameConfiguration.Maps.Single(m => m.Number == EvolutionQuestConfiguration.Season21TrialMap);
        Assert.That(trialArea.TerrainData, Has.Length.EqualTo(65539));
        Assert.That(trialArea.SafezoneMap?.Number, Is.EqualTo(0));
        var servers = await this._context.GetAsync<GameServerConfiguration>().ConfigureAwait(false);
        Assert.That(servers.Where(s => s.Maps.Any(m => m.Number == 0)).Select(s => s.Maps.Contains(trialArea)), Has.All.True);

        AssertSpawn(0, 764, 131, 147);
        AssertSpawn(2, 939, 193, 11);
        AssertSpawn(EvolutionQuestConfiguration.Season21TrialMap, 766, 146, 30);
        AssertSpawn(EvolutionQuestConfiguration.Season21TrialMap, 941, 103, 29);
        Assert.That(this.Monster(766).NpcWindow, Is.EqualTo(NpcWindow.NpcDialog));
        Assert.That(this.Monster(941).NpcWindow, Is.EqualTo(NpcWindow.NpcDialog));
        Assert.That(this.Monster(764).NpcWindow, Is.EqualTo(NpcWindow.Undefined));
        Assert.That(this.Monster(763).Attributes.Single(a => a.AttributeDefinition == GameLogic.Attributes.Stats.MaximumHealth).Value, Is.EqualTo(1000000));

        void AssertSpawn(byte map, short monster, byte x, byte y)
        {
            var spawn = this._gameConfiguration.Maps.Single(m => m.Number == map && m.Discriminator == 0).MonsterSpawns.Single(s => s.MonsterDefinition?.Number == monster);
            Assert.That((spawn.X1, spawn.Y1, spawn.X2, spawn.Y2), Is.EqualTo((x, y, x, y)), monster.ToString());
        }
    }

    /// <summary>
    /// Every class whose next class is a fourth evolution gets the quests 7 to 9 from Sent, every fourth evolution the
    /// quests 10 to 12 from Eunice; the last one of each evolves.
    /// </summary>
    [Test]
    public void EveryClassGetsItsChains()
    {
        var thirds = this._gameConfiguration.CharacterClasses.Where(c => c.NextGenerationClass?.GetEvolution() == ClassEvolution.Fourth).ToList();
        var fourths = this._gameConfiguration.CharacterClasses.Where(c => c.NextGenerationClass?.GetEvolution() == ClassEvolution.Fifth).ToList();
        Assert.That(thirds, Has.Count.EqualTo(16));
        Assert.That(fourths, Has.Count.EqualTo(16));

        foreach (var characterClass in thirds)
        {
            var chain = this.Monster(766).Quests.Where(q => Equals(q.QualifiedCharacter, characterClass)).OrderBy(q => q.Number).ToList();
            Assert.That(chain.Select(q => q.Number), Is.EqualTo(new short[] { 7, 8, 9 }), characterClass.Name.ValueInNeutralLanguage);
            Assert.That(chain[0].GetChainEvolution(), Is.EqualTo(ClassEvolution.Fourth));
            Assert.That(chain.Select(q => q.Rewards.Single(r => r.RewardType == QuestRewardType.LevelUpPoints).Value), Is.EqualTo(new[] { 20, 30, 50 }));
            Assert.That(chain.Select(q => q.Rewards.Any(r => r.RewardType == QuestRewardType.CharacterEvolution)), Is.EqualTo(new[] { false, false, true }));
            Assert.That(chain[1].RequiredMonsterKills.Select(k => (k.Monster!.Number, k.MinimumNumber)), Is.EquivalentTo(new[] { ((short)767, 5), ((short)770, 5), ((short)771, 5) }));
            Assert.That(chain[2].Rewards.Single(r => r.RewardType == QuestRewardType.Item).ItemReward!.Definition!.Name.ValueInNeutralLanguage, Is.EqualTo("Scroll of Gray Oblivion"));
        }

        foreach (var characterClass in fourths)
        {
            var chain = this.Monster(941).Quests.Where(q => Equals(q.QualifiedCharacter, characterClass)).OrderBy(q => q.Number).ToList();
            Assert.That(chain.Select(q => q.Number), Is.EqualTo(new short[] { 10, 11, 12 }), characterClass.Name.ValueInNeutralLanguage);
            Assert.That(chain[0].GetChainEvolution(), Is.EqualTo(ClassEvolution.Fifth));
            Assert.That(chain[2].Rewards.Single(r => r.RewardType == QuestRewardType.LevelUpPoints).Value, Is.EqualTo(200));
            Assert.That(chain.Select(q => q.Rewards.Any(r => r.RewardType == QuestRewardType.CharacterEvolution)), Is.EqualTo(new[] { false, false, true }));
        }
    }

    /// <summary>
    /// Each monster of the default trial configuration exists and has a walkable tile within its spawn range.
    /// </summary>
    [Test]
    public void TrialMonstersCanSpawn()
    {
        var configuration = new EvolutionQuestConfiguration();
        var trialArea = this._gameConfiguration.Maps.Single(m => m.Number == EvolutionQuestConfiguration.Season21TrialMap);
        var terrain = new GameMapTerrain(trialArea);
        foreach (var spawn in configuration.Steps.SelectMany(s => s.Spawns))
        {
            Assert.That(this._gameConfiguration.Monsters.Any(m => m.Number == spawn.MonsterNumber), spawn.MonsterNumber.ToString());
            var walkable = Enumerable.Range(spawn.X - 3, 7).SelectMany(x => Enumerable.Range(spawn.Y - 3, 7).Select(y => (x, y))).Any(p => terrain.WalkMap[p.x, p.y]);
            Assert.That(walkable, $"{spawn.MonsterNumber} at {spawn.X}/{spawn.Y}");
        }

        foreach (var entrance in configuration.Entrances)
        {
            Assert.That(terrain.WalkMap[entrance.X1, entrance.Y1], entrance.NpcNumber.ToString());
        }
    }

    /// <summary>
    /// A second run adds nothing.
    /// </summary>
    [Test]
    public async Task SecondRunChangesNothingAsync()
    {
        var quests = this._gameConfiguration.Monsters.Sum(m => m.Quests.Count);
        var spawns = this._gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count);
        var maps = this._gameConfiguration.Maps.Count;

        await new AddSeason21EvolutionQuestsUpdatePlugIn().ApplyUpdateAsync(this._context, this._gameConfiguration).ConfigureAwait(false);

        Assert.That(this._gameConfiguration.Monsters.Sum(m => m.Quests.Count), Is.EqualTo(quests));
        Assert.That(this._gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count), Is.EqualTo(spawns));
        Assert.That(this._gameConfiguration.Maps, Has.Count.EqualTo(maps));
    }

    /// <summary>
    /// Applies the updates of Season 21 which the quests depend on.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    internal static async Task ApplySeason21Async(IContext context, GameConfiguration gameConfiguration)
    {
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
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

    private MonsterDefinition Monster(short number) => this._gameConfiguration.Monsters.Single(m => m.Number == number);
}
