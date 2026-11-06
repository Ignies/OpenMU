// <copyright file="ItemExpiryAndZenSinksTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Economy;

using Moq;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.ItemExpiry;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.ZenSinks;

/// <summary>
/// Tests the item expiry and the zen sinks of Season 21.
/// </summary>
[TestFixture]
public class ItemExpiryAndZenSinksTest
{
    /// <summary>
    /// An expired item of the inventory is removed and the player told; the others stay.
    /// </summary>
    [Test]
    public async ValueTask ExpiredInventoryItemIsRemovedAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var expired = CreateItem(player, DateTime.UtcNow.AddSeconds(-1));
        var timed = CreateItem(player, DateTime.UtcNow.AddDays(1));
        var permanent = CreateItem(player, null);
        await player.Inventory!.AddItemAsync(20, expired).ConfigureAwait(false);
        await player.Inventory.AddItemAsync(21, timed).ConfigureAwait(false);
        await player.Inventory.AddItemAsync(22, permanent).ConfigureAwait(false);

        var removed = await player.RemoveExpiredItemsAsync(DateTime.UtcNow).ConfigureAwait(false);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(player.Inventory.Items, Is.EquivalentTo(new[] { timed, permanent }));
        Mock.Get(player.ViewPlugIns.GetPlugIn<IItemRemovedPlugIn>()!).Verify(v => v.RemoveItemAsync(20), Times.Once);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IItemExpiredPlugIn>()!).Verify(v => v.ItemExpiredAsync(ExpiredItemStorage.Inventory, 20, expired), Times.Once);
    }

    /// <summary>
    /// The expiry is sent as seconds since 1970; 0 means never.
    /// </summary>
    [Test]
    public void ExpiryIsInUnixSeconds()
    {
        var item = new MUnique.OpenMU.Persistence.BasicModel.Item { ExpiresAt = new DateTime(2026, 11, 13, 0, 0, 0, DateTimeKind.Utc) };
        Assert.That(item.GetExpiryUnixSeconds(), Is.EqualTo(1_794_528_000u));
        Assert.That(new MUnique.OpenMU.Persistence.BasicModel.Item().GetExpiryUnixSeconds(), Is.EqualTo(0u));
    }

    /// <summary>
    /// With the zen sinks, the seller of a personal store pays the tax of Season 21 and monsters drop half the money.
    /// </summary>
    [Test]
    public async ValueTask ZenSinksTakeTheSeason21ShareAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        Assert.That(ZenSinksFeaturePlugIn.GetSellerIncome(player.GameContext, 1_000_000), Is.EqualTo(1_000_000), "Off: Season 6.");
        Assert.That(ZenSinksFeaturePlugIn.ScaleMoneyDrop(player.GameContext, 140_000), Is.EqualTo(140_000u), "Off: Season 6.");

        player.GameContext.FeaturePlugIns.AddPlugIn(new ZenSinksFeaturePlugIn(), true);

        Assert.That(ZenSinksFeaturePlugIn.GetSellerIncome(player.GameContext, 1_000_000), Is.EqualTo(990_000));
        Assert.That(ZenSinksFeaturePlugIn.GetSellerIncome(player.GameContext, 50), Is.EqualTo(50), "Less than one zen of tax.");
        Assert.That(ZenSinksFeaturePlugIn.ScaleMoneyDrop(player.GameContext, 140_000), Is.EqualTo(70_000u));
        Assert.That(ZenSinksFeaturePlugIn.ScaleMoneyDrop(player.GameContext, 1), Is.EqualTo(1u), "A pile has at least one zen.");
    }

    private static Item CreateItem(Player player, DateTime? expiresAt)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = new ItemDefinition { Group = 14, Number = 13, Width = 1, Height = 1, Durability = 1 };
        item.Durability = 1;
        item.ExpiresAt = expiresAt;
        return item;
    }
}
