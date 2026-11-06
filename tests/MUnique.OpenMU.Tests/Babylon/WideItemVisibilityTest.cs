// <copyright file="WideItemVisibilityTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using System.IO;
using System.Reflection;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.NPC;
using MUnique.OpenMU.GameLogic.WideItems;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView;
using MUnique.OpenMU.GameServer.RemoteView.Inventory;
using MUnique.OpenMU.GameServer.RemoteView.NPC;
using MUnique.OpenMU.GameServer.RemoteView.Trade;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MUnique.OpenMU.Network.Packets.ServerToClient;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.Pathfinding;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests that a Season 6 client never receives an item which only the wide item format (W32) carries,
/// while the Babylon client gets every item (docs/babylon.md, "Wide items").
/// </summary>
[TestFixture]
public class WideItemVisibilityTest
{
    private static readonly ClientVersion Season6 = new(6, 3, ClientLanguage.English);

    /// <summary>
    /// The groups above 15, the wide format marker, the mastery and wing grade options and sphere levels above 5
    /// are what only the wide format carries.
    /// </summary>
    [Test]
    public void WideOnlyItemsAreTheOnesTheSeason6FormatCantCarry()
    {
        Assert.That(WideItemRules.IsWideOnly(CreateItem(0, 42)), Is.False, "a mastery item without its options");
        Assert.That(WideItemRules.IsWideOnly(CreateItem(15, 511)), Is.False);
        Assert.That(WideItemRules.IsWideOnly(CreateItem(16, 1)), Is.True, "group 16");
        Assert.That(WideItemRules.IsWideOnly(CreateItem(12, 512)), Is.True, "number above 511");
        Assert.That(WideItemRules.IsWideOnly(CreateMarkedItem()), Is.True, "marked definition");
        Assert.That(WideItemRules.IsWideOnly(CreateItemWithOption(Season21ItemOptionTypes.Mastery, 1)), Is.True);
        Assert.That(WideItemRules.IsWideOnly(CreateItemWithOption(Season21ItemOptionTypes.MasteryBonus, 1)), Is.True);
        Assert.That(WideItemRules.IsWideOnly(CreateItemWithOption(Season21ItemOptionTypes.WingGrade, 2)), Is.True);
        Assert.That(WideItemRules.IsWideOnly(CreateItemWithOption(ItemOptionTypes.SocketOption, 5)), Is.False, "sphere level 5");
        Assert.That(WideItemRules.IsWideOnly(CreateItemWithOption(ItemOptionTypes.SocketOption, 6)), Is.True, "sphere level 6");
    }

    /// <summary>
    /// The item serializer tells the game logic what the client's item format carries.
    /// </summary>
    [Test]
    public void TheItemSerializerTellsWhatTheClientCarries()
    {
        Assert.That(CreateViewer(Season6).Player.CarriesWideItems(), Is.False);
        Assert.That(CreateViewer(BabylonClient.Version).Player.CarriesWideItems(), Is.True);
    }

