// <copyright file="LuckyItemTicketExchangeCraftingTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the <see cref="LuckyItemTicketExchangeCrafting"/> with the season 6 data of David's lucky item exchange.
/// </summary>
[TestFixture]
public class LuckyItemTicketExchangeCraftingTest
{
    private const byte HelperGroup = 13;

    private GameConfiguration _gameConfiguration = null!;

    private IGameContext _gameContext = null!;

    private InMemoryPersistenceContextProvider _contextProvider = null!;

    private SimpleCraftingSettings _settings = null!;

    /// <summary>
    /// Sets up the test environment by initializing the season 6 configuration data.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(this._contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        this._gameConfiguration = (await this._contextProvider.CreateNewConfigurationContext().GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        this._gameContext = this.CreateGameContext();

        var david = this._gameConfiguration.Monsters.Single(m => m.NpcWindow == NpcWindow.CombineLuckyItem);
        this._settings = david.ItemCraftings.Single(c => c.Number == 51).SimpleCraftingSettings!;
    }

    /// <summary>
    /// Tests that a ticket is exchanged for the lucky armor of its item group and tier, which fits the character class,
    /// and that the ticket is consumed.
    /// </summary>
    /// <param name="characterClass">The character class of the player.</param>
    /// <param name="ticketNumber">The item number of the ticket.</param>
    /// <param name="expectedGroup">The expected item group of the lucky armor.</param>
    /// <param name="expectedNumber">The expected item number of the lucky armor.</param>
    [TestCase(CharacterClassNumber.DarkKnight, 135, 8, 62)] // 1st armor ticket: Scale Armor
    [TestCase(CharacterClassNumber.BladeMaster, 137, 7, 62)] // 1st helm ticket: Scale Helm
    [TestCase(CharacterClassNumber.FairyElf, 136, 9, 63)] // 1st pants ticket: Silk Pants
    [TestCase(CharacterClassNumber.DarkWizard, 138, 10, 64)] // 1st gloves ticket: Sphinx Gloves
    [TestCase(CharacterClassNumber.Summoner, 139, 11, 65)] // 1st boots ticket: Violent Wind Boots
    [TestCase(CharacterClassNumber.MagicGladiator, 135, 8, 64)] // 1st armor ticket: Sphinx Armor, for fewer classes than Scale Armor
    [TestCase(CharacterClassNumber.MagicGladiator, 140, 8, 71)] // 2nd armor ticket: Atlans Armor, which is made for the class
    [TestCase(CharacterClassNumber.DarkLord, 142, 7, 66)] // 2nd helm ticket: Adamantine Mask
    [TestCase(CharacterClassNumber.RageFighter, 144, 11, 72)] // 2nd boots ticket: Storm Jahad Boots
    public async ValueTask TicketIsExchangedForTheLuckyArmorOfTheClassAsync(CharacterClassNumber characterClass, short ticketNumber, byte expectedGroup, short expectedNumber)
    {
        var player = await this.CreatePlayerAsync(characterClass).ConfigureAwait(false);
        await this.AddTicketAsync(player, ticketNumber).ConfigureAwait(false);

        var (result, item) = await new LuckyItemTicketExchangeCrafting(this._settings).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.Success));
        Assert.That(item, Is.Not.Null);
        Assert.That(item!.Definition!.Group, Is.EqualTo(expectedGroup));
        Assert.That(item.Definition.Number, Is.EqualTo(expectedNumber));
        Assert.That(item.Durability, Is.EqualTo(byte.MaxValue));
        Assert.That(player.TemporaryStorage!.Items, Is.EquivalentTo(new[] { item }));
    }

    /// <summary>
    /// Tests that a ticket for which the character class has no lucky armor is not accepted, so it isn't consumed.
    /// The magic gladiator can't wear a helm.
    /// </summary>
    [Test]
    public async ValueTask TicketWithoutArmorForTheClassIsNotAcceptedAsync()
    {
        var player = await this.CreatePlayerAsync(CharacterClassNumber.MagicGladiator).ConfigureAwait(false);
        var ticket = await this.AddTicketAsync(player, 137).ConfigureAwait(false);

        var (result, item) = await new LuckyItemTicketExchangeCrafting(this._settings).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.IncorrectMixItems));
        Assert.That(item, Is.Null);
        Assert.That(player.TemporaryStorage!.Items, Is.EquivalentTo(new[] { ticket }));
    }

    /// <summary>
    /// Tests that only one ticket is exchanged at once.
    /// </summary>
    [Test]
    public async ValueTask TwoTicketsAreNotAcceptedAsync()
    {
        var player = await this.CreatePlayerAsync(CharacterClassNumber.DarkKnight).ConfigureAwait(false);
        await this.AddTicketAsync(player, 135).ConfigureAwait(false);
        await this.AddTicketAsync(player, 136).ConfigureAwait(false);

        var (result, _) = await new LuckyItemTicketExchangeCrafting(this._settings).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.IncorrectMixItems));
        Assert.That(player.TemporaryStorage!.Items.Count(), Is.EqualTo(2));
    }

    /// <summary>
    /// Tests that an item which isn't a ticket is not accepted.
    /// </summary>
    [Test]
    public async ValueTask OtherItemIsNotAcceptedAsync()
    {
        var player = await this.CreatePlayerAsync(CharacterClassNumber.DarkKnight).ConfigureAwait(false);
        var jewel = player.PersistenceContext.CreateNew<Item>();
        jewel.Definition = this._gameConfiguration.Items.Single(i => i.Group == 14 && i.Number == 13);
        jewel.Durability = 1;
        await player.TemporaryStorage!.AddItemAsync(jewel).ConfigureAwait(false);

        var (result, _) = await new LuckyItemTicketExchangeCrafting(this._settings).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.IncorrectMixItems));
    }

    /// <summary>
    /// Tests that a ticket is not accepted when the lucky armor doesn't fit into the inventory, so it isn't consumed.
    /// </summary>
    [Test]
    public async ValueTask TicketIsNotAcceptedWhenTheArmorDoesNotFitAsync()
    {
        var player = await this.CreatePlayerAsync(CharacterClassNumber.DarkKnight).ConfigureAwait(false);
        var ticket = await this.AddTicketAsync(player, 135).ConfigureAwait(false);
        var jewelOfBless = this._gameConfiguration.Items.Single(i => i.Group == 14 && i.Number == 13);
        foreach (var slot in player.Inventory!.FreeSlots.Where(s => s >= InventoryConstants.EquippableSlotsCount).ToList())
        {
            var jewel = player.PersistenceContext.CreateNew<Item>();
            jewel.Definition = jewelOfBless;
            jewel.Durability = 1;
            await player.Inventory.AddItemAsync(slot, jewel).ConfigureAwait(false);
        }

        var (result, item) = await new LuckyItemTicketExchangeCrafting(this._settings).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.TooManyItems));
        Assert.That(item, Is.Null);
        Assert.That(player.TemporaryStorage!.Items, Is.EquivalentTo(new[] { ticket }));
    }

    /// <summary>
    /// Tests that with the item rules of Season 21 the exchange rolls the level and the option of the lucky armor by the
    /// weights of <see cref="LuckyTicketRolls"/>; the durability stays the lucky item's.
    /// </summary>
    [Test]
    public async ValueTask TicketRollsLevelAndOptionWithTheSeason21RulesAsync()
    {
        var rolls = new LuckyTicketRolls
        {
            LevelWeights = [0, 0, 0, 0, 0, 0, 0, 0, 0, 10000],
            OptionWeights = [0, 0, 0, 10000],
        };
        var armor = this._gameConfiguration.Items.Single(i => i.Group == 8 && i.Number == 62);
        var defenseOption = this._gameConfiguration.ItemOptions.First(o => o.PossibleOptions.Any(p => p.OptionType == ItemOptionTypes.Option && p.PowerUpDefinition?.TargetAttribute == Stats.DefenseBase));
        if (!armor.PossibleItemOptions.Contains(defenseOption))
        {
            // The lucky sets update gives the pieces this option on a Season 21 world.
            armor.PossibleItemOptions.Add(defenseOption);
        }

        var gameContext = this.CreateGameContext();
        gameContext.FeaturePlugIns.AddPlugIn(new Season21ItemsFeaturePlugIn { Configuration = new Season21ItemsConfiguration { LuckyTicketRolls = rolls } }, true);
        var player = await this.CreatePlayerAsync(CharacterClassNumber.DarkKnight, gameContext).ConfigureAwait(false);
        await this.AddTicketAsync(player, 135).ConfigureAwait(false);

        var (result, item) = await new LuckyItemTicketExchangeCrafting(this._settings).DoMixAsync(player, 0).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(CraftingResult.Success));
        Assert.That(item!.Level, Is.EqualTo(9));
        Assert.That(item.ItemOptions.Single(o => o.ItemOption?.OptionType == ItemOptionTypes.Option).Level, Is.EqualTo(3));
        Assert.That(item.Durability, Is.EqualTo(byte.MaxValue));
    }

    /// <summary>
    /// Tests that the default rolls are the rows "(Lucky)" of S21Emu <c>ItemOptionRate.txt</c>, which <c>LuckySystem.xml</c>
    /// names with Option0-5 = 10: level +0 to +15 and option +0 to +3.
    /// </summary>
    [Test]
    public void DefaultRollsAreTheLuckyRowsOfS21Emu()
    {
        var rolls = new LuckyTicketRolls();

        Assert.That(rolls.LevelWeights, Is.EqualTo(new[] { 1000, 2000, 1700, 1400, 1200, 900, 700, 500, 300, 100, 70, 50, 35, 20, 15, 10 }));
        Assert.That(rolls.OptionWeights, Is.EqualTo(new[] { 4000, 3000, 2000, 1000 }));
    }

    private GameContext CreateGameContext()
    {
        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(this._gameConfiguration, this._contextProvider, mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        return gameContext;
    }

    private async ValueTask<Player> CreatePlayerAsync(CharacterClassNumber characterClass, IGameContext? gameContext = null)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(gameContext ?? this._gameContext).ConfigureAwait(false);
        player.SelectedCharacter!.CharacterClass = this._gameConfiguration.CharacterClasses.Single(c => c.Number == (byte)characterClass);
        return player;
    }

    private async ValueTask<Item> AddTicketAsync(Player player, short ticketNumber)
    {
        var ticket = player.PersistenceContext.CreateNew<Item>();
        ticket.Definition = this._gameConfiguration.Items.Single(i => i.Group == HelperGroup && i.Number == ticketNumber);
        ticket.Durability = 1;
        await player.TemporaryStorage!.AddItemAsync(ticket).ConfigureAwait(false);
        return ticket;
    }
}
