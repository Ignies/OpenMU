// <copyright file="FieldBossEntryItems.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Administration;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// The item which leads into the arena of a boss (the Spirit Nixie's Shard): handed out by an NPC, taken at the entry.
/// </summary>
internal static class FieldBossEntryItems
{
    /// <summary>
    /// Gets the definition of the entry item of a boss.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="boss">The boss.</param>
    /// <returns>The definition, or <c>null</c> when the boss needs none or the game doesn't define it.</returns>
    public static ItemDefinition? GetDefinition(IGameContext gameContext, FieldBossDefinition boss)
        => boss.EntryItemGroup < 0
            ? null
            : gameContext.Configuration.Items.FirstOrDefault(i => i.Group == boss.EntryItemGroup && i.Number == boss.EntryItemNumber);

    /// <summary>
    /// Determines whether a player carries the entry item.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="definition">The item definition.</param>
    /// <returns><c>true</c>, if the player has one.</returns>
    public static bool Has(Player player, ItemDefinition definition) => Find(player, definition) is not null;

    /// <summary>
    /// Takes one entry item from a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="definition">The item definition.</param>
    /// <returns><c>true</c>, if the player had one.</returns>
    public static async ValueTask<bool> TakeAsync(Player player, ItemDefinition definition)
    {
        if (Find(player, definition) is not { } item || player.Inventory is not { } inventory)
        {
            return false;
        }

        if (item.Durability > 1)
        {
            item.Durability--;
            await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(item, true)).ConfigureAwait(false);
            return true;
        }

        var slot = item.ItemSlot;
        await inventory.RemoveItemAsync(item).ConfigureAwait(false);
        await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemRemovedPlugIn>(p => p.RemoveItemAsync(slot)).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gives a player one entry item for its price.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="definition">The item definition.</param>
    /// <param name="price">The price in zen.</param>
    /// <returns><c>true</c>, if the player got it; <c>false</c> without the zen or the room.</returns>
    public static async ValueTask<bool> GiveAsync(Player player, ItemDefinition definition, int price)
    {
        var temporary = GameMasterItemFactory.CreateItem(definition, new ItemSpecification(definition.Group, definition.Number));
        temporary.Durability = 1;
        if (player.Inventory is not { } inventory
            || inventory.CheckInvSpace(temporary) is not { } slot
            || player.Money < price)
        {
            return false;
        }

        if (!await inventory.AddItemAsync(slot, temporary).ConfigureAwait(false) || inventory.GetItem(slot) is not { } item)
        {
            return false;
        }

        player.TryRemoveMoney(price);
        await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(item)).ConfigureAwait(false);
        return true;
    }

    private static Item? Find(Player player, ItemDefinition definition)
        => player.Inventory?.Items.FirstOrDefault(i => i.Definition == definition && i.ItemSlot >= InventoryConstants.EquippableSlotsCount);
}
