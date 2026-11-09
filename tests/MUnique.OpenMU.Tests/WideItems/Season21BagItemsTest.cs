// <copyright file="Season21BagItemsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.WideItems;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the uses of the Season 21 items of group 19: the item bags (Pandora Box, Red and Blue Card, Premium Muun Box,
/// Red Card [Elemental]) and the Blessing of Eunice.
/// </summary>
[TestFixture]
public class Season21BagItemsTest
{
    private const byte BagGroup = 19;

    private GameConfiguration _gameConfiguration = null!;

    private InMemoryPersistenceContextProvider _contextProvider = null!;

    /// <summary>
    /// Sets up the Season 6 configuration.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._contextProvider = new InMemoryPersistenceContextProvider();
        await new DataInitialization(this._contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(0, false).ConfigureAwait(false);
        this._gameConfiguration = (await this._contextProvider.CreateNewConfigurationContext().GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
    }

    /// <summary>
    /// A bag gives one item of every drop group of its definition next to the player, the Ruud of its row, and is used up.
    /// </summary>
    [Test]
    public async ValueTask BagGivesAnItemOfEveryGroupAndItsRuudAsync()
    {
        var player = await this.CreatePlayerAsync(true).ConfigureAwait(false);
        var bag = this.CreateBag(player, 14, ("Jewel of Bless", 0), ("Jewel of Soul", 0));

        var consumed = await new ElementalRedCardConsumeHandlerPlugIn().ConsumeItemAsync(player, bag, null, FruitUsage.Undefined).ConfigureAwait(false);

        Assert.That(consumed, Is.True);
        Assert.That(bag.Durability, Is.EqualTo(0));
        var drops = player.CurrentMap!.GetDropsInRange(player.Position, 2).OfType<DroppedItem>().Select(d => d.Item.Definition!.Name.ValueInNeutralLanguage).ToList();
        Assert.That(drops, Is.EquivalentTo(new[] { "Jewel of Bless", "Jewel of Soul" }));
        Assert.That(player.SelectedCharacter!.Ruud, Is.InRange(50, 250));
    }

    /// <summary>
    /// The level range of a group is the level of the given item, e.g. the Muuns of the Premium Muun Box at level 1.
    /// </summary>
    [Test]
    public async ValueTask BagItemTakesTheLevelOfItsGroupAsync()
    {
        var player = await this.CreatePlayerAsync(true).ConfigureAwait(false);
        var bag = this.CreateBag(player, 3, ("Kris", 1));

        var consumed = await new PremiumMuunBoxConsumeHandlerPlugIn().ConsumeItemAsync(player, bag, null, FruitUsage.Undefined).ConfigureAwait(false);

        Assert.That(consumed, Is.True);
        var drop = player.CurrentMap!.GetDropsInRange(player.Position, 2).OfType<DroppedItem>().Single();
        Assert.That(drop.Item.Level, Is.EqualTo(1));
        Assert.That(player.SelectedCharacter!.Ruud, Is.EqualTo(0));
    }

    /// <summary>
    /// Without the item rules of Season 21 a bag doesn't open and stays.
    /// </summary>
    [Test]
    public async ValueTask BagNeedsTheSeason21ItemRulesAsync()
    {
        var player = await this.CreatePlayerAsync(false).ConfigureAwait(false);
        var bag = this.CreateBag(player, 0, ("Jewel of Bless", 0));

        var consumed = await new PandoraBoxConsumeHandlerPlugIn().ConsumeItemAsync(player, bag, null, FruitUsage.Undefined).ConfigureAwait(false);

        Assert.That(consumed, Is.False);
        Assert.That(bag.Durability, Is.EqualTo(1));
        Assert.That(player.CurrentMap!.GetDropsInRange(player.Position, 2), Is.Empty);
    }

    /// <summary>
    /// The Blessing of Eunice is for the fifth evolution: it gives its experience rate for its time there, and nothing to
    /// an earlier evolution.
    /// </summary>
    /// <param name="evolution">The evolution of the player's class.</param>
    [TestCase(ClassEvolution.Fifth)]
    [TestCase(ClassEvolution.Fourth)]
    public async ValueTask BlessingOfEuniceIsForTheFifthEvolutionAsync(ClassEvolution evolution)
    {
        var player = await this.CreatePlayerAsync(false).ConfigureAwait(false);
        var classNumber = ClassCodes.FromCode(ClassCodes.GetCode(BaseClassCode.DarkKnight, evolution))!.Value;
        player.SelectedCharacter!.CharacterClass = new DataModel.Configuration.CharacterClass { Number = classNumber };
        var blessing = player.PersistenceContext.CreateNew<Item>();
        blessing.Definition = this.CreateEunice(player);
        blessing.Durability = 1;

        var consumed = await new BlessingOfEuniceConsumeHandlerPlugIn().ConsumeItemAsync(player, blessing, null, FruitUsage.Undefined).ConfigureAwait(false);

        Assert.That(consumed, Is.EqualTo(evolution == ClassEvolution.Fifth));
        Assert.That(player.MagicEffectList.GetActiveEffectsSnapshot().Any(e => e.Definition.Number == 447), Is.EqualTo(consumed));
        if (consumed)
        {
            Assert.That(player.Attributes![Stats.ExperienceRate], Is.EqualTo(1.5f).Within(1e-5));
        }
    }

    private async ValueTask<Player> CreatePlayerAsync(bool season21Items)
    {
        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var dropGenerator = new DefaultDropGenerator(this._gameConfiguration, Rand.GetRandomizer());
        var gameContext = new GameContext(this._gameConfiguration, this._contextProvider, mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), dropGenerator, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        if (season21Items)
        {
            gameContext.FeaturePlugIns.AddPlugIn(new Season21ItemsFeaturePlugIn { Configuration = new Season21ItemsConfiguration() }, true);
        }

        var player = await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false);
        player.CurrentMap = await gameContext.GetMapAsync(0).ConfigureAwait(false);
        player.Position = new Point(135, 130);
        return player;
    }

