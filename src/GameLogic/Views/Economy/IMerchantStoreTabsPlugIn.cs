// <copyright file="IMerchantStoreTabsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Economy;

/// <summary>
/// Interface of a view whose implementation shows the tabs of the store of a merchant (Season 21).
/// A client without it only sees the first tab.
/// </summary>
public interface IMerchantStoreTabsPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the tabs of the store of the opened merchant. The item list of <paramref name="activeTab"/> follows.
    /// </summary>
    /// <param name="tabCount">The number of tabs.</param>
    /// <param name="activeTab">The tab whose items are shown.</param>
    /// <returns>The task.</returns>
    ValueTask ShowMerchantStoreTabsAsync(byte tabCount, byte activeTab);
}
