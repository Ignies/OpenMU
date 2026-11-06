// <copyright file="IRuudViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Economy;

using MUnique.OpenMU.GameLogic.Ruud;

/// <summary>
/// Interface of a view whose implementation shows the Ruud of the character and the Ruud shop.
/// Without it, the player gets messages and can't open the shop.
/// </summary>
public interface IRuudViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the Ruud of the character.
    /// </summary>
    /// <param name="ruud">The Ruud of the character.</param>
    /// <param name="change">The change which led to it; 0 when it's just shown.</param>
    /// <returns>The task.</returns>
    ValueTask ShowRuudAsync(int ruud, int change);

    /// <summary>
    /// Opens the Ruud shop with the items which the character may buy.
    /// </summary>
    /// <param name="entries">The items with their index in the configuration.</param>
    /// <returns>The task.</returns>
    ValueTask ShowRuudShopAsync(IReadOnlyList<(int Index, RuudShopEntry Entry)> entries);

    /// <summary>
    /// Shows the result of a purchase.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="index">The index of the shop item.</param>
    /// <returns>The task.</returns>
    ValueTask ShowRuudShopBuyResultAsync(RuudShopBuyResult result, int index);
}
