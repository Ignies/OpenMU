// <copyright file="ItemCustomizationCollector.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.Persistence;

/// <summary>
/// The items a request asks about: the persistent item id of each slot of each asked place the player may see.
/// </summary>
public static class ItemCustomizationCollector
{
    /// <summary>
    /// Collects the items of the asked places. A place the player may not see (a player out of view, another player's
    /// vault, a closed shop, a trade with someone else) gives nothing.
    /// </summary>
    /// <param name="player">The asking player.</param>
    /// <param name="asks">The asked places, with the object id as the request carried it.</param>
    /// <returns>The items, with the place they were asked as.</returns>
    public static async ValueTask<List<(ItemCustomizationStorage Storage, ushort ObjectId, byte Slot, Guid ItemId)>> CollectAsync(
        Player player,
        IEnumerable<(ItemCustomizationStorage Storage, ushort ObjectId)> asks)
    {
        var result = new List<(ItemCustomizationStorage, ushort, byte, Guid)>();
        foreach (var (storage, objectId) in asks.Distinct())
        {
            foreach (var item in await ItemsOfAsync(player, storage, objectId).ConfigureAwait(false))
            {
                if (item is IIdentifiable { Id: var id } && id != Guid.Empty)
                {
                    result.Add((storage, objectId, item.ItemSlot, id));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the item of one slot of a place the player may see.
    /// </summary>
    /// <param name="player">The asking player.</param>
    /// <param name="storage">The storage.</param>
    /// <param name="objectId">The object id.</param>
    /// <param name="slot">The slot.</param>
    /// <returns>The persistent item id, or null.</returns>
    public static async ValueTask<Guid?> ItemIdAsync(Player player, ItemCustomizationStorage storage, ushort objectId, byte slot)
    {
        var items = await ItemsOfAsync(player, storage, objectId).ConfigureAwait(false);
        return items.FirstOrDefault(item => item.ItemSlot == slot) is IIdentifiable { Id: var id } && id != Guid.Empty ? id : null;
    }

    /// <summary>
    /// Gets the Gremory case a request names by its object id.
    /// </summary>
    /// <param name="objectId">The object id.</param>
    /// <returns>The case, or null.</returns>
    public static GremoryCaseKind? GremoryCaseOf(ushort objectId) =>
        Enum.IsDefined((GremoryCaseKind)objectId) && objectId <= byte.MaxValue ? (GremoryCaseKind)objectId : null;

    private static async ValueTask<IEnumerable<Item>> ItemsOfAsync(Player player, ItemCustomizationStorage storage, ushort objectId)
    {
        var target = PlayerOf(player, (ushort)(objectId & 0x7FFF));
        switch (storage)
        {
            case ItemCustomizationStorage.Worn when target is not null && (target == player || await InViewAsync(player, target).ConfigureAwait(false)):
                return target.Inventory?.Items.Where(item => item.ItemSlot < InventoryConstants.EquippableSlotsCount) ?? [];
            case ItemCustomizationStorage.Inventory:
                return player.Inventory?.Items ?? [];
            case ItemCustomizationStorage.Vault:
                return player.Vault?.Items ?? [];
            case ItemCustomizationStorage.Trade when target is not null && player.TradingPartner == target:
                return target.TemporaryStorage?.Items ?? [];
            case ItemCustomizationStorage.PersonalShop when target is { ShopStorage.StoreOpen: true } && await InViewAsync(player, target).ConfigureAwait(false):
                return target.ShopStorage.Items;
            case ItemCustomizationStorage.GremoryCase:
                return GremoryCaseOf(objectId) switch
                {
                    GremoryCaseKind.Character => player.SelectedCharacter?.GremoryCase?.Items ?? [],
                    GremoryCaseKind.Account => player.Account?.GremoryCase?.Items ?? [],
                    _ => [],
                };
            default:
                return [];
        }
    }

    /// <summary>The player an id names: the own one as the client knows it (<see cref="ViewExtensions.ConstantPlayerId"/>) or by its map id.</summary>
    private static Player? PlayerOf(Player player, ushort id) =>
        id == ViewExtensions.ConstantPlayerId || id == player.Id ? player : player.CurrentMap?.GetObject(id) as Player;

    private static async ValueTask<bool> InViewAsync(Player player, Player other)
    {
        using (await other.ObserverLock.ReaderLockAsync())
        {
            return other.Observers.Contains(player);
        }
    }
}
