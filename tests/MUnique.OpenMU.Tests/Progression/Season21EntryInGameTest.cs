// <copyright file="Season21EntryInGameTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EvolutionQuests;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.PlugIns;
using Moq;

/// <summary>
/// Every way onto a Season 21 map in a world whose caps are below the Season 21 levels, in each progression mode: the
/// warp list (Sanctuary of Aquilas, warp 75, total level 1600), the gate (719 from Kanturu Ruins) and the NPC teleport
/// (Sent 764 to the trial of the fourth evolution, total level 800). The requirement is the mode's, never the raw
/// Season 21 total, and a refusal says why.
/// </summary>
[TestFixture]
public class Season21EntryInGameTest
{
    private const int AquilasTotal = 1600;
    private const int FourthEvolutionTotal = 800;
    private const byte Aquilas = 145;
    private const byte KanturuRuins = 37;

    private GameConfiguration _gameConfiguration = null!;

    private GameContext _gameContext = null!;

    private ProgressionFeaturePlugIn _progression = null!;

    /// <summary>
    /// Initializes the Season 6 data with the Season 21 caps, maps, classes and evolution quests.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        using var context = contextProvider.CreateNewConfigurationContext();
        this._gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21EvolutionQuestsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);

        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var plugInManager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        this._gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), plugInManager, NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = this._gameContext.PlugInManager;
        mapInitializer.PathFinderPool = this._gameContext.PathFinderPool;

        var evolutionQuests = new EvolutionQuestFeaturePlugIn();
        this._gameContext.FeaturePlugIns.AddPlugIn(evolutionQuests, true);
        plugInManager.RegisterPlugInAtPlugInPoint<IPlayerTalkToNpcPlugIn>(evolutionQuests);
        this._progression = new ProgressionFeaturePlugIn();
        this._gameContext.FeaturePlugIns.AddPlugIn(this._progression, true);
    }

    /// <summary>
    /// Warp 75 asks the mode's requirement of total level 1600: met, the hero arrives; one short, it stays and is told
    /// what is missing.
    /// </summary>
    /// <param name="mode">The progression mode.</param>
    [TestCase(ProgressionMode.Original)]
    [TestCase(ProgressionMode.Normalized)]
    [TestCase(ProgressionMode.Stars)]
    public async ValueTask WarpListFollowsTheModeAsync(ProgressionMode mode)
    {
        var (met, below, missing) = this.ForMap(mode, AquilasTotal);
        var warp = this._gameConfiguration.WarpList.Single(w => w.Index == 75);

        var refused = await this.CreatePlayerAsync(0, 132, 148, below).ConfigureAwait(false);
        await new WarpAction().WarpToAsync(refused, warp).ConfigureAwait(false);
        await ArriveAsync(refused).ConfigureAwait(false);
        Assert.That(refused.CurrentMap?.Definition.Number, Is.EqualTo(0));
        AssertToldWhy(refused, missing);

        var player = await this.CreatePlayerAsync(0, 132, 148, met).ConfigureAwait(false);
        await new WarpAction().WarpToAsync(player, warp).ConfigureAwait(false);
        await ArriveAsync(player).ConfigureAwait(false);
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(Aquilas));
    }

    /// <summary>
    /// Gate 719 in Kanturu Ruins asks the same requirement as the warp.
    /// </summary>
    /// <param name="mode">The progression mode.</param>
    [TestCase(ProgressionMode.Original)]
    [TestCase(ProgressionMode.Normalized)]
    [TestCase(ProgressionMode.Stars)]
    public async ValueTask GateFollowsTheModeAsync(ProgressionMode mode)
    {
        var (met, below, missing) = this.ForMap(mode, AquilasTotal);
        var gate = this._gameConfiguration.Maps.Single(m => m.Number == KanturuRuins && m.Discriminator == 0).EnterGates.Single(g => g.Number == 719);

        var refused = await this.CreatePlayerAsync(KanturuRuins, 124, 171, below).ConfigureAwait(false);
        await new WarpGateAction().EnterGateAsync(refused, gate).ConfigureAwait(false);
        await ArriveAsync(refused).ConfigureAwait(false);
        Assert.That(refused.CurrentMap?.Definition.Number, Is.EqualTo(KanturuRuins));
        AssertToldWhy(refused, missing);

        var player = await this.CreatePlayerAsync(KanturuRuins, 124, 171, met).ConfigureAwait(false);
        await new WarpGateAction().EnterGateAsync(player, gate).ConfigureAwait(false);
        await ArriveAsync(player).ConfigureAwait(false);
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(Aquilas));
    }

    /// <summary>
    /// Sent 764 takes a third class hero to the trial at the mode's requirement of total level 800.
    /// </summary>
    /// <param name="mode">The progression mode.</param>
    [TestCase(ProgressionMode.Original)]
    [TestCase(ProgressionMode.Normalized)]
    [TestCase(ProgressionMode.Stars)]
    public async ValueTask NpcTeleportFollowsTheModeAsync(ProgressionMode mode)
    {
        var (met, below) = this.ForEvolution(mode, FourthEvolutionTotal);

        var refused = await this.CreatePlayerAsync(0, 132, 148, below).ConfigureAwait(false);
        await TalkAsync(refused, 764).ConfigureAwait(false);
        Assert.That(refused.CurrentMap?.Definition.Number, Is.EqualTo(0));
        Mock.Get(refused.ViewPlugIns.GetPlugIn<IShowMessageOfObjectPlugIn>()!)
            .Verify(v => v.ShowMessageOfObjectAsync(It.Is<string>(m => !string.IsNullOrWhiteSpace(m)), It.IsAny<IIdentifiable>()), Times.Once);

        var player = await this.CreatePlayerAsync(0, 132, 148, met).ConfigureAwait(false);
        await TalkAsync(player, 764).ConfigureAwait(false);
        Assert.That(player.CurrentMap?.Definition.Number, Is.EqualTo(EvolutionQuestConfiguration.Season21TrialMap));
    }

    private static void AssertToldWhy(Player player, string missing)
    {
        Mock.Get(player.ViewPlugIns.GetPlugIn<IShowMessagePlugIn>()!)
            .Verify(v => v.ShowMessageAsync(It.Is<string>(m => m.Contains(missing)), MessageType.BlueNormal), Times.Once);
    }

    private static async ValueTask ArriveAsync(Player player)
    {
        if (player.PlayerState.CurrentState == PlayerState.ChangingMap || player.CurrentMap is null)
        {
            await player.ClientReadyAfterMapChangeAsync().ConfigureAwait(false);
        }
    }

    private static async ValueTask TalkAsync(Player player, short npcNumber)
    {
        var npc = player.CurrentMap!.GetNpcsInRange(player.Position, 255).First(n => n.Definition.Number == npcNumber);
        await new TalkNpcAction().TalkToNpcAsync(player, npc).ConfigureAwait(false);
        await ArriveAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the mode on a world capped at level 400 and master level 200 (none with stars).
    /// </summary>
    private ProgressionConfiguration SetMode(ProgressionMode mode)
    {
        var configuration = new ProgressionConfiguration { Mode = mode, MasterLevelCap = mode == ProgressionMode.Stars ? null : 200 };
        this._progression.Configuration = configuration;
        this._progression.GetWorld(this._gameConfiguration);
        Assume.That(this._gameConfiguration.MaximumLevel, Is.EqualTo(400));
        Assume.That(this._gameConfiguration.MaximumMasterLevel, Is.EqualTo(mode == ProgressionMode.Stars ? 0 : 200));
        return configuration;
    }

    /// <summary>
    /// Gets a hero which meets the map requirement of a Season 21 total in the mode, one which is one step short, and
    /// the text which names what the short one misses.
    /// </summary>
    private (Hero Met, Hero Short, string Missing) ForMap(ProgressionMode mode, int season21Total)
    {
        var configuration = this.SetMode(mode);
        if (mode == ProgressionMode.Original)
        {
            var reachable = Math.Min(season21Total, 600);
            return (new Hero(400, reachable - 400, 0), new Hero(400, reachable - 401, 0), $"Total Level {reachable}");
        }

        var requirement = ProgressionMapping.GetRequirement(season21Total, configuration);
        var met = new Hero(requirement.Level, requirement.MasterLevel, requirement.Stars);
        return mode == ProgressionMode.Normalized
            ? (met, met with { MasterLevel = requirement.MasterLevel - 1 }, $"Master Level {requirement.MasterLevel}")
            : (met, met with { Stars = requirement.Stars - 1 }, $"Resets {requirement.Stars}");
    }

    /// <summary>
    /// Gets a hero which meets an evolution's Season 21 total in the mode and one which is one level short.
    /// </summary>
    private (Hero Met, Hero Short) ForEvolution(ProgressionMode mode, int season21Total)
    {
        var configuration = this.SetMode(mode);
        if (mode == ProgressionMode.Original)
        {
            var reachable = Math.Min(season21Total, 600);
            return (new Hero(400, reachable - 400, 0), new Hero(400, reachable - 401, 0));
        }

        var level = EvolutionRules.GetRequiredLevel(season21Total, configuration);
        Assume.That(level, Is.LessThanOrEqualTo(400));
        return (new Hero(level, 0, 0), new Hero(level - 1, 0, 0));
    }

    private async ValueTask<Player> CreatePlayerAsync(byte map, byte x, byte y, Hero hero)
    {
        var player = new InGamePlayer(this._gameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        await new CreateCharacterAction().CreateCharacterAsync(player, player.Account.LoginName, (byte)CharacterClassNumber.DarkKnight).ConfigureAwait(false);
        var character = player.Account.Characters.Single();
        character.CharacterClass = this._gameConfiguration.CharacterClasses.Single(c => c.Number == (byte)CharacterClassNumber.BladeMaster);
        character.CurrentMap = this._gameConfiguration.Maps.Single(m => m.Number == map && m.Discriminator == 0);
        (character.PositionX, character.PositionY) = (x, y);
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        player.Attributes![Stats.Level] = hero.Level;
        player.Attributes[Stats.MasterLevel] = hero.MasterLevel;
        player.Attributes[Stats.Resets] = hero.Stars;
        player.TryAddMoney(10_000_000);
        return player;
    }

    private readonly record struct Hero(int Level, int MasterLevel, int Stars);

    private sealed class InGamePlayer : Player
    {
        public InGamePlayer(IGameContext gameContext)
            : base(gameContext)
        {
        }

        protected override ICustomPlugInContainer<IViewPlugIn> CreateViewPlugInContainer() => new MockViewPlugInContainer();
    }
}
