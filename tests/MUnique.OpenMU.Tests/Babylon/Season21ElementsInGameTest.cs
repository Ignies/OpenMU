// <copyright file="Season21ElementsInGameTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.Craftings;
using MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;
using MonsterAttribute = MUnique.OpenMU.Persistence.BasicModel.MonsterAttribute;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;

/// <summary>
/// Tests the elements, pentagrams, errtels and earrings of Season 21 in the game: elemental damage both ways, a
/// pentagram with an errtel equipped, an earring with its option and its link, and the W32 format which carries them.
/// </summary>
[TestFixture]
public class Season21ElementsInGameTest
{
    private const byte Group = 12;
    private const short BeginnersPentagram = 215;
    private const short AngerErrtel = 221;
    private const short LeftWrathEarring = 449;
    private const short RightWrathEarring = 457;

    private GameConfiguration _gameConfiguration = null!;
    private IPersistenceContextProvider _contextProvider = null!;
    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up a Season 6 configuration with the items, the elements and the earrings of Season 21 and turns the
    /// elements feature on.
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
                new AddSeason21ElementsUpdatePlugIn(),
                new AddSeason21EarringsUpdatePlugIn(),
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
        gameContext.PlugInManager.RegisterPlugIn<IFeaturePlugIn, ElementsFeaturePlugIn>();
        gameContext.PlugInManager.ActivatePlugIn<ElementsFeaturePlugIn>();
        gameContext.FeaturePlugIns.GetPlugIn<ElementsFeaturePlugIn>()!.Configuration = this._gameConfiguration.PlugInConfigurations
            .Single(c => c.TypeId == typeof(ElementsFeaturePlugIn).GUID)
            .GetConfiguration<ElementsConfiguration>(null);
        this._gameContext = gameContext;
    }

    /// <summary>
    /// A water monster with an elemental damage of 100 deals 120 to a character without pentagram (the wheel's 120 %
    /// against no element) and 95 to one with a fire pentagram for beginners: 120 % water against fire, minus the
    /// pentagram's elemental defense of 25. Its elemental defense rate of 5 against the monster's attack rate of
    /// 1,000 lets 99.5 % of the elemental hits through.
    /// </summary>
    [Test]
    public async ValueTask MonsterDealsElementalDamageToPlayersAsync()
    {
        var monster = await this.CreateMonsterAsync(Element.Water, damage: 100, defense: 0).ConfigureAwait(false);
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);

        var hit = await monster.CalculateDamageAsync(player, null, false).ConfigureAwait(false);
        Assert.That(hit.ElementalDamage, Is.EqualTo(120));
        Assert.That(hit.Element, Is.EqualTo(Element.Water));
        Assert.That(hit.TotalDamage, Is.GreaterThanOrEqualTo(120), "the elemental damage is a part of the hit");

        await this.EquipAsync(player, this.CreatePentagram(player, Element.Fire)).ConfigureAwait(false);
        Assert.That(player.Attributes![Stats.Element], Is.EqualTo((float)Element.Fire));
        uint elemental = 0;
        for (var tries = 0; tries < 20 && elemental == 0; tries++)
        {
            elemental = (await monster.CalculateDamageAsync(player, null, false).ConfigureAwait(false)).ElementalDamage;
        }

        Assert.That(elemental, Is.EqualTo((100 * 1.2) - 25).Within(1));
    }

    /// <summary>
    /// A character with a fire pentagram (25-40) and an errtel of anger (+30 at level 0) deals (55-70) x 90 % (fire
    /// against wind) minus 10 to a wind monster; a character without pentagram deals no elemental damage.
    /// </summary>
    [Test]
    public async ValueTask PlayerDealsElementalDamageToMonstersAsync()
    {
        var monster = await this.CreateMonsterAsync(Element.Wind, damage: 0, defense: 10).ConfigureAwait(false);
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        Assert.That((await player.CalculateDamageAsync(monster, null, false).ConfigureAwait(false)).ElementalDamage, Is.Zero, "no pentagram");

        var pentagram = this.CreatePentagram(player, Element.Fire);
        await this.EquipAsync(player, pentagram).ConfigureAwait(false);
        await this.InsertErrtelAsync(player, pentagram).ConfigureAwait(false);

        var hit = await player.CalculateDamageAsync(monster, null, false).ConfigureAwait(false);
        Assert.That(hit.ElementalDamage, Is.InRange((uint)(((25 + 30) * 0.9) - 10), (uint)(((40 + 30) * 0.9) - 10)));
        Assert.That(hit.Element, Is.EqualTo(Element.Fire));
    }

    /// <summary>
    /// An errtel goes into the socket of its kind of a pentagram of its element; equipped, the pentagram gives the
    /// errtel's option; taken out, the errtel comes back with its options and the pentagram loses them.
    /// </summary>
    [Test]
    public async ValueTask PentagramWithErrtelAppliesItsOptionsAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var pentagram = this.CreatePentagram(player, Element.Fire);
        await this.EquipAsync(player, pentagram).ConfigureAwait(false);
        Assert.That(player.Attributes![Stats.ElementDamageMin], Is.EqualTo(25));
        Assert.That(player.Attributes[Stats.ElementDamageBonus], Is.Zero);

        var wrongElement = await this.AddErrtelAsync(player, Element.Water).ConfigureAwait(false);
        var action = new ErrtelSocketAction();
        Assert.That(await action.HandleAsync(player, ErrtelSocketMode.Insert, InventoryConstants.PentagramSlot, wrongElement.ItemSlot, 0).ConfigureAwait(false), Is.EqualTo(ErrtelSocketResult.Refused), "water errtel, fire pentagram");

        await this.InsertErrtelAsync(player, pentagram).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.ElementDamageBonus], Is.EqualTo(30), "rank 1, level 0");
        Assert.That(pentagram.GetRankOptions(0).Single().ItemOption!.Number, Is.EqualTo(0));
        Assert.That(WideItemRules.IsWideOnly(pentagram), Is.True);

        Assert.That(await action.HandleAsync(player, ErrtelSocketMode.Remove, InventoryConstants.PentagramSlot, 0, 0).ConfigureAwait(false), Is.EqualTo(ErrtelSocketResult.Success));
        Assert.That(player.Attributes[Stats.ElementDamageBonus], Is.Zero);
        var errtel = player.Inventory!.Items.Single(i => i.Definition!.Number == AngerErrtel && i != wrongElement);
        Assert.That(errtel.GetElement(), Is.EqualTo(Element.Fire));
        Assert.That(errtel.GetRankOptions().Single().Level, Is.EqualTo(0));
    }

    /// <summary>
    /// An earring applies its option (maximum health +200), and a left and right earring of one tier add their link
    /// options (enemy defense ignore +1 %, all stats +4).
    /// </summary>
    [Test]
    public async ValueTask EarringOptionAndLinkApplyAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var health = player.Attributes![Stats.MaximumHealth];
        var left = this.CreateItem(player, this.Item(LeftWrathEarring));
        AddOption(left, Season21ItemOptionTypes.EarringOption, 11);

        await this.EquipAsync(player, left).ConfigureAwait(false);
        Assert.That(left.ItemSlot, Is.EqualTo(InventoryConstants.LeftEarringSlot));
        Assert.That(player.Attributes[Stats.MaximumHealth] - health, Is.EqualTo(200).Within(0.01));
        Assert.That(player.Attributes[Stats.DefenseIgnoreChance], Is.Zero, "one earring is no pair");

        await this.EquipAsync(player, this.CreateItem(player, this.Item(RightWrathEarring))).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.DefenseIgnoreChance], Is.EqualTo(0.01f).Within(0.0001));
        Assert.That(player.Attributes[Stats.AllStatsBonus], Is.EqualTo(4));
    }

    /// <summary>
    /// The W32 format carries the element and the socketed errtels of a pentagram, the rank options of an errtel
    /// and the options of an earring, and reads them back.
    /// </summary>
    [Test]
    public async ValueTask W32CarriesElementsErrtelsAndEarringsAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var w32 = new ItemSerializerW32();
        var data = new byte[ItemSerializerW32.Length];

        var errtel = await this.AddErrtelAsync(player, Element.Earth).ConfigureAwait(false);
        w32.SerializeItem(data, errtel);
        Assert.That(data[21], Is.EqualTo((byte)Element.Earth | (2 << 4)), "earth, errtel of anger");
        Assert.That(data[13..18], Is.EqualTo(new byte[] { 0, 0xFF, 0xFF, 0xFF, 0xFF }), "rank 1: option 0");
        Assert.That(data[22..24], Is.EqualTo(new byte[] { 1, 0 }), "one rank, socket 0");
        var readErrtel = w32.DeserializeItem(data, this._gameConfiguration, player.PersistenceContext);
        Assert.That(readErrtel.GetElement(), Is.EqualTo(Element.Earth));
        Assert.That(readErrtel.GetRankOptions().Single().ItemOption!.Number, Is.EqualTo(0));

        var pentagram = this.CreatePentagram(player, Element.Fire);
        await this.EquipAsync(player, pentagram).ConfigureAwait(false);
        await this.InsertErrtelAsync(player, pentagram).ConfigureAwait(false);
        data.AsSpan().Clear();
        w32.SerializeItem(data, pentagram);
        Assert.That(data[21], Is.EqualTo((byte)Element.Fire | (1 << 4)), "fire pentagram");
        Assert.That(data[13..18], Is.EqualTo(new byte[] { 1, 0xFE, 0xFE, 0xFE, 0xFE }), "an errtel of one rank in socket 0, four empty");
        var readPentagram = w32.DeserializeItem(data, this._gameConfiguration, player.PersistenceContext);
        Assert.That(readPentagram.SocketCount, Is.EqualTo(5));
        Assert.That(readPentagram.GetElement(), Is.EqualTo(Element.Fire));

        var earring = this.CreateItem(player, this.Item(RightWrathEarring));
        AddOption(earring, Season21ItemOptionTypes.EarringOption, 9);
        AddOption(earring, Season21ItemOptionTypes.EarringOption, 12);
        data.AsSpan().Clear();
        w32.SerializeItem(data, earring);
        Assert.That(data[22..28], Is.EqualTo(new byte[] { 2, 9, 12, 0xFF, 0xFF, 0xFF }));
        var readEarring = w32.DeserializeItem(data, this._gameConfiguration, player.PersistenceContext);
        Assert.That(readEarring.ItemOptions.Select(o => (int)o.ItemOption!.Number), Is.EquivalentTo(new[] { 9, 12 }));
        Assert.That(WideItemRules.IsWideOnly(earring), Is.True);
    }

    /// <summary>
    /// The refinery: an errtel of level 0 and rank 1 with one elemental rune levels up to 1; at level 0 it can't rank
    /// up (rank 1 needs level 6); Arca's Prophecy +11 with an emblem and packed jewels becomes Antonia's Sword [I] at
    /// 80 % and keeps its element and sockets.
    /// </summary>
    [Test]
    public async ValueTask RefineryLevelsErrtelsAndUpgradesPentagramsAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var configuration = ElementsFeaturePlugIn.GetConfiguration(this._gameContext)!;
        configuration.LevelUps = [new ErrtelLevelUp { Level = 0, Rates = [100], Runes = [1] }];
        configuration.RankUps = [new ErrtelRankUp { Rank = 1, Rates = [0, 0, 0, 0, 0, 0, 70] }];

        var errtel = this.CreateItem(player, this.Item(AngerErrtel));
        AddOption(errtel, Season21ItemOptionTypes.Element, (int)Element.Fire);
        AddOption(errtel, Season21ItemOptionTypes.ErrtelRank, 0);
        await player.TemporaryStorage!.AddItemAsync(errtel).ConfigureAwait(false);
        Assert.That(new TestRankUp(this.CraftingSettings("Errtel Rank Up")).TryGetRequiredItems(player, out _, out _), Is.EqualTo(CraftingResult.IncorrectMixItems), "level 0");

        var rune = this.CreateItem(player, this.Item(288));
        rune.Durability = 1;
        await player.TemporaryStorage.AddItemAsync(rune).ConfigureAwait(false);
        var levelUp = new TestLevelUp(this.CraftingSettings("Errtel Level Up (Level 1)"));
        Assert.That(levelUp.TryGetRequiredItems(player, out var items, out var rate), Is.Null);
        Assert.That(rate, Is.EqualTo(100));
        await levelUp.CreateAsync(items, player).ConfigureAwait(false);
        Assert.That(errtel.GetRankOptions().Single().Level, Is.EqualTo(1));
        await player.TemporaryStorage.RemoveItemAsync(rune).ConfigureAwait(false);
        await player.TemporaryStorage.RemoveItemAsync(errtel).ConfigureAwait(false);

        var pentagram = this.CreateItem(player, this.Item(212));
        pentagram.SocketCount = 3;
        AddOption(pentagram, Season21ItemOptionTypes.Element, (int)Element.Wind);
        var upgrade = new TestUpgrade(this.CraftingSettings("Arca's Prophecy Upgrade"));
        foreach (var required in this.CraftingSettings("Arca's Prophecy Upgrade").RequiredItems)
        {
            var material = required.Reference == ElementalUpgradeCrafting.MainItemReference ? pentagram : this.CreateItem(player, required.PossibleItems.Single());
            material.Level = required.Reference == ElementalUpgradeCrafting.MainItemReference ? (byte)11 : (byte)required.MinimumItemLevel;
            await player.TemporaryStorage.AddItemAsync(material).ConfigureAwait(false);
        }

        Assert.That(upgrade.TryGetRequiredItems(player, out items, out rate), Is.Null);
        Assert.That(rate, Is.EqualTo(80), "+11: 80 %");
        await upgrade.CreateAsync(items, player).ConfigureAwait(false);
        Assert.That(pentagram.Definition!.Number, Is.EqualTo(503));
        Assert.That(pentagram.GetElement(), Is.EqualTo(Element.Wind));
        Assert.That(pentagram.SocketCount, Is.EqualTo(3));
    }

    /// <summary>
    /// A critical elemental hit deals the maximum elemental damage (40 of the beginner's pentagram, x 120 % against no
    /// element); punish adds 6 % of the target's maximum health; a perfect dodge takes none; 5 % of the defense adds to
    /// the elemental defense.
    /// </summary>
    [Test]
    public async ValueTask CriticalPunishDodgeAndDefenseShareAsync()
    {
        var attacker = await this.CreatePlayerAsync().ConfigureAwait(false);
        var defender = await this.CreatePlayerAsync().ConfigureAwait(false);
        await this.EquipAsync(attacker, this.CreatePentagram(attacker, Element.Fire)).ConfigureAwait(false);
        var configuration = new ElementsConfiguration { RadianceChance = 1 };
        Add(attacker, Stats.ElementCriticalChance, 1);
        uint Hit() => ElementalDamageCalculator.Calculate(attacker.Attributes!, Element.Fire, defender.Attributes!, Element.None, false, 100, configuration);

        Assert.That(Hit(), Is.EqualTo(48), "critical: 40 x 120 %");
        Add(defender, Stats.DefensePvm, 100);
        Add(defender, Stats.ElementDefenseFromDefense, 0.05f);
        Assert.That(Hit(), Is.EqualTo(48 - 5), "5 % of the defense of 100");
        Add(attacker, Stats.ElementPunish, 0.06f);
        Assert.That(Hit(), Is.EqualTo(43 + (uint)(defender.Attributes![Stats.MaximumHealth] * 0.06f)), "punish");
        Add(defender, Stats.ElementDodgeChance, 1);
        Assert.That(Hit(), Is.Zero, "perfect dodge");
    }

    /// <summary>
    /// The Iron Shield of the Magic (203) has the pentagram option 3 (pentagram defense +10 %), which needs an errtel of
    /// anger of rank 1: it is active while the errtel is in, and gone when it is taken out.
    /// </summary>
    [Test]
    public async ValueTask PentagramOptionNeedsItsErrtelsAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var pentagram = this.CreateItem(player, this.Item(203));
        pentagram.SocketCount = 4;
        AddOption(pentagram, Season21ItemOptionTypes.Element, (int)Element.Fire);
        await this.EquipAsync(player, pentagram).ConfigureAwait(false);
        Assert.That(player.Attributes![Stats.ElementDefenseIncrease], Is.Zero);

        await this.InsertErrtelAsync(player, pentagram).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.ElementDefenseIncrease], Is.EqualTo(0.1f).Within(0.0001));

        await new ErrtelSocketAction().HandleAsync(player, ErrtelSocketMode.Remove, pentagram.ItemSlot, 0, 0).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.ElementDefenseIncrease], Is.Zero);
    }

    /// <summary>
    /// Two mastery errtels of the set "Fighting spirit" in a mastery pentagram give its two-piece bonus, elemental
    /// damage +40 (ErrtelMastery.xml; worldofmu.net: "2-Piece Effect increases Elemental ATK +40").
    /// </summary>
    [Test]
    public async ValueTask ErrtelSetBonusNeedsTwoErrtelsOfTheSetAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var pentagram = this.CreateItem(player, this.Item(503));
        pentagram.SocketCount = 4;
        AddOption(pentagram, Season21ItemOptionTypes.Element, (int)Element.Fire);
        await this.EquipAsync(player, pentagram).ConfigureAwait(false);
        var action = new ErrtelSocketAction();
        foreach (var (number, socket) in new[] { ((short)505, (byte)0), ((short)506, (byte)1) })
        {
            var errtel = this.CreateItem(player, this.Item(number));
            AddOption(errtel, Season21ItemOptionTypes.Element, (int)Element.Fire);
            AddOption(errtel, Season21ItemOptionTypes.ErrtelRank, number == 505 ? 0 : 50);
            AddOption(errtel, Season21ItemOptionTypes.ErrtelSet, 0);
            await player.Inventory!.AddItemAsync(errtel).ConfigureAwait(false);
            Assert.That(await action.HandleAsync(player, ErrtelSocketMode.Insert, pentagram.ItemSlot, errtel.ItemSlot, socket).ConfigureAwait(false), Is.EqualTo(ErrtelSocketResult.Success));
        }

        Assert.That(player.Attributes![Stats.ElementDamageBonus], Is.EqualTo(93 + 40), "anger [I] rank 1 + the set");
    }

    /// <summary>
    /// Absorb life of the radiance errtel gives the attacker its share of the elemental damage as health.
    /// </summary>
    [Test]
    public async ValueTask AbsorbLifeHealsTheAttackerAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        Add(player, Stats.ElementAbsorbLife, 0.1f);
        player.Attributes![Stats.CurrentHealth] = 10;
        var plugIn = new ElementsFeaturePlugIn { Configuration = new ElementsConfiguration { RadianceChance = 1 } };

        plugIn.ApplyAbsorb(player, 100);

        Assert.That(player.Attributes[Stats.CurrentHealth], Is.EqualTo(20));
    }

    /// <summary>
    /// The Expansion Scroll of Radiance slot opens the fifth socket of a pentagram with four sockets, not of others.
    /// </summary>
    [Test]
    public async ValueTask RadianceScrollOpensTheFifthSocketAsync()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var pentagram = this.CreateItem(player, this.Item(BeginnersPentagram));
        pentagram.SocketCount = 4;
        await player.Inventory!.AddItemAsync(pentagram).ConfigureAwait(false);
        var handler = new RadianceSlotScrollConsumeHandlerPlugIn();
        var scroll = this.CreateItem(player, this._gameConfiguration.Items.Single(i => i.Group == 14 && i.Number == 316));
        await player.Inventory.AddItemAsync(scroll).ConfigureAwait(false);

        Assert.That(await handler.ConsumeItemAsync(player, scroll, pentagram, default).ConfigureAwait(false), Is.True);
        Assert.That(pentagram.SocketCount, Is.EqualTo(5));
        Assert.That(await handler.ConsumeItemAsync(player, scroll, pentagram, default).ConfigureAwait(false), Is.False, "already five");
    }

    /// <summary>
    /// The earring enhancement: +0 to +1 takes one lapidary stone at 60 % (EarringItemOption.txt section 4); a failure
    /// above +7 resets the earring to +0 (Webzen guide 150).
    /// </summary>
    [Test]
    public async ValueTask EarringEnhancementRaisesTheLevelAndResetsAbove7Async()
    {
        var player = await this.CreatePlayerAsync().ConfigureAwait(false);
        var earring = this.CreateItem(player, this.Item(LeftWrathEarring));
        await player.TemporaryStorage!.AddItemAsync(earring).ConfigureAwait(false);
        var stone = this.CreateItem(player, this._gameConfiguration.Items.Single(i => i.Group == 13 && i.Number == 510));
        stone.Durability = 1;
        await player.TemporaryStorage.AddItemAsync(stone).ConfigureAwait(false);
        var first = this.CraftingSettings("Earring Enhancement (+1)");
        Assert.That((first.SuccessPercent, first.Money), Is.EqualTo((60, 1_000_000)));
        var enhance = new TestEnhance(first);
        Assert.That(enhance.TryGetRequiredItems(player, out var items, out _), Is.Null);
        await enhance.CreateAsync(items, player).ConfigureAwait(false);
        Assert.That(earring.Level, Is.EqualTo(1));

        var ninth = this.CraftingSettings("Earring Enhancement (+9)");
        Assert.That(ninth.RequiredItems.Single(r => r.Reference > 0).FailResult, Is.EqualTo(MixResult.LevelResetToZero));
        Assert.That(this.CraftingSettings("Earring Enhancement (+7)").RequiredItems.Single(r => r.Reference > 0).FailResult, Is.EqualTo(MixResult.StaysAsIs));
    }

    /// <summary>
    /// ElementInfo carries the elemental stats of the own character.
    /// </summary>
    [Test]
    public async ValueTask ElementInfoCarriesTheStatsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(GameServer.BabylonClient.Version);
        var attributes = new AttributeSystem.AttributeSystem([], [], []);
        attributes.AddElement(new AttributeSystem.ConstValueAttribute(3, Stats.Element), Stats.Element);
        attributes.AddElement(new AttributeSystem.ConstValueAttribute(25, Stats.ElementDamageMin), Stats.ElementDamageMin);
        attributes.AddElement(new AttributeSystem.ConstValueAttribute(0.1f, Stats.ElementCriticalChance), Stats.ElementCriticalChance);

        await new GameServer.RemoteView.Character.ElementInfoBabylonPlugIn(player).ShowElementInfoAsync(attributes).ConfigureAwait(false);

        var packet = new Network.Packets.Babylon.ElementInfo(output.ToArray());
        Assert.That((packet.Element, packet.DamageMin, packet.CriticalChance), Is.EqualTo(((byte)3, 25u, (byte)10)));
    }

    private static void Add(Player player, AttributeSystem.AttributeDefinition attribute, float value)
        => player.Attributes!.AddElement(new AttributeSystem.ConstValueAttribute(value, attribute), attribute);

    private static void AddOption(Item item, ItemOptionType type, int number, int level = 0, int index = 0)
    {
        var option = item.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).First(o => o.OptionType == type && o.Number == number);
        item.ItemOptions.Add(new Persistence.BasicModel.ItemOptionLink { ItemOption = option, Level = level, Index = index });
    }

    private async ValueTask<Player> CreatePlayerAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        Assert.That(await player.PlayerState.TryAdvanceToAsync(PlayerState.EnteredWorld).ConfigureAwait(false), Is.True);
        return player;
    }

    private ItemDefinition Item(short number) => this._gameConfiguration.Items.Single(i => i.Group == Group && i.Number == number);

    private Item CreateItem(Player player, ItemDefinition definition)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = definition;
        item.Durability = Math.Max((byte)1, definition.Durability);
        return item;
    }

    private Item CreatePentagram(Player player, Element element)
    {
        var pentagram = this.CreateItem(player, this.Item(BeginnersPentagram));
        pentagram.SocketCount = Pentagrams.Sockets;
        AddOption(pentagram, Season21ItemOptionTypes.Element, (int)element);
        return pentagram;
    }

    private async ValueTask<Item> AddErrtelAsync(Player player, Element element)
    {
        var errtel = this.CreateItem(player, this.Item(AngerErrtel));
        AddOption(errtel, Season21ItemOptionTypes.Element, (int)element);
        AddOption(errtel, Season21ItemOptionTypes.ErrtelRank, 0);
        Assert.That(await player.Inventory!.AddItemAsync(errtel).ConfigureAwait(false), Is.True);
        return errtel;
    }

    private async ValueTask InsertErrtelAsync(Player player, Item pentagram)
    {
        var errtel = await this.AddErrtelAsync(player, pentagram.GetElement()).ConfigureAwait(false);
        var result = await new ErrtelSocketAction().HandleAsync(player, ErrtelSocketMode.Insert, pentagram.ItemSlot, errtel.ItemSlot, 0).ConfigureAwait(false);
        Assert.That(result, Is.EqualTo(ErrtelSocketResult.Success));
    }

    private async ValueTask EquipAsync(Player player, Item item)
    {
        var slot = item.Definition!.ItemSlot!.ItemSlots.First();
        Assert.That(await player.Inventory!.AddItemAsync((byte)slot, item).ConfigureAwait(false), Is.True);
    }

    private async ValueTask<Monster> CreateMonsterAsync(Element element, int damage, int defense)
    {
        var definition = new MonsterDefinition { Id = Guid.NewGuid(), Number = 999, Designation = "Element Test", ObjectKind = NpcObjectKind.Monster };
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MaximumHealth, Value = 1_000_000 });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.Level, Value = 10 });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.Element, Value = (float)element });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.ElementDamageMin, Value = damage });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.ElementDamageMax, Value = damage });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.ElementAttackRate, Value = 1000 });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.ElementDefense, Value = defense });
        var map = await this._gameContext.GetMapAsync(0).ConfigureAwait(false);
        var area = new MonsterSpawnArea { MonsterDefinition = definition, GameMap = map!.Definition, SpawnTrigger = SpawnTrigger.OnceAtEventStart, X1 = 170, Y1 = 90, X2 = 200, Y2 = 120, Quantity = 1 };
        var monster = new Monster(area, definition, map, NullDropGenerator.Instance, new BasicMonsterIntelligence(), this._gameContext.PlugInManager, this._gameContext.PathFinderPool);
        monster.Initialize();
        await map.AddAsync(monster).ConfigureAwait(false);
        return monster;
    }

    private SimpleCraftingSettings CraftingSettings(string name)
        => this._gameConfiguration.Monsters.SelectMany(m => m.ItemCraftings).Single(c => c.Name == name).SimpleCraftingSettings!;

    private sealed class TestLevelUp(SimpleCraftingSettings settings) : ErrtelLevelUpCrafting(settings)
    {
        public ValueTask<List<Item>> CreateAsync(IList<CraftingRequiredItemLink> items, Player player) => this.CreateOrModifyResultItemsAsync(items, player, 0, 100);
    }

    private sealed class TestRankUp(SimpleCraftingSettings settings) : ErrtelRankUpCrafting(settings);

    private sealed class TestUpgrade(SimpleCraftingSettings settings) : ElementalUpgradeCrafting(settings)
    {
        public ValueTask<List<Item>> CreateAsync(IList<CraftingRequiredItemLink> items, Player player) => this.CreateOrModifyResultItemsAsync(items, player, 0, 100);
    }

    private sealed class TestEnhance(SimpleCraftingSettings settings) : SimpleItemCraftingHandler(settings)
    {
        public ValueTask<List<Item>> CreateAsync(IList<CraftingRequiredItemLink> items, Player player) => this.CreateOrModifyResultItemsAsync(items, player, 0, 100);
    }
}
