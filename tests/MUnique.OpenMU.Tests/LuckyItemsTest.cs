// <copyright file="LuckyItemsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the lucky items of Season 6 beyond David's ticket exchange: the refinery, the wear and the jewels of
/// extension and elevation, with the season 6 data.
/// </summary>
[TestFixture]
public class LuckyItemsTest
{
    private const byte RefineryCraftingNumber = 52;

    private GameConfiguration _gameConfiguration = null!;

    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up the test environment by initializing the season 6 configuration data.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        this._gameConfiguration = (await contextProvider.CreateNewConfigurationContext().GetAsync<GameConfiguration>().ConfigureAwait(false)).First();

        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        this._gameContext = gameContext;
    }

    /// <summary>
    /// The items for which David exchanges a ticket are the lucky items; others aren't.
    /// </summary>
    [Test]
    public void LuckyItemsAreTheResultsOfTheTicketExchange()
    {
        Assert.That(this.Definition(8, 62).IsLuckyItem(this._gameConfiguration), Is.True); // Scale Armor of the lucky set
        Assert.That(this.Definition(11, 72).IsLuckyItem(this._gameConfiguration), Is.True); // Storm Jahad Boots of the lucky set
        Assert.That(this.Definition(8, 1).IsLuckyItem(this._gameConfiguration), Is.False); // Dragon Armor
        Assert.That(this.Definition(14, 13).IsLuckyItem(this._gameConfiguration), Is.False); // Jewel of Bless
    }

    /// <summary>
    /// The refinery uses up the lucky piece and gives a jewel of extension, a jewel of elevation or nothing.
    /// </summary>
    [Test]
    public async ValueTask RefineryUsesUpTheLuckyPieceAsync()
    {
        var crafting = this._gameConfiguration.Monsters.Single(m => m.NpcWindow == NpcWindow.CombineLuckyItem).ItemCraftings.Single(c => c.Number == RefineryCraftingNumber);
        var results = new HashSet<short?>();
        for (var i = 0; i < 40; i++)
        {
            var player = await this.CreatePlayerAsync().ConfigureAwait(false);
            await player.TemporaryStorage!.AddItemAsync(this.CreateItem(player, 8, 62)).ConfigureAwait(false);

            var (result, item) = await new LuckyItemRefineryCrafting(crafting.SimpleCraftingSettings!).DoMixAsync(player, 0).ConfigureAwait(false);

            Assert.That(player.TemporaryStorage.Items.Where(x => x.Definition!.Group == 8), Is.Empty);
            Assert.That(result, Is.AnyOf(CraftingResult.Success, CraftingResult.Failed));
            Assert.That(item?.Definition?.Number, result == CraftingResult.Success ? Is.AnyOf((short)160, (short)161) : Is.Null);
            results.Add(item?.Definition?.Number);
        }

        Assert.That(results, Does.Contain((short?)160));
    }

    /// <summary>
    /// The refinery doesn't accept an item which isn't lucky.
    /// </summary>
    [Test]
    public async ValueTask RefineryRefusesOtherItemsAsync()
    {
        var crafting = this._gameConfiguration.Monsters.Single(m => m.NpcWindow == NpcWindow.CombineLuckyItem).ItemCraftings.Single(c => c.Number == RefineryCraftingNumber);
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var armor = this.CreateItem(player, 8, 1);
        await player.TemporaryStorage!.AddItemAsync(armor).ConfigureAwait(false);

        var (result, _) = await new LuckyItemRefineryCrafting(crafting.SimpleCraftingSettings!).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.LackingMixItems));
        Assert.That(player.TemporaryStorage.Items, Is.EquivalentTo(new[] { armor }));
    }

    /// <summary>
    /// The jewel of extension repairs a lucky piece, but no other item.
    /// </summary>
    /// <param name="group">The group of the target item.</param>
    /// <param name="number">The number of the target item.</param>
    /// <param name="expectedConsumption">Whether the jewel is expected to be consumed.</param>
    [TestCase(8, 62, true)]
    [TestCase(8, 1, false)]
    public async ValueTask JewelOfExtensionRepairsLuckyItemsAsync(byte group, short number, bool expectedConsumption)
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var target = this.CreateItem(player, group, number);
        target.Durability = 10;
        await player.Inventory!.AddItemAsync(20, target).ConfigureAwait(false);
        var jewel = this.CreateItem(player, 14, 160);
        await player.Inventory.AddItemAsync(44, jewel).ConfigureAwait(false);

        var consumed = await new JewelOfExtensionConsumeHandlerPlugIn().ConsumeItemAsync(player, jewel, target, FruitUsage.Undefined).ConfigureAwait(false);

        Assert.That(consumed, Is.EqualTo(expectedConsumption));
        Assert.That(target.Durability, expectedConsumption ? Is.EqualTo(target.GetMaximumDurabilityOfOnePiece()) : Is.EqualTo(10));
    }

    /// <summary>
    /// The jewel of elevation gives a lucky piece a harmony option, once.
    /// </summary>
    [Test]
    public async ValueTask JewelOfElevationAddsAHarmonyOptionAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var target = this.CreateItem(player, 8, 62);
        await player.Inventory!.AddItemAsync(20, target).ConfigureAwait(false);
        var first = this.CreateItem(player, 14, 161);
        await player.Inventory.AddItemAsync(44, first).ConfigureAwait(false);
        var second = this.CreateItem(player, 14, 161);
        await player.Inventory.AddItemAsync(45, second).ConfigureAwait(false);
        var handler = new JewelOfElevationConsumeHandlerPlugIn();

        var firstConsumed = await handler.ConsumeItemAsync(player, first, target, FruitUsage.Undefined).ConfigureAwait(false);
        var secondConsumed = await handler.ConsumeItemAsync(player, second, target, FruitUsage.Undefined).ConfigureAwait(false);

        Assert.That(firstConsumed, Is.True);
        Assert.That(target.ItemOptions.Count(o => o.ItemOption?.OptionType == ItemOptionTypes.HarmonyOption), Is.EqualTo(1));
        Assert.That(secondConsumed, Is.False);
    }

    /// <summary>
    /// The jewel of elevation doesn't work on an item which isn't lucky.
    /// </summary>
    [Test]
    public async ValueTask JewelOfElevationRefusesOtherItemsAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var target = this.CreateItem(player, 8, 1);
        await player.Inventory!.AddItemAsync(20, target).ConfigureAwait(false);
        var jewel = this.CreateItem(player, 14, 161);
        await player.Inventory.AddItemAsync(44, jewel).ConfigureAwait(false);

        var consumed = await new JewelOfElevationConsumeHandlerPlugIn().ConsumeItemAsync(player, jewel, target, FruitUsage.Undefined).ConfigureAwait(false);

        Assert.That(consumed, Is.False);
        Assert.That(target.ItemOptions, Is.Empty);
    }

    /// <summary>
    /// A worn lucky piece loses a point of durability each time it wears; other worn items don't.
    /// </summary>
    [Test]
    public async ValueTask WornLuckyItemsWearAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var lucky = this.CreateItem(player, 9, 62);
        await player.Inventory!.AddItemAsync(InventoryConstants.PantsSlot, lucky).ConfigureAwait(false);
        var other = this.CreateItem(player, 8, 1);
        await player.Inventory.AddItemAsync(InventoryConstants.ArmorSlot, other).ConfigureAwait(false);
        var luckyDurability = lucky.Durability;
        var otherDurability = other.Durability;

        await LuckyItemWearPlugIn.WearAsync(player).ConfigureAwait(false);

        Assert.That(lucky.Durability, Is.EqualTo(luckyDurability - 1));
        Assert.That(other.Durability, Is.EqualTo(otherDurability));
    }

    private async ValueTask<Player> CreatePlayerAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        return player;
    }

    private ItemDefinition Definition(byte group, short number)
        => this._gameConfiguration.Items.Single(i => i.Group == group && i.Number == number);

    private Item CreateItem(Player player, byte group, short number)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = this.Definition(group, number);
        item.Durability = item.GetMaximumDurabilityOfOnePiece();
        return item;
    }
}
