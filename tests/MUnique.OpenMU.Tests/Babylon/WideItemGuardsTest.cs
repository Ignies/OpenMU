// <copyright file="WideItemGuardsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.PlayerActions.Trade;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.NPC;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests that the game logic never hands a Season 6 player an item which only the wide item format carries:
/// not through a trade, a merchant or the ground. The players of these tests have mocked views, whose item format
/// carries no wide items unless the test says so.
/// </summary>
[TestFixture]
public class WideItemGuardsTest
{
    /// <summary>
    /// A wide item can't be put into a trade with a Season 6 partner; with a Babylon partner it can.
    /// </summary>
    /// <param name="partnerCarriesWideItems">If set to <c>true</c>, the partner's client carries wide items.</param>
    [TestCase(false)]
    [TestCase(true)]
    public async ValueTask TradeAsync(bool partnerCarriesWideItems)
    {
        var trader = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var partner = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        SetCarriesWideItems(trader, true);
        SetCarriesWideItems(partner, partnerCarriesWideItems);
        var item = CreateWideItem();
        await trader.Inventory!.AddItemAsync(20, item).ConfigureAwait(false);
        await new TradeRequestAction().RequestTradeAsync(trader, partner).ConfigureAwait(false);
        await new TradeAcceptAction().HandleTradeAcceptAsync(partner, true).ConfigureAwait(false);

        await new MoveItemAction().MoveItemAsync(trader, 20, Storages.Inventory, 0, Storages.Trade).ConfigureAwait(false);

        Assert.That(trader.TemporaryStorage!.Items.Contains(item), Is.EqualTo(partnerCarriesWideItems));
    }

    /// <summary>
    /// A Season 6 player can't buy a wide item of a merchant.
    /// </summary>
    [Test]
    public async ValueTask MerchantAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Money = 1_000_000;
        var storeItem = CreateWideItem();
        storeItem.ItemSlot = 5;
        var store = new BasicModel.ItemStorage();
        store.Items.Add(storeItem);
        player.OpenedNpc = new NonPlayerCharacter(null!, new MonsterDefinition { MerchantStore = store }, null!);

        await new BuyNpcItemAction().BuyItemAsync(player, 5).ConfigureAwait(false);

        Assert.That(player.Money, Is.EqualTo(1_000_000));
        Mock.Get(player.ViewPlugIns.GetPlugIn<IBuyNpcItemFailedPlugIn>()!).Verify(p => p.BuyNpcItemFailedAsync(), Times.Once);
    }

    /// <summary>
    /// A Season 6 player can't pick up a wide item, even with the id of its drop.
    /// </summary>
    [Test]
    public async ValueTask PickupAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var map = player.CurrentMap!;
        var dropped = new DroppedItem(CreateWideItem(), player.Position, map, null, null);
        await map.AddAsync(dropped).ConfigureAwait(false);

        await new PickupItemAction().PickupItemAsync(player, dropped.Id).ConfigureAwait(false);

        Assert.That(player.Inventory!.Items.Contains(dropped.Item), Is.False);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IItemPickUpFailedPlugIn>()!).Verify(p => p.ItemPickUpFailedAsync(ItemPickFailReason.General), Times.Once);
        await map.RemoveAsync(dropped).ConfigureAwait(false);
    }

    private static void SetCarriesWideItems(Player player, bool value)
        => Mock.Get(player.ViewPlugIns.GetPlugIn<IItemFormatViewPlugIn>()!).Setup(p => p.CarriesWideItems).Returns(value);

    private static Item CreateWideItem()
        => new BasicModel.Item
        {
            Durability = 1,
            Definition = new BasicModel.ItemDefinition { Group = 16, Number = 1, Width = 1, Height = 1, Value = 100, Durability = 1 },
        };
}
