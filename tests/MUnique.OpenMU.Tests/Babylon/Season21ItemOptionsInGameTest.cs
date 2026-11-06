// <copyright file="Season21ItemOptionsInGameTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the options of Season 21 in the game: their stats, the seed sphere enhancement up to level 10 and the W32
/// item format which carries them.
/// </summary>
[TestFixture]
public class Season21ItemOptionsInGameTest
{
    private GameConfiguration _gameConfiguration = null!;
    private IPersistenceContextProvider _contextProvider = null!;
    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up a Season 6 configuration with the items and the item options of Season 21.
    /// </summary>
    [OneTimeSetUp]
    public async ValueTask SetupAsync()
    {
        this._contextProvider = new InMemoryPersistenceContextProvider();
        await new Persistence.Initialization.VersionSeasonSix.DataInitialization(this._contextProvider, new NullLoggerFactory()).CreateInitialDataAsync(1, true).ConfigureAwait(false);
        using (var context = this._contextProvider.CreateNewConfigurationContext())
        {
            var configuration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
            UpdatePlugInBase[] updates =
            [
                new AddSeason21GearUpdatePlugIn(),
                new AddSeason21ItemCatalogUpdatePlugIn(),
                new ActivateSeason21WideItemsUpdatePlugIn(),
                new AddSeason21MasteryOptionsUpdatePlugIn(),
                new AddSeason21WingGradesUpdatePlugIn(),
                new AddSeason21SphereLevelsUpdatePlugIn(),
            ];
            foreach (var update in updates)
            {
                await update.ApplyUpdateAsync(context, configuration).ConfigureAwait(false);
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        this._gameConfiguration = (await this._contextProvider.CreateNewConfigurationContext().GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(this._gameConfiguration, this._contextProvider, mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        this._gameContext = gameContext;
    }

    /// <summary>
    /// A mastery armor with its health option raises the maximum health by 165; its damage decrease bonus option
    /// raises the defense.
    /// </summary>
    [Test]
    public async ValueTask MasteryArmorOptionsApplyAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        var armor = this.CreateItem(player, this.Item(8, 99), 0);
        var before = player.Attributes![Stats.MaximumHealth];

        AddOption(armor, Season21ItemOptionTypes.Mastery, 6, 0);
        await this.EquipAsync(player, armor).ConfigureAwait(false);

        Assert.That(player.Attributes[Stats.MaximumHealth] - before, Is.EqualTo(165).Within(0.01));
        Assert.That(WideItemRules.IsWideOnly(armor), Is.True);
    }

    /// <summary>
    /// The bonus option of a mastery weapon raises the all stats bonus by its level's value; the classes pass it on
    /// to strength, agility, vitality and energy (Season21ItemOptionsTest.AllStatsBonusRaisesTheStats).
    /// </summary>
    [Test]
    public async ValueTask MasteryWeaponBonusRaisesAllStatsAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        var sword = this.CreateItem(player, this.Item(0, 42), 0);
        AddOption(sword, Season21ItemOptionTypes.MasteryBonus, 0, 2);

        await this.EquipAsync(player, sword).ConfigureAwait(false);

        Assert.That(player.Attributes![Stats.AllStatsBonus], Is.EqualTo(25).Within(0.01), "level 2: +25");
    }

    /// <summary>
    /// A grade option of a fourth wing applies its value of the grade.
    /// </summary>
    [Test]
    public async ValueTask WingGradeOptionAppliesAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        var wings = this.CreateItem(player, this.Item(12, 414), 0);
        AddOption(wings, Season21ItemOptionTypes.WingGrade, 7, 3);
        var before = player.Attributes![Stats.TotalStrength];

        await this.EquipAsync(player, wings).ConfigureAwait(false);

        Assert.That(player.Attributes[Stats.TotalStrength] - before, Is.EqualTo(40).Within(0.01), "grade 2 force");
    }

    /// <summary>
    /// A level 5 seed sphere, 7 sphere upgrade runes and the enhancement give the level 6 seed sphere of the same
    /// element and option; mounted, its option has level 6, which the W32 format keeps and Season 6 can't carry.
    /// A Season 6 client can't enhance into a sphere it couldn't show.
    /// </summary>
    [Test]
    public async ValueTask SeedSphereEnhancementReachesLevel6Async()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        var enhance = new TestEnhanceCrafting(this.CraftingSettings(100));
        await this.AddAsync(player, this.Item(12, 124), 2).ConfigureAwait(false);
        await this.AddRunesAsync(player, 7).ConfigureAwait(false);

        Assert.That(enhance.TryGetRequiredItems(player, out _, out _), Is.EqualTo(CraftingResult.IncorrectMixItems), "Season 6 client");

        Mock.Get(player.ViewPlugIns.GetPlugIn<IItemFormatViewPlugIn>()!).Setup(p => p.CarriesWideItems).Returns(true);
        var sphere = await enhance.CraftAsync(player).ConfigureAwait(false);
        Assert.That(sphere.Definition!.Number, Is.EqualTo(311));
        Assert.That(sphere.Level, Is.EqualTo(2));

        foreach (var item in player.TemporaryStorage!.Items.ToList())
        {
            await player.TemporaryStorage.RemoveItemAsync(item).ConfigureAwait(false);
        }

        var seedOptions = sphere.Definition.PossibleItemOptions.Single(o => o.PossibleOptions.Any(p => p.OptionType == ItemOptionTypes.SocketOption));
        var socketItem = await this.AddAsync(player, this._gameConfiguration.Items.First(i => i.MaximumSockets > 0 && i.PossibleItemOptions.Contains(seedOptions)), 0).ConfigureAwait(false);
        socketItem.SocketCount = 1;
        await player.TemporaryStorage.AddItemAsync(sphere).ConfigureAwait(false);
        await this.AddAsync(player, this._gameConfiguration.Items.First(i => i.Name.ValueInNeutralLanguage == "Jewel of Chaos"), 0).ConfigureAwait(false);
        await this.AddAsync(player, this._gameConfiguration.Items.First(i => i.Name.ValueInNeutralLanguage == "Jewel of Creation"), 0).ConfigureAwait(false);
        var mounted = await new TestMountCrafting(this.CraftingSettings(44)).CraftAsync(player).ConfigureAwait(false);

        var socketOption = mounted.ItemOptions.Single(o => o.ItemOption!.OptionType == ItemOptionTypes.SocketOption);
        Assert.That(socketOption.Level, Is.EqualTo(6));
        Assert.That(WideItemRules.IsWideOnly(mounted), Is.True);

        var w32 = new ItemSerializerW32();
        var data = new byte[ItemSerializerW32.Length];
        w32.SerializeItem(data, mounted);
        var read = w32.DeserializeItem(data, this._gameConfiguration, player.PersistenceContext);
        Assert.That(read.ItemOptions.Single(o => o.ItemOption!.OptionType == ItemOptionTypes.SocketOption).Level, Is.EqualTo(6));
    }

