// <copyright file="AbilityCardCraftingTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.AbilityCore;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the <see cref="AbilityCardCrafting"/> at the chaos goblin.
/// </summary>
[TestFixture]
public class AbilityCardCraftingTest
{
    private const byte CardGroup = 20;
    private const short AbilityCrystal = 164;
    private const short BoundAbilityCrystal = 238;
    private const short UpgradeStone = 239;

    private GameConfiguration _gameConfiguration = null!;

    private IGameContext _gameContext = null!;

    private IGameContext _gameContextWithoutCore = null!;

    private MonsterDefinition _goblin = null!;

    /// <summary>
    /// Sets up a game with the Season 21 classes, items and the Ability Enhancement Core.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        using var context = contextProvider.CreateNewConfigurationContext();
        this._gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ClassesUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21AbilityCoreUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        this._goblin = this._gameConfiguration.Monsters.Single(m => m.NpcWindow == NpcWindow.ChaosMachine);

        this._gameContext = this.CreateGameContext(contextProvider);
        this._gameContext.FeaturePlugIns.AddPlugIn(new AbilityCoreFeaturePlugIn { Configuration = Season21AbilityCoreInitializer.CreateConfiguration() }, true);
        this._gameContextWithoutCore = this.CreateGameContext(contextProvider);
    }

    /// <summary>
    /// An option card has the chosen type, a random color and one random card option; crystals, bound or not, the
    /// jewel and the zen are taken.
    /// </summary>
    [Test]
    public async ValueTask OptionCardHasTheTypeAndAnOptionAsync()
    {
        var player = await this.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        player.Money = 10_000_000;
        await this.AddAsync(player, CardGroup, AbilityCrystal, 3).ConfigureAwait(false);
        await this.AddAsync(player, CardGroup, BoundAbilityCrystal, 2).ConfigureAwait(false);
        await this.AddAsync(player, 14, 13, 1).ConfigureAwait(false);

        var (result, item) = await this.CreateCrafting(AbilityCardCrafting.FirstOptionCardNumber + 1).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.Success));
        Assert.That(item!.Definition!.Number, Is.AnyOf((short)214, (short)219, (short)224));
        Assert.That(item.Level, Is.Zero);
        Assert.That(item.ItemOptions.Select(o => o.ItemOption!.OptionType), Is.EqualTo(new[] { Season21ItemOptionTypes.AbilityCardOption }));
        Assert.That(player.Money, Is.Zero);
        Assert.That(player.TemporaryStorage!.Items, Is.EquivalentTo(new[] { item }));
    }

    /// <summary>
    /// A skill card is one of the skill cards of the class.
    /// </summary>
    [Test]
    public async ValueTask SkillCardIsOfTheClassAsync()
    {
        var configuration = Season21AbilityCoreInitializer.CreateConfiguration();
        var player = await this.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        for (var i = 0; i < 6; i++)
        {
            foreach (var old in player.TemporaryStorage!.Items.ToList())
            {
                await player.TemporaryStorage.RemoveItemAsync(old).ConfigureAwait(false);
            }

            player.Money = 100_000_000;
            await this.AddAsync(player, CardGroup, AbilityCrystal, 50).ConfigureAwait(false);
            await this.AddAsync(player, 14, 13, 10).ConfigureAwait(false);

            var (result, item) = await this.CreateCrafting(AbilityCardCrafting.SkillCardNumber).DoMixAsync(player, 0).ConfigureAwait(false);

            Assert.That(result, Is.EqualTo(CraftingResult.Success));
            var card = configuration.Cards.Single(c => c.ItemGroup == CardGroup && c.ItemNumber == item!.Definition!.Number);
            Assert.That(card.IsFor(BaseClassCode.DarkWizard), Is.True, item!.Definition!.Name.ValueInNeutralLanguage);
            Assert.That(item.ItemOptions, Has.Count.EqualTo(1));
        }
    }

    /// <summary>
    /// The stones crafting gives five Ability Upgrade Stones.
    /// </summary>
    [Test]
    public async ValueTask StonesAreFiveAsync()
    {
        var player = await this.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        player.Money = 50_000_000;
        await this.AddAsync(player, CardGroup, AbilityCrystal, 25).ConfigureAwait(false);
        await this.AddAsync(player, 14, 13, 5).ConfigureAwait(false);

        var (result, item) = await this.CreateCrafting(AbilityCardCrafting.UpgradeStonesNumber).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.Success));
        Assert.That((item!.Definition!.Number, item.Durability, item.ItemOptions.Count), Is.EqualTo((UpgradeStone, 5.0, 0)));
    }

    /// <summary>
    /// Without the core the crafting takes nothing.
    /// </summary>
    [Test]
    public async ValueTask NothingIsCraftedWithoutTheCoreAsync()
    {
        var player = await this.CreatePlayerAsync(this._gameContextWithoutCore).ConfigureAwait(false);
        player.Money = 10_000_000;
        await this.AddAsync(player, CardGroup, AbilityCrystal, 5).ConfigureAwait(false);
        await this.AddAsync(player, 14, 13, 1).ConfigureAwait(false);

        var (result, item) = await this.CreateCrafting(AbilityCardCrafting.FirstOptionCardNumber).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.IncorrectMixItems));
        Assert.That(item, Is.Null);
        Assert.That(player.Money, Is.EqualTo(10_000_000));
        Assert.That(player.TemporaryStorage!.Items.Count(), Is.EqualTo(2));
    }

    private AbilityCardCrafting CreateCrafting(int number)
        => new(this._goblin.ItemCraftings.Single(c => c.Number == number).SimpleCraftingSettings!);

    private async ValueTask AddAsync(Player player, byte group, short number, int amount)
    {
        var definition = this._gameConfiguration.Items.Single(d => d.Group == group && d.Number == number);
        var stackable = definition.Durability > 1;
        var stacks = stackable ? 1 : amount;
        for (var i = 0; i < stacks; i++)
        {
            var item = player.PersistenceContext.CreateNew<Item>();
            item.Definition = definition;
            item.Durability = stackable ? amount : 1;
            Assert.That(await player.TemporaryStorage!.AddItemAsync(item).ConfigureAwait(false), Is.True);
        }
    }

    private IGameContext CreateGameContext(InMemoryPersistenceContextProvider contextProvider)
    {
        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var plugInManager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        var gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), plugInManager, NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        return gameContext;
    }

    private async ValueTask<Player> CreatePlayerAsync(IGameContext gameContext)
    {
        var player = new InGamePlayer(gameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        var first = ClassCodes.FromCode(ClassCodes.GetCode(BaseClassCode.DarkWizard, ClassEvolution.First))!.Value;
        await new CreateCharacterAction().CreateCharacterAsync(player, "A" + Guid.NewGuid().ToString("N")[..8], first).ConfigureAwait(false);
        await player.SetSelectedCharacterAsync(player.Account.Characters.Single()).ConfigureAwait(false);
        player.Attributes![Stats.Level] = 400;
        return player;
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