    private Item CreateBag(Player player, short number, params (string Name, byte Level)[] groups)
    {
        var definition = player.PersistenceContext.CreateNew<ItemDefinition>();
        definition.Group = BagGroup;
        definition.Number = number;
        definition.Width = 1;
        definition.Height = 1;
        definition.Durability = 1;
        foreach (var (name, level) in groups)
        {
            var group = player.PersistenceContext.CreateNew<ItemDropItemGroup>();
            group.Chance = 1;
            group.ItemType = SpecialItemType.RandomItem;
            group.MinimumLevel = level;
            group.MaximumLevel = level;
            group.PossibleItems.Add(this._gameConfiguration.Items.First(i => i.Name.ValueInNeutralLanguage == name));
            definition.DropItems.Add(group);
        }

        var bag = player.PersistenceContext.CreateNew<Item>();
        bag.Definition = definition;
        bag.Durability = 1;
        return bag;
    }

    private ItemDefinition CreateEunice(Player player)
    {
        var definition = player.PersistenceContext.CreateNew<ItemDefinition>();
        definition.Group = BagGroup;
        definition.Number = 17;
        definition.Durability = 1;
        var effect = player.PersistenceContext.CreateNew<MagicEffectDefinition>();
        effect.Number = 447;
        effect.SubType = 200;
        effect.Duration = player.PersistenceContext.CreateNew<PowerUpDefinitionValue>();
        effect.Duration.ConstantValue.Value = 1800;
        var powerUp = player.PersistenceContext.CreateNew<PowerUpDefinition>();
        powerUp.TargetAttribute = Stats.ExperienceRate;
        powerUp.Boost = player.PersistenceContext.CreateNew<PowerUpDefinitionValue>();
        powerUp.Boost.ConstantValue.AggregateType = AttributeSystem.AggregateType.Multiplicate;
        powerUp.Boost.ConstantValue.Value = 1.5f;
        effect.PowerUpDefinitions.Add(powerUp);
        definition.ConsumeEffect = effect;
        return definition;
    }
}