    /// <summary>
    /// A Season 6 client gets the inventory without the wide items; the Babylon client gets all of them.
    /// </summary>
    /// <param name="babylon">If set to <c>true</c>, the client is the Babylon client.</param>
    /// <param name="expectedCount">The expected item count.</param>
    [TestCase(false, 1)]
    [TestCase(true, 3)]
    public async ValueTask InventoryListAsync(bool babylon, int expectedCount)
    {
        var (player, output) = CreateViewer(babylon ? BabylonClient.Version : Season6);
        var character = new BasicModel.Character { Inventory = new BasicModel.ItemStorage() };
        character.Inventory.Items.Add(CreateItem(0, 1, slot: 12));
        character.Inventory.Items.Add(CreateItem(16, 1, slot: 13));
        character.Inventory.Items.Add(CreateItemWithOption(Season21ItemOptionTypes.Mastery, 1, slot: 14));
        typeof(Player).GetField("_selectedCharacter", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(player, character);

        await new UpdateInventoryListPlugIn(player).UpdateInventoryListAsync().ConfigureAwait(false);

        Assert.That(new CharacterInventory(output.ToArray()).ItemCount, Is.EqualTo(expectedCount));
    }

    /// <summary>
    /// A wide item which appears in the inventory isn't sent to a Season 6 client.
    /// </summary>
    [Test]
    public async ValueTask ItemAppearAsync()
    {
        var (season6, season6Output) = CreateViewer(Season6);
        var (babylon, babylonOutput) = CreateViewer(BabylonClient.Version);
        var item = CreateItem(16, 1, slot: 12);

        await new ItemAppearPlugIn(season6).ItemAppearAsync(item).ConfigureAwait(false);
        await new ItemAppearPlugIn(babylon).ItemAppearAsync(item).ConfigureAwait(false);

        Assert.That(season6Output.Length, Is.Zero);
        Assert.That(babylonOutput.Length, Is.Not.Zero);
    }

    /// <summary>
    /// A Season 6 client doesn't see wide items on the ground.
    /// </summary>
    [Test]
    public async ValueTask DroppedItemsAsync()
    {
        var map = new GameMap(new BasicModel.GameMapDefinition(), TimeSpan.FromMinutes(1), 8);
        await using var normal = new DroppedItem(CreateItem(0, 1), new Point(10, 10), map, null, null);
        await using var wide = new DroppedItem(CreateItem(20, 3), new Point(11, 10), map, null, null);
        var (season6, season6Output) = CreateViewer(Season6);
        var (babylon, babylonOutput) = CreateViewer(BabylonClient.Version);

        await new ShowDroppedItemsPlugIn(season6).ShowDroppedItemsAsync([normal, wide], true).ConfigureAwait(false);
        await new ShowDroppedItemsPlugIn(babylon).ShowDroppedItemsAsync([normal, wide], true).ConfigureAwait(false);
        Assert.That(new ItemsDropped(season6Output.ToArray()).ItemCount, Is.EqualTo(1));
        Assert.That(new ItemsDropped(babylonOutput.ToArray()).ItemCount, Is.EqualTo(2));

        season6Output.SetLength(0);
        await new ShowDroppedItemsPlugIn(season6).ShowDroppedItemsAsync([wide], true).ConfigureAwait(false);
        Assert.That(season6Output.Length, Is.Zero, "nothing to show, nothing sent");
    }

    /// <summary>
    /// A Season 6 trade partner doesn't see a wide item.
    /// </summary>
    [Test]
    public async ValueTask TradeItemAppearAsync()
    {
        var (season6, output) = CreateViewer(Season6);

        await new TradeItemAppearPlugIn(season6).TradeItemAppearAsync(0, CreateItem(16, 1)).ConfigureAwait(false);

        Assert.That(output.Length, Is.Zero);
    }

    /// <summary>
    /// A Season 6 client gets the merchant's (and the vault's) items without the wide ones.
    /// </summary>
    [Test]
    public async ValueTask MerchantStoreAndVaultAsync()
    {
        var (season6, output) = CreateViewer(Season6);
        var items = new List<Item> { CreateItem(0, 1, slot: 0), CreateItem(16, 1, slot: 2), CreateMarkedItem(slot: 4) };

        await new ShowMerchantStoreItemListPlugIn(season6).ShowMerchantStoreItemListAsync(items, StoreKind.Normal).ConfigureAwait(false);

        Assert.That(new StoreItemList(output.ToArray()).ItemCount, Is.EqualTo(1));
    }

    /// <summary>
    /// A wide item which another player equips reaches a Season 6 client as an empty slot.
    /// </summary>
    [Test]
    public void OtherPlayersWideGearIsAnEmptySlot()
    {
        var serializer = new AppearanceSerializer();
        var empty = new byte[serializer.NeededSpace];
        var withWideWeapon = new byte[serializer.NeededSpace];
        var weapon = new BasicModel.ItemDefinition { Group = 16, Number = 5, Width = 1, Height = 3 };

        serializer.WriteAppearanceData(empty, new TestAppearance(), false);
        serializer.WriteAppearanceData(withWideWeapon, new TestAppearance(new BasicModel.ItemAppearance { ItemSlot = InventoryConstants.LeftHandSlot, Definition = weapon }), false);

        Assert.That(withWideWeapon, Is.EqualTo(empty), "group 16 would wrap onto group 0");
    }

    private static (RemotePlayer Player, MemoryStream Output) CreateViewer(ClientVersion version)
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, ItemSerializer>();
        manager.RegisterPlugIn<IViewPlugIn, ItemSerializerW32>();
        manager.RegisterPlugIn<IViewPlugIn, AppearanceSerializer>();
        player.ClientVersion = version;
        return (player, output);
    }

    private static Item CreateItem(byte group, short number, byte slot = 12)
        => new BasicModel.Item
        {
            ItemSlot = slot,
            Durability = 10,
            Definition = new BasicModel.ItemDefinition { Group = group, Number = number, Width = 1, Height = 1 },
        };

    private static Item CreateMarkedItem(byte slot = 12)
    {
        var item = CreateItem(13, 300, slot);
        var marker = new BasicModel.ItemOptionDefinition();
        marker.PossibleOptions.Add(new BasicModel.IncreasableItemOption { OptionType = Season21ItemOptionTypes.WideFormat });
        item.Definition!.PossibleItemOptions.Add(marker);
        return item;
    }

    private static Item CreateItemWithOption(ItemOptionType type, int level, byte slot = 12)
    {
        var item = CreateItem(0, 42, slot);
        item.ItemOptions.Add(new BasicModel.ItemOptionLink { ItemOption = new BasicModel.IncreasableItemOption { OptionType = type, Number = 1 }, Level = level });
        if (type == ItemOptionTypes.SocketOption)
        {
            item.SocketCount = 1;
        }

        return item;
    }

    private sealed class TestAppearance : IAppearanceData
    {
        private readonly ItemAppearance[] _items;

        public TestAppearance(params ItemAppearance[] items) => this._items = items;

        public event EventHandler? AppearanceChanged
        {
            add { }
            remove { }
        }

        public CharacterClass? CharacterClass => null;

        public DataModel.Entities.CharacterStatus CharacterStatus => default;

        public CharacterPose Pose => default;

        public bool FullAncientSetEquipped => false;

        public IEnumerable<ItemAppearance> EquippedItems => this._items;
    }
}
