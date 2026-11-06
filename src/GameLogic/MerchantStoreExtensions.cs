// <copyright file="MerchantStoreExtensions.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

using MUnique.OpenMU.DataModel.Configuration.Items;

/// <summary>
/// Extension methods for the <see cref="ItemStorage"/> of a merchant store.
/// </summary>
public static class MerchantStoreExtensions
{
    /// <summary>
    /// Gets the items of a merchant store which can be offered to the player.
    /// Items whose <see cref="ItemDefinition.IsActive"/> is <c>false</c> are left out.
    /// </summary>
    /// <param name="merchantStore">The merchant store.</param>
    /// <returns>The items which can be offered to the player.</returns>
    public static ICollection<Item> GetOfferedItems(this ItemStorage merchantStore)
    {
        if (merchantStore.Items.All(item => item.Definition?.IsActive is not false))
        {
            return merchantStore.Items;
        }

        return merchantStore.Items.Where(item => item.Definition?.IsActive is not false).ToList();
    }

    /// <summary>
    /// Gets the items of a tab of the store of a merchant. The tab 0 is the <see cref="MonsterDefinition.MerchantStore"/>,
    /// the further ones are its <see cref="MonsterDefinition.MerchantStoreTabs"/>.
    /// </summary>
    /// <param name="merchant">The definition of the merchant.</param>
    /// <param name="tab">The index of the tab.</param>
    /// <returns>The items of the tab, or <c>null</c> if the merchant has no such tab.</returns>
    public static ItemStorage? GetStoreTab(this MonsterDefinition merchant, byte tab)
        => tab == 0 ? merchant.MerchantStore : merchant.MerchantStoreTabs?.FirstOrDefault(t => t.Index == tab)?.Items;

    /// <summary>
    /// Gets the number of tabs of the store of a merchant: one more than the highest index of a tab with items, at least 1.
    /// </summary>
    /// <param name="merchant">The definition of the merchant.</param>
    /// <returns>The number of tabs.</returns>
    public static byte GetStoreTabCount(this MonsterDefinition merchant)
        => (byte)(merchant.MerchantStoreTabs?.Where(t => t.Items?.Items.Count > 0).Select(t => t.Index + 1).DefaultIfEmpty(1).Max() ?? 1);
}
