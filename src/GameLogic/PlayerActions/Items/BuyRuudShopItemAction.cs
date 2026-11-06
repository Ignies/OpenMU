// <copyright file="BuyRuudShopItemAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Items;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Buys an item of the Ruud shop, paid with the Ruud of the character.
/// </summary>
public class BuyRuudShopItemAction
{
    /// <summary>
    /// Buys the item of the Ruud shop with the given index.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="index">The index of the shop item in the configuration.</param>
    /// <returns>The result.</returns>
    public async ValueTask<RuudShopBuyResult> BuyAsync(Player player, int index)
    {
        var result = await this.TryBuyAsync(player, index).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IRuudViewPlugIn>(p => p.ShowRuudShopBuyResultAsync(result, index)).ConfigureAwait(false);
        return result;
    }

    private static Item CreateItem(Player player, RuudShopEntry entry, ItemDefinition definition)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = definition;
        item.Level = entry.Level;
        item.Durability = entry.Count > 0 ? entry.Count : definition.Durability;
        if (entry.LuckChance > 0 && Rand.NextRandomBool(entry.LuckChance) && FindOption(definition, ItemOptionTypes.Luck) is { } luck)
        {
            var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
            link.ItemOption = luck;
            item.ItemOptions.Add(link);
        }

        if (entry.OptionChance > 0 && entry.OptionLevel > 0 && Rand.NextRandomBool(entry.OptionChance) && FindOption(definition, ItemOptionTypes.Option) is { } option)
        {
            var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
            link.ItemOption = option;
            link.Level = Rand.NextInt(1, entry.OptionLevel + 1);
            item.ItemOptions.Add(link);
        }

        return item;
    }

    private static IncreasableItemOption? FindOption(ItemDefinition definition, ItemOptionType type)
        => definition.PossibleItemOptions.SelectMany(o => o.PossibleOptions).FirstOrDefault(o => o.OptionType == type);

    private async ValueTask<RuudShopBuyResult> TryBuyAsync(Player player, int index)
    {
        if (RuudFeaturePlugIn.Get(player.GameContext) is not { } plugIn
            || player.OpenedNpc?.Definition.Number != (plugIn.Configuration ?? new RuudConfiguration()).ShopNpcNumber)
        {
            return RuudShopBuyResult.ShopClosed;
        }

        if (plugIn.GetShopEntries(player).FirstOrDefault(e => e.Index == index).Entry is not { } entry
            || player.GameContext.Configuration.Items.FirstOrDefault(d => d.Group == entry.Group && d.Number == entry.Number) is not { IsActive: true } definition)
        {
            return RuudShopBuyResult.UnknownItem;
        }

        if (player.GetRuud() < entry.Price)
        {
            return RuudShopBuyResult.NotEnoughRuud;
        }

        if (player.Inventory is not { } inventory)
        {
            return RuudShopBuyResult.InventoryFull;
        }

        var item = CreateItem(player, entry, definition);
        if (inventory.CheckInvSpace(item) is not { } slot
            || !await inventory.AddItemAsync(slot, item).ConfigureAwait(false))
        {
            await player.PersistenceContext.DeleteAsync(item).ConfigureAwait(false);
            return RuudShopBuyResult.InventoryFull;
        }

        await player.TryTakeRuudAsync(entry.Price).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(item)).ConfigureAwait(false);
        return RuudShopBuyResult.Bought;
    }
}
