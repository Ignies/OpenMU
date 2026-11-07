// <copyright file="AbilityCoreInGameTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.AbilityCore;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlayerActions.Character;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the Ability Enhancement Core in game with the Season 21 data: cards give their options and skills, the slot
/// rules, the level up and the reduction.
/// </summary>
[TestFixture]
public class AbilityCoreInGameTest
{
    private const byte CardGroup = 20;
    private const short MeteorStormCard = 165;
    private const short MeteorStorm = 2037;
    private const short MeteorStormFourth = 1076;
    private const short BlueType1 = 213;
    private const short BlueType4 = 216;
    private const short BlueElite = 228;
    private const short UpgradeStone = 239;
    private const short DefenseIncrease = 0;
    private const short CritRate = 50;

    private GameConfiguration _gameConfiguration = null!;

    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up a Season 6 configuration with the classes, items, skills and core of Season 21.
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
        await new AddSeason21SkillsUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);
        await new AddSeason21AbilityCoreUpdatePlugIn().ApplyUpdateAsync(context, this._gameConfiguration).ConfigureAwait(false);

        var mapInitializer = new MapInitializer(this._gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var plugInManager = new PlugInManager(null, new NullLoggerFactory(), null, null);
        var gameContext = new GameContext(this._gameConfiguration, contextProvider, mapInitializer, new NullLoggerFactory(), plugInManager, NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        gameContext.FeaturePlugIns.AddPlugIn(new AbilityCoreFeaturePlugIn { Configuration = Season21AbilityCoreInitializer.CreateConfiguration() }, true);
        this._gameContext = gameContext;
    }

    /// <summary>
    /// An option card in an open slot raises its attribute by its value at the card level; taken out, it is gone.
    /// </summary>
    [Test]
    public async ValueTask OptionCardGivesItsOptionAsync()
    {
        var player = await this.CreatePlayerAsync(ClassEvolution.Fifth, 800).ConfigureAwait(false);
        var defense = player.Attributes![Stats.DefenseBase];
        var card = await this.AddCardAsync(player, BlueType4, 2, DefenseIncrease).ConfigureAwait(false);

        var result = await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, AbilityCoreSlots.FirstOption, card.ItemSlot).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(AbilityCoreResult.Success));
        Assert.That(player.Attributes[Stats.DefenseBase], Is.EqualTo(defense + 75.6f).Within(0.01));
        Assert.That(player.Inventory!.Items, Does.Not.Contain(card));

        result = await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Remove, AbilityCoreSlots.FirstOption, AbilityCoreService.AnySlot).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(AbilityCoreResult.Success));
        Assert.That(player.Attributes[Stats.DefenseBase], Is.EqualTo(defense).Within(0.01));
        Assert.That(player.Inventory.Items, Does.Contain(card));
    }

    /// <summary>
    /// The slots need the fifth evolution and their level; a card goes into a slot of its kind and row color only.
    /// </summary>
    [Test]
    public async ValueTask SlotRulesAreCheckedAsync()
    {
        var fourth = await this.CreatePlayerAsync(ClassEvolution.Fourth, 800).ConfigureAwait(false);
        var card = await this.AddCardAsync(fourth, BlueType4, 0, DefenseIncrease).ConfigureAwait(false);
        Assert.That(await AbilityCoreService.RequestAsync(fourth, AbilityCoreAction.Insert, AbilityCoreSlots.FirstOption, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.NotFifthEvolution));

        var player = await this.CreatePlayerAsync(ClassEvolution.Fifth, 800).ConfigureAwait(false);
        card = await this.AddCardAsync(player, BlueType4, 0, DefenseIncrease).ConfigureAwait(false);
        var secondRow = (byte)(AbilityCoreSlots.FirstOption + AbilityCoreSlots.OptionsPerRow);
        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, secondRow, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.SlotLocked));
        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, AbilityCoreSlots.FirstSkill, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.WrongSlot));

        player.Attributes![Stats.MasterLevel] = 850;
        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, secondRow, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.WrongSlot));
        Assert.That(AbilityCoreService.GetOpenSlots(player) & (1u << secondRow), Is.Not.Zero);
    }

    /// <summary>
    /// A skill card needs its fourth evolution skill; in a skill slot it gives its fifth evolution skill.
    /// </summary>
    [Test]
    public async ValueTask SkillCardGivesItsSkillAsync()
    {
        var player = await this.CreatePlayerAsync(ClassEvolution.Fifth, 800).ConfigureAwait(false);
        var card = await this.AddCardAsync(player, MeteorStormCard, 0, 65).ConfigureAwait(false);

        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, AbilityCoreSlots.FirstSkill, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.RequirementMissing));

        await player.SkillList!.AddLearnedSkillAsync(this._gameConfiguration.Skills.Single(s => s.Number == MeteorStormFourth)).ConfigureAwait(false);
        var health = player.Attributes![Stats.MaximumHealth];
        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, AbilityCoreSlots.FirstSkill, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.Success));

        Assert.That(player.SkillList.ContainsSkill((ushort)MeteorStorm), Is.True);
        Assert.That(player.Attributes[Stats.MaximumHealth], Is.EqualTo(health + 25).Within(0.01));

        await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Remove, AbilityCoreSlots.FirstSkill, AbilityCoreService.AnySlot).ConfigureAwait(false);
        Assert.That(player.SkillList.ContainsSkill((ushort)MeteorStorm), Is.False);
    }

    /// <summary>
    /// An elite card needs the five option cards of its row, and no higher level than the lowest of them.
    /// </summary>
    [Test]
    public async ValueTask EliteCardNeedsAFullRowAsync()
    {
        var player = await this.CreatePlayerAsync(ClassEvolution.Fifth, 800).ConfigureAwait(false);
        var elite = await this.AddCardAsync(player, BlueElite, 2, CritRate).ConfigureAwait(false);

        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, AbilityCoreSlots.FirstElite, elite.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.RequirementMissing));

        foreach (var slot in AbilityCoreSlots.GetOptionSlots(0))
        {
            var card = await this.AddCardAsync(player, BlueType1, 1, 15).ConfigureAwait(false);
            Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, slot, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.Success));
        }

        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, AbilityCoreSlots.FirstElite, elite.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.EliteLevelTooHigh));

        elite.Level = 1;
        var chance = player.Attributes![Stats.CriticalDamageChance];
        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Insert, AbilityCoreSlots.FirstElite, elite.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.Success));
        Assert.That(player.Attributes[Stats.CriticalDamageChance], Is.EqualTo(chance + 0.038f).Within(0.0001));
        Assert.That(AbilityCoreService.GetActiveSlots(player) & (1u << AbilityCoreSlots.FirstElite), Is.Not.Zero);
    }

    /// <summary>
    /// A level up takes other cards of the same item, zen and stones; a reduction gives stones for zen.
    /// </summary>
    [Test]
    public async ValueTask LevelUpAndReductionUseTheirPricesAsync()
    {
        var player = await this.CreatePlayerAsync(ClassEvolution.Fifth, 800).ConfigureAwait(false);
        player.Money = 50_000_000;
        var card = await this.AddCardAsync(player, BlueType4, 0, DefenseIncrease).ConfigureAwait(false);

        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.LevelUp, 0, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.NotEnoughCards));

        var materials = new[]
        {
            await this.AddCardAsync(player, BlueType4, 0, 12).ConfigureAwait(false),
            await this.AddCardAsync(player, BlueType4, 0, 18).ConfigureAwait(false),
        };
        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.LevelUp, 0, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.NotEnoughStones));

        await this.AddItemAsync(player, UpgradeStone, 1).ConfigureAwait(false);
        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.LevelUp, 0, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.Success));

        Assert.That(card.Level, Is.EqualTo(1));
        Assert.That(player.Money, Is.EqualTo(45_000_000));
        Assert.That(player.Inventory!.Items, Is.EquivalentTo(new[] { card }));
        Assert.That(materials.Select(m => m.Definition), Has.All.Not.Null);

        Assert.That(await AbilityCoreService.RequestAsync(player, AbilityCoreAction.Reduce, 0, card.ItemSlot).ConfigureAwait(false), Is.EqualTo(AbilityCoreResult.Success));

        Assert.That(player.Money, Is.EqualTo(35_000_000));
        var stone = player.Inventory.Items.Single();
        Assert.That(stone.Definition!.Number, Is.EqualTo(UpgradeStone));
        Assert.That(stone.Durability, Is.EqualTo(1));
    }

    private async ValueTask<Item> AddCardAsync(Player player, short number, byte level, short option)
    {
        var item = await this.AddItemAsync(player, number, 1).ConfigureAwait(false);
        item.Level = level;
        var itemOption = item.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions)
            .Single(o => o.OptionType == Season21ItemOptionTypes.AbilityCardOption && o.Number == option);
        var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
        link.ItemOption = itemOption;
        item.ItemOptions.Add(link);
        return item;
    }

    private async ValueTask<Item> AddItemAsync(Player player, short number, double durability)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = this._gameConfiguration.Items.Single(d => d.Group == CardGroup && d.Number == number);
        item.Durability = durability;
        Assert.That(await player.Inventory!.AddItemAsync(item).ConfigureAwait(false), Is.True);
        return item;
    }

    private async ValueTask<Player> CreatePlayerAsync(ClassEvolution evolution, int masterLevel)
    {
        var player = new InGamePlayer(this._gameContext);
        player.Account = player.PersistenceContext.CreateNew<Account>();
        player.Account.LoginName = Guid.NewGuid().ToString("N")[..10];
        await player.PlayerState.TryAdvanceToAsync(PlayerState.LoginScreen).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.Authenticated).ConfigureAwait(false);
        await player.PlayerState.TryAdvanceToAsync(PlayerState.CharacterSelection).ConfigureAwait(false);
        var first = ClassCodes.FromCode(ClassCodes.GetCode(BaseClassCode.DarkWizard, ClassEvolution.First))!.Value;
        await new CreateCharacterAction().CreateCharacterAsync(player, "A" + Guid.NewGuid().ToString("N")[..8], first).ConfigureAwait(false);
        var character = player.Account.Characters.Single();
        character.CharacterClass = this._gameConfiguration.FindByClassCode(ClassCodes.GetCode(BaseClassCode.DarkWizard, evolution));
        await player.SetSelectedCharacterAsync(character).ConfigureAwait(false);
        foreach (var item in player.Inventory!.Items.ToList())
        {
            await player.Inventory.RemoveItemAsync(item).ConfigureAwait(false);
        }

        player.Attributes![Stats.Level] = 400;
        player.Attributes[Stats.MasterLevel] = masterLevel;
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
