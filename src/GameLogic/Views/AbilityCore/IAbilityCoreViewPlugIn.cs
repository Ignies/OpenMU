// <copyright file="IAbilityCoreViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.AbilityCore;

using MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// Interface of a view whose implementation shows the Ability Enhancement Core. Without it (a Season 6 client), the
/// player has no window of the core.
/// </summary>
public interface IAbilityCoreViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the core: which slots are open, which cards take effect, and the cards.
    /// </summary>
    /// <param name="openSlots">The open slots, bit n for the slot n.</param>
    /// <param name="activeSlots">The slots whose card takes effect, bit n for the slot n.</param>
    /// <param name="cards">The cards, in their slots.</param>
    /// <returns>The task.</returns>
    ValueTask ShowAbilityCoreAsync(uint openSlots, uint activeSlots, IReadOnlyCollection<Item> cards);

    /// <summary>
    /// Shows the result of an action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="result">The result.</param>
    /// <param name="coreSlot">The slot of the core.</param>
    /// <param name="inventorySlot">The slot of the inventory.</param>
    /// <param name="item">The card after the action, if there is one.</param>
    /// <returns>The task.</returns>
    ValueTask AbilityCoreResultAsync(AbilityCoreAction action, AbilityCoreResult result, byte coreSlot, byte inventorySlot, Item? item);
}
