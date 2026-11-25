// <copyright file="StarSmithLayersTravelTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Trade;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.GameLogic.Views.Trade;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Everything travels with the item (Hidden-Babylon docs/star_smithing.md section 2, PR 7a): an item's constellation
/// layers are on its customisation record, keyed by the item's id, so a trade, a vault, a drop and a pickup by
/// someone else keep them exactly, and the new holder is told them.
/// </summary>
[TestFixture]
public class StarSmithLayersTravelTest
{
    private const string Layers = "101:5:1:1000,1000,9000,1000,5000,8000;113:3:13:20000,20000,30000,20000,30000,30000,20000,30000";

    /// <summary>
    /// A trade moves the item itself: the partner holds the same item, its record and layers are the same, and the
    /// partner's channel finds it in the own inventory.
    /// </summary>
    [Test]
    public async ValueTask ATradeKeepsTheLayersAsync()
    {
        var (trader1, trader2) = await TwoPlayersAsync().ConfigureAwait(false);
        var item = await SmithedItemAsync(trader1, 20).ConfigureAwait(false);

        await new TradeRequestAction().RequestTradeAsync(trader1, trader2).ConfigureAwait(false);
        await new TradeAcceptAction().HandleTradeAcceptAsync(trader2, true).ConfigureAwait(false);
        await new MoveItemAction().MoveItemAsync(trader1, 20, Storages.Inventory, 0, Storages.Trade).ConfigureAwait(false);
        await new TradeButtonAction().TradeButtonChangedAsync(trader1, TradeButtonState.Checked).ConfigureAwait(false);
        await new TradeButtonAction().TradeButtonChangedAsync(trader2, TradeButtonState.Checked).ConfigureAwait(false);

        var received = trader2.Inventory!.ItemStorage.Items.Single();
        Assert.That(received, Is.SameAs(item));
        await AssertLayersAsync(trader2, item, ItemCustomizationStorage.Inventory, 0, received.ItemSlot).ConfigureAwait(false);
    }

    /// <summary>
    /// The vault keeps the layers; the channel finds the item there.
    /// </summary>
    [Test]
    public async ValueTask TheVaultKeepsTheLayersAsync()
    {
        var (player, _) = await TwoPlayersAsync().ConfigureAwait(false);
        var item = await SmithedItemAsync(player, 20).ConfigureAwait(false);
        player.Vault = new Storage(InventoryConstants.WarehouseSize, player.PersistenceContext.CreateNew<ItemStorage>());
        player.OpenedNpc = new NonPlayerCharacter(null!, new MonsterDefinition { NpcWindow = NpcWindow.VaultStorage }, null!);
        Assert.That(await player.PlayerState.TryAdvanceToAsync(PlayerState.NpcDialogOpened).ConfigureAwait(false), Is.True);

        await new MoveItemAction().MoveItemAsync(player, 20, Storages.Inventory, 5, Storages.Vault).ConfigureAwait(false);

        Assert.That(player.Vault.GetItem(5), Is.SameAs(item));
        await AssertLayersAsync(player, item, ItemCustomizationStorage.Vault, 0, 5).ConfigureAwait(false);
    }

    /// <summary>
    /// An item on the ground keeps its layers for everyone in view, and whoever picks it up holds the same item and
    /// record.
    /// </summary>
    [Test]
    public async ValueTask ADropAndPickupKeepTheLayersAsync()
    {
        var (owner, finder) = await TwoPlayersAsync().ConfigureAwait(false);
        var item = await SmithedItemAsync(owner, 20).ConfigureAwait(false);
        await owner.Inventory!.RemoveItemAsync(item).ConfigureAwait(false);
        owner.PersistenceContext.Detach(item);
        var drop = new DroppedItem(item, finder.Position, finder.CurrentMap!, owner, null);
        await finder.CurrentMap!.AddAsync(drop).ConfigureAwait(false);

        await AssertLayersAsync(finder, item, ItemCustomizationStorage.GroundItem, drop.Id, 0).ConfigureAwait(false);
        await new PickupItemAction().PickupItemAsync(finder, drop.Id).ConfigureAwait(false);

        var picked = finder.Inventory!.ItemStorage.Items.Single();
        Assert.That(picked, Is.SameAs(item));
        await AssertLayersAsync(finder, item, ItemCustomizationStorage.Inventory, 0, picked.ItemSlot).ConfigureAwait(false);
    }

    private static async ValueTask AssertLayersAsync(Player viewer, Item item, ItemCustomizationStorage storage, ushort objectId, byte slot)
    {
        var id = ((MUnique.OpenMU.Persistence.IIdentifiable)item).Id;
        var found = await ItemCustomizationCollector.CollectAsync(viewer, [(storage, objectId)]).ConfigureAwait(false);
        Assert.That(found.Select(f => (f.Slot, f.ItemId)), Does.Contain((slot, id)), "the channel finds the item where it is now");
        var record = (await viewer.PersistenceContext.GetItemCustomizationsAsync([id]).ConfigureAwait(false)).Single();
        Assert.Multiple(() =>
        {
            Assert.That(record.Layers, Is.EqualTo(Layers));
            Assert.That(record.Rarity, Is.EqualTo(5));
            Assert.That(StarSmithLayers.Parse(record.Layers).Select(l => l.Number), Is.EqualTo(new short[] { 101, 113 }));
        });
    }

    private static async ValueTask<Item> SmithedItemAsync(Player player, byte slot)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = 0, Number = 1, Width = 1, Height = 1, Durability = 1 };
        item.Durability = 1;
        await player.Inventory!.AddItemAsync(slot, item).ConfigureAwait(false);
        var record = player.PersistenceContext.CreateNew<DataModel.Entities.ItemCustomization>();
        record.ItemId = ((MUnique.OpenMU.Persistence.IIdentifiable)item).Id;
        record.Layers = Layers;
        record.Rarity = 5;
        await player.PersistenceContext.SaveChangesAsync().ConfigureAwait(false);
        return item;
    }

    private static async ValueTask<(Player One, Player Two)> TwoPlayersAsync()
    {
        var configuration = PlayerTestHelper.CreateGameConfiguration();
        configuration.ItemDropDuration = TimeSpan.FromMinutes(1);
        configuration.InfoRange = 15;
        var mapInitializer = new MapInitializer(configuration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var gameContext = new GameContext(configuration, new InMemoryPersistenceContextProvider(), mapInitializer, new NullLoggerFactory(), new PlugInManager(null, new NullLoggerFactory(), null, null), NullDropGenerator.Instance, new ConfigurationChangeMediator());
        mapInitializer.PlugInManager = gameContext.PlugInManager;
        mapInitializer.PathFinderPool = gameContext.PathFinderPool;
        var one = await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
        var two = await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
        return (one, two);
    }
}
