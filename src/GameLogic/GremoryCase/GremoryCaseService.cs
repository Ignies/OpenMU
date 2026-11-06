// <copyright file="GremoryCaseService.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GremoryCase;

using MUnique.OpenMU.GameLogic.ItemExpiry;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Adds rewards to the gremory cases of a player, lets the player claim them and removes the expired ones.
/// </summary>
public static class GremoryCaseService
{
    /// <summary>
    /// Determines whether rewards go into the cases of the player: the case is on and the player's client shows it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if rewards go into the cases.</returns>
    public static bool IsAvailable(Player player)
        => GremoryCaseFeaturePlugIn.Get(player.GameContext) is not null && player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>() is not null;

    /// <summary>
    /// Determines whether the item rewards of the mini games go into the case of the player's character.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the rewards go into the case.</returns>
    public static bool IsMiniGameRewardTarget(Player player)
        => IsAvailable(player) && (GremoryCaseFeaturePlugIn.Get(player.GameContext)?.Configuration ?? new GremoryCaseConfiguration()).MiniGameRewards;

    /// <summary>
    /// Tries to add a reward item to a case of the player. A temporary item is made persistent.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="item">The item.</param>
    /// <param name="kind">The kind of the case.</param>
    /// <param name="duration">The time to claim it; <c>null</c> for the configured one.</param>
    /// <returns>The added item, or <c>null</c> if the case is off, not shown to the player or full.</returns>
    public static async ValueTask<Item?> TryAddAsync(Player player, Item item, GremoryCaseKind kind, TimeSpan? duration = null)
    {
        if (GremoryCaseFeaturePlugIn.Get(player.GameContext) is not { } plugIn
            || player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>() is not { } view
            || GetStorage(player, kind, true) is not { } storage)
        {
            return null;
        }

        var configuration = plugIn.Configuration ?? new GremoryCaseConfiguration();
        if (FindFreeSlot(storage, GetCapacity(configuration, kind)) is not { } slot)
        {
            return null;
        }

        var persistent = item is TemporaryItem temporary ? temporary.MakePersistent(player.PersistenceContext) : item;
        var expiresAt = DateTime.UtcNow + (duration ?? configuration.Duration);
        if (persistent.ExpiresAt is null || persistent.ExpiresAt > expiresAt)
        {
            persistent.ExpiresAt = expiresAt;
        }

        persistent.ItemSlot = slot;
        storage.Items.Add(persistent);
        await view.GremoryCaseItemAddedAsync(kind, persistent).ConfigureAwait(false);
        return persistent;
    }

