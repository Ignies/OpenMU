// <copyright file="MerchantStoreTabsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Economy;

using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.GameLogic.Views.NPC;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;

/// <summary>
/// Tests the tabs of a merchant store (Season 21, OpenMU issue 653): the tab 0 is the Season 6 store.
/// </summary>
[TestFixture]
public class MerchantStoreTabsTest
{
    private const byte Slot = 0;

    /// <summary>
    /// After selecting the tab 2, a purchase buys the item of that tab.
    /// </summary>
    [Test]
    public async ValueTask BuyFromTheTab2Async()
    {
        var (player, items) = await CreatePlayerAtMerchantAsync().ConfigureAwait(false);

        var selected = await new SelectMerchantStoreTabAction().SelectTabAsync(player, 2).ConfigureAwait(false);
        await new BuyNpcItemAction().BuyItemAsync(player, Slot).ConfigureAwait(false);

        Assert.That(selected, Is.True);
        Assert.That(player.MerchantTab, Is.EqualTo(2));
        Assert.That(player.Inventory!.Items.Single().Definition, Is.SameAs(items[2].Definition));
        Mock.Get(player.ViewPlugIns.GetPlugIn<IMerchantStoreTabsPlugIn>()!).Verify(v => v.ShowMerchantStoreTabsAsync(3, 2), Times.Once);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IShowMerchantStoreItemListPlugIn>()!)
            .Verify(v => v.ShowMerchantStoreItemListAsync(It.Is<ICollection<Item>>(l => l.Single() == items[2]), StoreKind.Normal), Times.Once);
    }

    /// <summary>
    /// Without a selected tab, a purchase buys the item of the first tab, as in Season 6.
    /// </summary>
    [Test]
    public async ValueTask WithoutATabTheFirstOneSellsAsync()
    {
        var (player, items) = await CreatePlayerAtMerchantAsync().ConfigureAwait(false);

        await new BuyNpcItemAction().BuyItemAsync(player, Slot).ConfigureAwait(false);

        Assert.That(player.Inventory!.Items.Single().Definition, Is.SameAs(items[0].Definition));
    }

    /// <summary>
    /// A tab which the merchant doesn't have is refused; the active tab stays.
    /// </summary>
    [Test]
    public async ValueTask UnknownTabIsRefusedAsync()
    {
        var (player, _) = await CreatePlayerAtMerchantAsync().ConfigureAwait(false);
        await new SelectMerchantStoreTabAction().SelectTabAsync(player, 1).ConfigureAwait(false);

        var selected = await new SelectMerchantStoreTabAction().SelectTabAsync(player, 4).ConfigureAwait(false);

        Assert.That(selected, Is.False);
        Assert.That(player.MerchantTab, Is.EqualTo(1));
    }

    /// <summary>
    /// The tab count is one more than the highest tab with items.
    /// </summary>
    [Test]
    public void TabCountCountsTheHighestTab()
    {
        var merchant = new BasicModel.MonsterDefinition();
        Assert.That(merchant.GetStoreTabCount(), Is.EqualTo(1));
        merchant.MerchantStoreTabs.Add(CreateTab(3, CreateStoreItem(1)));
        Assert.That(merchant.GetStoreTabCount(), Is.EqualTo(4));
        Assert.That(new MonsterDefinition().GetStoreTabCount(), Is.EqualTo(1), "A definition without tabs, as Season 6 has it.");
    }

    private static async ValueTask<(Player Player, List<Item> Items)> CreatePlayerAtMerchantAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Money = 1_000_000;
        var items = new List<Item> { CreateStoreItem(1), CreateStoreItem(2), CreateStoreItem(3) };
        var merchant = new BasicModel.MonsterDefinition { MerchantStore = CreateStorage(items[0]) };
        merchant.MerchantStoreTabs.Add(CreateTab(1, items[1]));
        merchant.MerchantStoreTabs.Add(CreateTab(2, items[2]));
        player.OpenedNpc = new NonPlayerCharacter(null!, merchant, null!);
        return (player, items);
    }

    private static MerchantStoreTab CreateTab(byte index, Item item) => new BasicModel.MerchantStoreTab { Index = index, Items = CreateStorage(item) };

    private static ItemStorage CreateStorage(Item item)
    {
        var storage = new BasicModel.ItemStorage();
        storage.Items.Add(item);
        return storage;
    }

    private static Item CreateStoreItem(short number)
    {
        var item = new BasicModel.Item
        {
            Definition = new ItemDefinition
            {
                Group = 14,
                Number = number,
                Value = 100,
                PriceDefinition = new BasicModel.ItemPriceDefinition { BasePriceFormula = "value" },
                Width = 1,
                Height = 1,
                Durability = 1,
            },
            Durability = 1,
            ItemSlot = Slot,
        };
        return item;
    }
}
