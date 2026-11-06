// <copyright file="RewardDelivery.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Delivers a reward item: into the gremory case of the character, or into the inventory when the case isn't available.
/// </summary>
public static class RewardDelivery
{
    /// <summary>
    /// Creates the item of the reward and delivers it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="reward">The reward.</param>
    /// <param name="inventoryFirst">If <c>true</c>, the inventory is tried before the gremory case, e.g. for what a character gathers itself.</param>
    /// <returns><c>true</c>, if the item was delivered; <c>false</c> when the item is unknown or inactive, or nothing has room.</returns>
    public static async ValueTask<bool> DeliverAsync(Player player, RewardItem reward, bool inventoryFirst = false)
    {
        if (player.GameContext.Configuration.Items.FirstOrDefault(d => d.Group == reward.Group && d.Number == reward.Number) is not { IsActive: true } definition)
        {
            player.Logger.LogWarning("The reward item {group}/{number} is unknown or inactive.", reward.Group, reward.Number);
            return false;
        }

        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = definition;
        item.Level = reward.Level;
        item.Durability = reward.Count > 0 ? reward.Count : definition.Durability;

        if (inventoryFirst && await TryAddToInventoryAsync(player, item).ConfigureAwait(false))
        {
            return true;
        }

        if (await GremoryCaseService.TryAddAsync(player, item, GremoryCaseKind.Character).ConfigureAwait(false) is not null)
        {
            return true;
        }

        if (!inventoryFirst && await TryAddToInventoryAsync(player, item).ConfigureAwait(false))
        {
            return true;
        }

        await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
        await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.RewardLostByFullInventory), definition.Name.GetTranslation(player.Culture)).ConfigureAwait(false);
        return false;
    }

    private static async ValueTask<bool> TryAddToInventoryAsync(Player player, Item item)
    {
        if (player.Inventory is not { } inventory
            || inventory.CheckInvSpace(item) is not { } slot
            || !await inventory.AddItemAsync(slot, item).ConfigureAwait(false))
        {
            return false;
        }

        await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(item)).ConfigureAwait(false);
        return true;
    }
}
