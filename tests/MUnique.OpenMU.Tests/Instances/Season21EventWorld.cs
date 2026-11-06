// <copyright file="Season21EventWorld.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Instances;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// An in-memory world with the Season 6 data, the Season 21 maps, classes and items, plus the updates a test asks for;
/// shared by the tests of the instances and events of Season 21.
/// </summary>
internal sealed class Season21EventWorld
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(15);

    private Season21EventWorld(GameConfiguration configuration, GameContext gameContext, PlugInManager plugInManager)
    {
        this.Configuration = configuration;
        this.GameContext = gameContext;
        this.PlugInManager = plugInManager;
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public GameConfiguration Configuration { get; }

    /// <summary>
    /// Gets the game context.
    /// </summary>
    public GameContext GameContext { get; }

    /// <summary>
    /// Gets the plugin manager.
    /// </summary>
    public PlugInManager PlugInManager { get; }

    /// <summary>
    /// Creates the world with the base updates of Season 21 and the given ones, applied in that order.
    /// </summary>
    /// <param name="updates">The updates.</param>
    /// <returns>The world.</returns>
    public static async ValueTask<Season21EventWorld> CreateAsync(params UpdatePlugInBase[] updates)
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        using var context = contextProvider.CreateNewConfigurationContext();
        var configuration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        UpdatePlugInBase[] baseUpdates =
        [
            new AddSeason21LevelCapsUpdatePlugIn(),
            new AddSeason21MapsUpdatePlugIn(),
            new AddSeason21ClassesUpdatePlugIn(),
            new AddSeason21ItemCatalogUpdatePlugIn(),
        ];
        foreach (var update in baseUpdates.Concat(updates))
        {
            await update.ApplyUpdateAsync(context, configuration).ConfigureAwait(false);
        }

        var mapInitializer = new MapInitializer(configuration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var plugInManager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        var gameContext = new GameContext(configuration, contextProvider, mapInitializer, new NullLoggerFactory(), plugInManager, new DefaultDropGenerator(configuration, Rand.GetRandomizer()), new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        return new Season21EventWorld(configuration, gameContext, plugInManager);
    }

    /// <summary>
    /// Waits until the condition is true, or fails after a timeout.
    /// </summary>
    /// <param name="condition">The condition.</param>
    public static async ValueTask WaitUntilAsync(Func<bool> condition)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.Elapsed > WaitTimeout)
            {
                Assert.Fail("Timed out waiting for the condition.");
            }

            await Task.Delay(20).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Skips the waits of a game until it plays.
    /// </summary>
    /// <param name="game">The game.</param>
    public static async ValueTask SkipUntilPlayingAsync(MiniGameContext game)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (game.State != MiniGameState.Playing)
        {
            if (stopwatch.Elapsed > WaitTimeout)
            {
                Assert.Fail("The game didn't start.");
            }

            game.SkipCurrentWait();
            await Task.Delay(20).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Puts the players into a new party, the first one as its master.
    /// </summary>
    /// <param name="players">The players.</param>
    /// <returns>The party.</returns>
    public static async ValueTask<Party> CreatePartyAsync(params Player[] players)
    {
        var party = new Party(new PartyManager(5, new NullLogger<Party>()), 5, new NullLogger<Party>());
        foreach (var player in players)
        {
            await party.AddAsync(player).ConfigureAwait(false);
        }

        return party;
    }

    /// <summary>
    /// Reads the stored configuration of a plugin, with its references into the game configuration.
    /// </summary>
    /// <typeparam name="T">The type of the configuration.</typeparam>
    /// <param name="typeId">The type id of the plugin.</param>
    /// <returns>The configuration.</returns>
    public T? GetStoredConfiguration<T>(Guid typeId)
        where T : class
        => this.Configuration.PlugInConfigurations.Single(c => c.TypeId == typeId).GetConfiguration<T>(new GameConfigurationReferenceHandler(this.Configuration));

    /// <summary>
    /// Adds a feature plugin, active.
    /// </summary>
    /// <param name="plugIn">The plugin.</param>
    public void AddFeaturePlugIn(IFeaturePlugIn plugIn) => this.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);

    /// <summary>
    /// Registers a plugin at a plugin point.
    /// </summary>
    /// <typeparam name="TPlugIn">The plugin point.</typeparam>
    /// <param name="plugIn">The plugin.</param>
    public void Register<TPlugIn>(TPlugIn plugIn)
        where TPlugIn : class
        => this.PlugInManager.RegisterPlugInAtPlugInPoint(plugIn);

    /// <summary>
    /// Creates a player of the class on a map, in the world.
    /// </summary>
    /// <param name="number">The class.</param>
    /// <param name="level">The level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="mapNumber">The map.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The player.</returns>
    public async ValueTask<Player> CreatePlayerAsync(CharacterClassNumber number, int level, int masterLevel, short mapNumber = 0, byte x = 132, byte y = 148)
    {
        var player = new InGamePlayer(this.GameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        await new CreateCharacterAction().CreateCharacterAsync(player, player.Account.LoginName, (byte)CharacterClassNumber.DarkKnight).ConfigureAwait(false);
        var character = player.Account.Characters.Single();
        character.CharacterClass = this.Configuration.CharacterClasses.Single(c => c.Number == (byte)number);
        character.CurrentMap = this.Configuration.Maps.Single(m => m.Number == mapNumber && m.Discriminator == 0);
        (character.PositionX, character.PositionY) = (x, y);
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        player.Attributes![Stats.Level] = level;
        player.Attributes[Stats.MasterLevel] = masterLevel;
        return player;
    }

    /// <summary>
    /// Lets the player talk to the NPC of the number nearby, and completes a warp it caused.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="npcNumber">The number of the NPC.</param>
    public async ValueTask TalkAsync(Player player, short npcNumber)
    {
        if (player.PlayerState.CurrentState == PlayerState.NpcDialogOpened)
        {
            await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        }

        var npc = player.CurrentMap!.GetNpcsInRange(player.Position, 255).First(n => n.Definition.Number == npcNumber);
        await new TalkNpcAction().TalkToNpcAsync(player, npc).ConfigureAwait(false);
        await CompleteMapChangeAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// Completes a map change of the player, as its client would acknowledge it.
    /// </summary>
    /// <param name="player">The player.</param>
    public static async ValueTask CompleteMapChangeAsync(Player player)
    {
        if (player.PlayerState.CurrentState == PlayerState.ChangingMap || player.CurrentMap is null)
        {
            await player.ClientReadyAfterMapChangeAsync().ConfigureAwait(false);
        }
    }

    private sealed class InGamePlayer : Player
    {
        public InGamePlayer(IGameContext gameContext)
            : base(gameContext)
        {
        }

        protected override ICustomPlugInContainer<IViewPlugIn> CreateViewPlugInContainer() => new MockViewPlugInContainer();
    }
}