    /// <summary>
    /// Moves an item of a case into the inventory of the player. A claimed item doesn't expire anymore.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="kind">The kind of the case.</param>
    /// <param name="slot">The slot of the item in the case.</param>
    /// <returns>The result.</returns>
    public static async ValueTask<GremoryCaseClaimResult> ClaimAsync(Player player, GremoryCaseKind kind, byte slot)
    {
        var result = await TryClaimAsync(player, kind, slot).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IGremoryCaseViewPlugIn>(p => p.GremoryCaseClaimResultAsync(kind, slot, result)).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Shows both cases to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    public static async ValueTask ShowAsync(Player player)
    {
        if (GremoryCaseFeaturePlugIn.Get(player.GameContext) is not { } plugIn
            || player.ViewPlugIns.GetPlugIn<IGremoryCaseViewPlugIn>() is not { } view)
        {
            return;
        }

        var configuration = plugIn.Configuration ?? new GremoryCaseConfiguration();
        var now = DateTime.UtcNow;
        foreach (var kind in Enum.GetValues<GremoryCaseKind>())
        {
            var items = GetStorage(player, kind, false)?.Items.Where(i => !i.IsExpired(now)).OrderBy(i => i.ItemSlot).ToList() ?? [];
            await view.ShowGremoryCaseAsync(kind, GetCapacity(configuration, kind), items).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Removes the expired items of both cases of the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="now">The current point in time (UTC).</param>
    /// <returns>The number of removed items.</returns>
    public static async ValueTask<int> RemoveExpiredAsync(Player player, DateTime now)
    {
        var removed = 0;
        foreach (var kind in Enum.GetValues<GremoryCaseKind>())
        {
            if (GetStorage(player, kind, false) is not { } storage)
            {
                continue;
            }

            foreach (var item in storage.Items.Where(i => i.IsExpired(now)).ToList())
            {
                await RemoveAsync(player, storage, kind, item).ConfigureAwait(false);
                removed++;
            }
        }

        return removed;
    }

    /// <summary>
    /// Gets the storage of a case of the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="kind">The kind of the case.</param>
    /// <param name="create">If set to <c>true</c>, a missing storage is created.</param>
    /// <returns>The storage, if it exists or was created.</returns>
    public static ItemStorage? GetStorage(Player player, GremoryCaseKind kind, bool create)
    {
        switch (kind)
        {
            case GremoryCaseKind.Character when player.SelectedCharacter is { } character:
                return character.GremoryCase ??= create ? player.PersistenceContext.CreateNew<ItemStorage>() : null;
            case GremoryCaseKind.Account when player.Account is { } account:
                return account.GremoryCase ??= create ? player.PersistenceContext.CreateNew<ItemStorage>() : null;
            default:
                return null;
        }
    }

    private static int GetCapacity(GremoryCaseConfiguration configuration, GremoryCaseKind kind)
        => Math.Clamp(kind == GremoryCaseKind.Account ? configuration.AccountCapacity : configuration.CharacterCapacity, 1, byte.MaxValue);

    private static byte? FindFreeSlot(ItemStorage storage, int capacity)
    {
        var used = storage.Items.Select(i => i.ItemSlot).ToHashSet();
        for (var slot = 0; slot < capacity; slot++)
        {
            if (!used.Contains((byte)slot))
            {
                return (byte)slot;
            }
        }

        return null;
    }

    private static async ValueTask<GremoryCaseClaimResult> TryClaimAsync(Player player, GremoryCaseKind kind, byte slot)
    {
        if (GremoryCaseFeaturePlugIn.Get(player.GameContext) is null)
        {
            return GremoryCaseClaimResult.Disabled;
        }

        if (GetStorage(player, kind, false) is not { } storage
            || storage.Items.FirstOrDefault(i => i.ItemSlot == slot) is not { } item
            || player.Inventory is not { } inventory)
        {
            return GremoryCaseClaimResult.NotFound;
        }

        if (item.IsExpired(DateTime.UtcNow))
        {
            await RemoveAsync(player, storage, kind, item).ConfigureAwait(false);
            return GremoryCaseClaimResult.Expired;
        }

        if (inventory.CheckInvSpace(item) is not { } inventorySlot)
        {
            return GremoryCaseClaimResult.InventoryFull;
        }

        storage.Items.Remove(item);
        var expiresAt = item.ExpiresAt;
        item.ExpiresAt = null;
        if (!await inventory.AddItemAsync(inventorySlot, item).ConfigureAwait(false))
        {
            item.ExpiresAt = expiresAt;
            item.ItemSlot = slot;
            storage.Items.Add(item);
            return GremoryCaseClaimResult.InventoryFull;
        }

        await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(item)).ConfigureAwait(false);
        return GremoryCaseClaimResult.Claimed;
    }

    private static async ValueTask RemoveAsync(Player player, ItemStorage storage, GremoryCaseKind kind, Item item)
    {
        storage.Items.Remove(item);
        await player.InvokeViewPlugInAsync<IGremoryCaseViewPlugIn>(p => p.GremoryCaseItemExpiredAsync(kind, item.ItemSlot)).ConfigureAwait(false);
        await player.ShowExpiredAsync(kind == GremoryCaseKind.Account ? ExpiredItemStorage.AccountGremoryCase : ExpiredItemStorage.CharacterGremoryCase, item.ItemSlot, item).ConfigureAwait(false);
        await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
    }
}
