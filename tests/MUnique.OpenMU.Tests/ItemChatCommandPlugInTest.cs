// <copyright file="ItemChatCommandPlugInTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Moq;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Administration;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// Tests the item chat command of game masters.
/// </summary>
[TestFixture]
public class ItemChatCommandPlugInTest
{
    /// <summary>
    /// An inactive item isn't available in the game, so a game master can't make it either: e.g. an item which the
    /// item format of the client can't carry yet.
    /// </summary>
    [Test]
    public async Task InactiveItemIsNotCreatedAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.CharacterStatus = CharacterStatus.GameMaster;
        player.CurrentMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
        player.GameContext.Configuration.Items.Add(new ItemDefinition { Group = 16, Number = 1, Width = 1, Height = 1, Durability = 1, IsActive = false, Name = "Muun" });

        await new ItemChatCommandPlugIn().HandleCommandAsync(player, "/item 16 1").ConfigureAwait(false);

        Mock.Get(player.ViewPlugIns.GetPlugIn<IShowMessagePlugIn>()!).Verify(v => v!.ShowMessageAsync("16 1 does not exist.", MessageType.BlueNormal), Times.Once);
    }

    /// <summary>
    /// A socket item gets the number of sockets and the seeds of <c>sck</c> and <c>seeds</c>: each seed by its index over
    /// all elements (as the W32 item carries it) and its sphere level, into the sockets in order.
    /// </summary>
    [Test]
    public async Task SocketsAndSeedsAreSetAsync()
    {
        var (player, sword) = await CreateAsync(5).ConfigureAwait(false);
        var fire = AddOption(player, sword, ItemOptionTypes.SocketOption, 1, 0);
        var water = AddOption(player, sword, ItemOptionTypes.SocketOption, 2, 1);
        var item = Create(sword);

        Assert.That(GameMasterItemOptions.Apply(item, new GameMasterItemExtras(Sockets: 3, Seeds: "1:3,12"), player.GameContext.Configuration, null), Is.Null);

        Assert.That(item.SocketCount, Is.EqualTo(3));
        Assert.That(item.ItemOptions.Select(o => (o.ItemOption, o.Index, o.Level)), Is.EquivalentTo(new[] { (fire, 0, 3), (water, 1, 1) }));
        Assert.That(GameMasterItemOptions.Apply(Create(sword), new GameMasterItemExtras(Sockets: 1, Seeds: "1,1"), player.GameContext.Configuration, null), Is.EqualTo("1"), "more seeds than sockets");
        Assert.That(GameMasterItemOptions.Apply(Create(sword), new GameMasterItemExtras(Seeds: "9"), player.GameContext.Configuration, null), Is.EqualTo("9"), "no such seed");
    }

    /// <summary>
    /// A pentagram gets its element and the errtels of <c>errtels</c> in its sockets: socket, number of ranks and level.
    /// </summary>
    [Test]
    public async Task PentagramGetsElementAndErrtelsAsync()
    {
        var (player, pentagram) = await CreateAsync(5).ConfigureAwait(false);
        pentagram.ItemSlot = player.PersistenceContext.CreateNew<ItemSlotType>();
        pentagram.ItemSlot.ItemSlots.Add(InventoryConstants.PentagramSlot);
        AddOption(player, pentagram, Season21ItemOptionTypes.Element, 3, 0);
        var errtel = player.PersistenceContext.CreateNew<ItemDefinition>();
        (errtel.Group, errtel.Number, errtel.IsActive) = (12, 221, true);
        player.GameContext.Configuration.Items.Add(errtel);
        var rank1 = AddOption(player, errtel, Season21ItemOptionTypes.ErrtelRank, 0, 1);
        var rank2 = AddOption(player, errtel, Season21ItemOptionTypes.ErrtelRank, 1, 2);
        var item = Create(pentagram);

        Assert.That(GameMasterItemOptions.Apply(item, new GameMasterItemExtras(Errtels: "0:2:7", Options: "element:3"), player.GameContext.Configuration, null), Is.Null);

        Assert.That(item.GetElement(), Is.EqualTo(Element.Earth));
        Assert.That(item.GetRankOptions(0).Select(o => (o.ItemOption, o.Level)), Is.EqualTo(new[] { (rank1, 7), (rank2, 7) }));
        Assert.That(GameMasterItemOptions.Apply(Create(pentagram), new GameMasterItemExtras(Errtels: "1:1"), player.GameContext.Configuration, null), Is.EqualTo("1:1"), "no errtel of the blessing");
    }

    /// <summary>
    /// The options of <c>opts</c> are options of the item definition by kind and number, with a level: mastery options
    /// and the mastery bonus here; one the item can't have is refused. <c>muun</c> starts the additional option of a Muun.
    /// </summary>
    [Test]
    public async Task MasteryAndMuunOptionsAreSetAsync()
    {
        var (player, armor) = await CreateAsync(0).ConfigureAwait(false);
        var health = AddOption(player, armor, Season21ItemOptionTypes.Mastery, 6, 0);
        var bonus = AddOption(player, armor, Season21ItemOptionTypes.MasteryBonus, 1, 0);
        var item = Create(armor);

        Assert.That(GameMasterItemOptions.Apply(item, new GameMasterItemExtras(Options: "mastery:6,bonus:1:2", MuunOptionDays: 3), player.GameContext.Configuration, null), Is.Null);

        Assert.That(item.ItemOptions.Select(o => (o.ItemOption, o.Level)), Is.EquivalentTo(new[] { (health, 0), (bonus, 2) }));
        Assert.That(item.TimedOptionExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(3)).Within(TimeSpan.FromMinutes(1)));
        Assert.That(GameMasterItemOptions.Apply(Create(armor), new GameMasterItemExtras(Options: "mastery:9"), player.GameContext.Configuration, null), Is.EqualTo("mastery:9"));
    }

    /// <summary>
    /// The chat command passes its new arguments on; an entry which doesn't fit the item is named and nothing is made.
    /// </summary>
    [Test]
    public async Task CommandRefusesAnUnfitEntryAsync()
    {
        var (player, sword) = await CreateAsync(5).ConfigureAwait(false);
        player.SelectedCharacter!.CharacterStatus = CharacterStatus.GameMaster;
        player.CurrentMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
        var messages = Mock.Get(player.ViewPlugIns.GetPlugIn<IShowMessagePlugIn>()!);

        await new ItemChatCommandPlugIn().HandleCommandAsync(player, $"/item group={sword.Group} number={sword.Number} seeds=9").ConfigureAwait(false);

        messages.Verify(v => v.ShowMessageAsync(It.Is<string>(m => m.Contains("9") && !m.Contains("created")), MessageType.BlueNormal), Times.Once);
        messages.Verify(v => v.ShowMessageAsync(It.Is<string>(m => m.Contains("created")), MessageType.BlueNormal), Times.Never);
    }

    private static async Task<(Player Player, ItemDefinition Definition)> CreateAsync(int maximumSockets)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var definition = player.PersistenceContext.CreateNew<ItemDefinition>();
        (definition.Group, definition.Number, definition.Width, definition.Height, definition.Durability, definition.MaximumSockets, definition.IsActive) = (0, 30, 1, 3, 50, maximumSockets, true);
        player.GameContext.Configuration.Items.Add(definition);
        return (player, definition);
    }

    private static IncreasableItemOption AddOption(Player player, ItemDefinition definition, ItemOptionType type, int number, int subOptionType)
    {
        if (definition.PossibleItemOptions.FirstOrDefault() is not { } options)
        {
            options = player.PersistenceContext.CreateNew<ItemOptionDefinition>();
            definition.PossibleItemOptions.Add(options);
        }

        var option = player.PersistenceContext.CreateNew<IncreasableItemOption>();
        (option.OptionType, option.Number, option.SubOptionType) = (type, number, subOptionType);
        options.PossibleOptions.Add(option);
        return option;
    }

    private static TemporaryItem Create(ItemDefinition definition) => GameMasterItemFactory.CreateItem(definition, new ItemSpecification(definition.Group, definition.Number));
}
