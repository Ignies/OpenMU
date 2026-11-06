// <copyright file="SelectMerchantStoreTabAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Items;

using MUnique.OpenMU.GameLogic.Views.Economy;
using MUnique.OpenMU.GameLogic.Views.NPC;

/// <summary>
/// Shows another tab of the store of the opened merchant; purchases then refer to the items of that tab.
/// </summary>
public class SelectMerchantStoreTabAction
{
    /// <summary>
    /// Selects the tab of the store of the opened merchant.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="tab">The index of the tab.</param>
    /// <returns><c>true</c>, if the merchant has the tab.</returns>
    public async ValueTask<bool> SelectTabAsync(Player player, byte tab)
    {
        if (player.OpenedNpc?.Definition is not { } merchant
            || merchant.GetStoreTab(tab) is not { } store)
        {
            return false;
        }

        player.MerchantTab = tab;
        await player.InvokeViewPlugInAsync<IMerchantStoreTabsPlugIn>(p => p.ShowMerchantStoreTabsAsync(merchant.GetStoreTabCount(), tab)).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IShowMerchantStoreItemListPlugIn>(p => p.ShowMerchantStoreItemListAsync(store.GetOfferedItems(), StoreKind.Normal)).ConfigureAwait(false);
        return true;
    }
}