    private static void AddOption(Item item, ItemOptionType type, int number, int level)
    {
        var option = item.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Single(o => o.OptionType == type && o.Number == number);
        item.ItemOptions.Add(new Persistence.BasicModel.ItemOptionLink { ItemOption = option, Level = level });
    }

    private static async ValueTask<Item> CraftAsync(SimpleItemCraftingHandler handler, Player player, Func<IList<CraftingRequiredItemLink>, ValueTask<List<Item>>> createResult)
    {
        Assert.That(handler.TryGetRequiredItems(player, out var requiredItems, out _), Is.Null, handler.GetType().Name);
        return (await createResult(requiredItems).ConfigureAwait(false)).Single();
    }

    private ItemDefinition Item(byte group, short number) => this._gameConfiguration.Items.Single(i => i.Group == group && i.Number == number);

    private SimpleCraftingSettings CraftingSettings(byte number)
        => this._gameConfiguration.Monsters.SelectMany(m => m.ItemCraftings).Single(c => c.Number == number).SimpleCraftingSettings!;

    private Item CreateItem(Player player, ItemDefinition definition, byte level)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = definition;
        item.Level = level;
        item.Durability = definition.Durability;
        return item;
    }

    private async ValueTask EquipAsync(Player player, Item item)
    {
        var slot = item.Definition!.ItemSlot!.ItemSlots.First();
        Assert.That(await player.Inventory!.AddItemAsync((byte)slot, item).ConfigureAwait(false), Is.True);
    }

    private async ValueTask<Item> AddAsync(Player player, ItemDefinition definition, byte level)
    {
        var item = this.CreateItem(player, definition, level);
        Assert.That(await player.TemporaryStorage!.AddItemAsync(item).ConfigureAwait(false), Is.True);
        return item;
    }

    private async ValueTask AddRunesAsync(Player player, int count)
    {
        var rune = await this.AddAsync(player, this.Item(12, 401), 0).ConfigureAwait(false);
        if (rune.IsStackable())
        {
            rune.Durability = count;
            return;
        }

        for (var i = 1; i < count; i++)
        {
            await this.AddAsync(player, this.Item(12, 401), 0).ConfigureAwait(false);
        }
    }

    private sealed class TestEnhanceCrafting(SimpleCraftingSettings settings) : SeedSphereEnhanceCrafting(settings)
    {
        public ValueTask<Item> CraftAsync(Player player) => Season21ItemOptionsInGameTest.CraftAsync(this, player, items => this.CreateOrModifyResultItemsAsync(items, player, 0, 100));
    }

    private sealed class TestMountCrafting(SimpleCraftingSettings settings) : MountSeedSphereCrafting(settings)
    {
        public ValueTask<Item> CraftAsync(Player player) => Season21ItemOptionsInGameTest.CraftAsync(this, player, items => this.CreateOrModifyResultItemsAsync(items, player, 0, 100));
    }
}
