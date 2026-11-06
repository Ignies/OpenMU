// <copyright file="ItemExpiryExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemExpiry;

using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Extensions for items with an expiry (<see cref="Item.ExpiresAt"/>).
/// </summary>
public static class ItemExpiryExtensions
{
    /// <summary>
    /// Determines whether the item is expired at the given point in time.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="now">The current point in time (UTC).</param>
    /// <returns><c>true</c>, if the item is expired.</returns>
    public static bool IsExpired(this Item item, DateTime now) => item.ExpiresAt is { } expiresAt && expiresAt <= now;

    /// <summary>
    /// Gets the expiry as seconds since the Unix epoch, as the Babylon packets carry it.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The seconds since 1970-01-01 UTC, or 0 when the item doesn't expire.</returns>
    public static uint GetExpiryUnixSeconds(this Item item)
        => item.ExpiresAt is { } expiresAt ? (uint)Math.Clamp(new DateTimeOffset(DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc)).ToUnixTimeSeconds(), 1, uint.MaxValue) : 0;

    /// <summary>
    /// Removes the expired items of the inventory and the vault of the player and tells the player about each of them.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="now">The current point in time (UTC).</param>
    /// <returns>The number of removed items.</returns>
    public static async ValueTask<int> RemoveExpiredItemsAsync(this Player player, DateTime now)
    {
        var removed = 0;
        if (player.Inventory is { } inventory)
        {
            foreach (var item in inventory.Items.Where(i => i.IsExpired(now)).ToList())
            {
                var slot = item.ItemSlot;
                await inventory.RemoveItemAsync(item).ConfigureAwait(false);
                await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(slot)).ConfigureAwait(false);
                await player.ShowExpiredAsync(ExpiredItemStorage.Inventory, slot, item).ConfigureAwait(false);
                await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
                removed++;
            }
        }

        if (player.Account?.Vault is { } vault)
        {
            foreach (var item in vault.Items.Where(i => i.IsExpired(now)).ToList())
            {
                if (player.Vault is { } openedVault && openedVault.ItemStorage == vault)
                {
                    await openedVault.RemoveItemAsync(item).ConfigureAwait(false);
                }
                else
                {
                    vault.Items.Remove(item);
                }

                await player.ShowExpiredAsync(ExpiredItemStorage.Vault, item.ItemSlot, item).ConfigureAwait(false);
                await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
                removed++;
            }
        }

        return removed;
    }

    /// <summary>
    /// Tells the player that an item expired: through the <see cref="IItemExpiredPlugIn"/>, or with a message.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="storage">The storage from which the item was removed.</param>
    /// <param name="slot">The slot of the item.</param>
    /// <param name="item">The item.</param>
    /// <returns>The task.</returns>
    internal static async ValueTask ShowExpiredAsync(this Player player, ExpiredItemStorage storage, byte slot, Item item)
    {
        if (player.ViewPlugIns.GetPlugIn<IItemExpiredPlugIn>() is { } view)
        {
            await view.ItemExpiredAsync(storage, slot, item).ConfigureAwait(false);
            return;
        }

        await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.ItemExpired), item.Definition?.Name.ToString() ?? string.Empty).ConfigureAwait(false);
    }
}
