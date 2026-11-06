// <copyright file="GremoryCaseTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Economy;

using Moq;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.ItemExpiry;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Tests the gremory case: rewards land in it, are claimed into the inventory, and expire.
/// </summary>
[TestFixture]
public class GremoryCaseTest
{
    /// <summary>
    /// A reward lands in the case of the character, with the configured time to claim it, and the player sees it.
    /// </summary>
    [Test]
    public async ValueTask RewardLandsInTheCharacterCaseAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var item = CreateItem(player);

        var added = await GremoryCaseService.TryAddAsync(player, item, GremoryCaseKind.Character).ConfigureAwait(false);

        Assert.That(added, Is.SameAs(item));
        Assert.That(player.SelectedCharacter!.GremoryCase!.Items, Is.EquivalentTo(new[] { item }));
        Assert.That(player.Inventory!.Items, Is.Empty);
        Assert.That(item.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(7)).Within(TimeSpan.FromMinutes(1)));
        Mock.Get(player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>()!).Verify(v => v.GremoryCaseItemAddedAsync(GremoryCaseKind.Character, item), Times.Once);
    }

    /// <summary>
    /// A reward for the account lands in the case of the account.
    /// </summary>
    [Test]
    public async ValueTask AccountRewardLandsInTheAccountCaseAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var item = CreateItem(player);

        await GremoryCaseService.TryAddAsync(player, item, GremoryCaseKind.Account).ConfigureAwait(false);

        Assert.That(player.Account!.GremoryCase!.Items, Is.EquivalentTo(new[] { item }));
        Assert.That(player.SelectedCharacter!.GremoryCase, Is.Null);
    }

    /// <summary>
    /// A claimed reward moves into the inventory and doesn't expire anymore.
    /// </summary>
    [Test]
    public async ValueTask ClaimMovesTheRewardIntoTheInventoryAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var item = (await GremoryCaseService.TryAddAsync(player, CreateItem(player), GremoryCaseKind.Character).ConfigureAwait(false))!;

        var result = await GremoryCaseService.ClaimAsync(player, GremoryCaseKind.Character, item.ItemSlot).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(GremoryCaseClaimResult.Claimed));
        Assert.That(player.SelectedCharacter!.GremoryCase!.Items, Is.Empty);
        Assert.That(player.Inventory!.Items, Is.EquivalentTo(new[] { item }));
        Assert.That(item.ExpiresAt, Is.Null);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IItemAppearPlugIn>()!).Verify(v => v.ItemAppearAsync(item), Times.Once);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>()!).Verify(v => v.GremoryCaseClaimResultAsync(GremoryCaseKind.Character, 0, GremoryCaseClaimResult.Claimed), Times.Once);
    }

    /// <summary>
    /// A full case takes no further reward, so the caller can give it the old way.
    /// </summary>
    [Test]
    public async ValueTask FullCaseTakesNoRewardAsync()
    {
        var player = await CreatePlayerAsync(new GremoryCaseConfiguration { CharacterCapacity = 1 }).ConfigureAwait(false);
        await GremoryCaseService.TryAddAsync(player, CreateItem(player), GremoryCaseKind.Character).ConfigureAwait(false);

        var second = await GremoryCaseService.TryAddAsync(player, CreateItem(player), GremoryCaseKind.Character).ConfigureAwait(false);

        Assert.That(second, Is.Null);
        Assert.That(player.SelectedCharacter!.GremoryCase!.Items, Has.Count.EqualTo(1));
    }

    /// <summary>
    /// Without the plugin, rewards don't go into a case.
    /// </summary>
    [Test]
    public async ValueTask OffCaseTakesNoRewardAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);

        var added = await GremoryCaseService.TryAddAsync(player, CreateItem(player), GremoryCaseKind.Character).ConfigureAwait(false);

        Assert.That(added, Is.Null);
        Assert.That(GremoryCaseService.IsMiniGameRewardTarget(player), Is.False);
    }

    /// <summary>
    /// An expired reward is removed, and the player is told.
    /// </summary>
    [Test]
    public async ValueTask ExpiredRewardIsRemovedAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var item = (await GremoryCaseService.TryAddAsync(player, CreateItem(player), GremoryCaseKind.Account).ConfigureAwait(false))!;
        item.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        var removed = await GremoryCaseService.RemoveExpiredAsync(player, DateTime.UtcNow).ConfigureAwait(false);

        Assert.That(removed, Is.EqualTo(1));
        Assert.That(player.Account!.GremoryCase!.Items, Is.Empty);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>()!).Verify(v => v.GremoryCaseItemExpiredAsync(GremoryCaseKind.Account, item.ItemSlot), Times.Once);
        Mock.Get(player.ViewPlugIns.GetPlugIn<IItemExpiredPlugIn>()!).Verify(v => v.ItemExpiredAsync(ExpiredItemStorage.AccountGremoryCase, item.ItemSlot, item), Times.Once);
    }

    /// <summary>
    /// The claim of an expired reward fails and removes it.
    /// </summary>
    [Test]
    public async ValueTask ClaimOfAnExpiredRewardFailsAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var item = (await GremoryCaseService.TryAddAsync(player, CreateItem(player), GremoryCaseKind.Character).ConfigureAwait(false))!;
        item.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);

        var result = await GremoryCaseService.ClaimAsync(player, GremoryCaseKind.Character, item.ItemSlot).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(GremoryCaseClaimResult.Expired));
        Assert.That(player.Inventory!.Items, Is.Empty);
        Assert.That(player.SelectedCharacter!.GremoryCase!.Items, Is.Empty);
    }

    /// <summary>
    /// The claim fails when the inventory has no room; the reward stays.
    /// </summary>
    [Test]
    public async ValueTask ClaimWithAFullInventoryKeepsTheRewardAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        var big = CreateItem(player);
        big.Definition!.Width = 8;
        big.Definition.Height = 8;
        await player.Inventory!.AddItemAsync(InventoryConstants.EquippableSlotsCount, CreateItem(player)).ConfigureAwait(false);
        await GremoryCaseService.TryAddAsync(player, big, GremoryCaseKind.Character).ConfigureAwait(false);

        var result = await GremoryCaseService.ClaimAsync(player, GremoryCaseKind.Character, big.ItemSlot).ConfigureAwait(false);

        Assert.That(result, Is.EqualTo(GremoryCaseClaimResult.InventoryFull));
        Assert.That(player.SelectedCharacter!.GremoryCase!.Items, Is.EquivalentTo(new[] { big }));
    }

    /// <summary>
    /// The mini game rewards go into the case when the plugin says so.
    /// </summary>
    [Test]
    public async ValueTask MiniGameRewardsGoIntoTheCaseByDefaultAsync()
    {
        var player = await CreatePlayerAsync().ConfigureAwait(false);
        Assert.That(GremoryCaseService.IsMiniGameRewardTarget(player), Is.True);

        var other = await CreatePlayerAsync(new GremoryCaseConfiguration { MiniGameRewards = false }).ConfigureAwait(false);
        Assert.That(GremoryCaseService.IsMiniGameRewardTarget(other), Is.False);
    }

    private static async ValueTask<Player> CreatePlayerAsync(GremoryCaseConfiguration? configuration = null)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        Mock.Get(player.Account!).SetupProperty(a => a.GremoryCase);
        player.GameContext.FeaturePlugIns.AddPlugIn(new GremoryCaseFeaturePlugIn { Configuration = configuration ?? new GremoryCaseConfiguration() }, true);
        return player;
    }

    private static Item CreateItem(Player player)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = new ItemDefinition { Group = 14, Number = 13, Width = 1, Height = 1, Durability = 1 };
        item.Durability = 1;
        return item;
    }
}
