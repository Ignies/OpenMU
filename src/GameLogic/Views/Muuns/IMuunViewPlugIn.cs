// <copyright file="IMuunViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Muuns;

using MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// Interface of a view whose implementation shows the Muun inventory, the Muun actions and the Muuns of other players.
/// Without it (a Season 6 client), the player has no Muun system.
/// </summary>
public interface IMuunViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the Muun inventory.
    /// </summary>
    /// <param name="items">The items, in their slots.</param>
    ValueTask ShowMuunInventoryAsync(IReadOnlyCollection<Item> items);

    /// <summary>
    /// Shows the result of a Muun item move. On success, the item is shown at its new place.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="fromStorage">The storage of the source slot.</param>
    /// <param name="fromSlot">The source slot.</param>
    /// <param name="toStorage">The storage of the target slot.</param>
    /// <param name="toSlot">The target slot.</param>
    /// <param name="item">The moved item, if it was moved.</param>
    ValueTask MuunItemMovedAsync(MuunActionResult result, MuunStorageKind fromStorage, byte fromSlot, MuunStorageKind toStorage, byte toSlot, Item? item);

    /// <summary>
    /// Shows the result of a Muun action, with the item it changed or created.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="result">The result.</param>
    /// <param name="storage">The storage of the item.</param>
    /// <param name="slot">The slot of the item.</param>
    /// <param name="item">The changed or created item; <c>null</c> when there is none.</param>
    ValueTask MuunActionResultAsync(MuunAction action, MuunActionResult result, MuunStorageKind storage, byte slot, Item? item);

    /// <summary>
    /// Shows the Muun of a player (its main Muun, and whether it's ridden) to the player of this view.
    /// </summary>
    /// <param name="owner">The player who owns the Muun.</param>
    /// <param name="mainMuun">The Muun of the main slot; <c>null</c> for none.</param>
    /// <param name="isRiding">If set to <c>true</c>, the owner rides it.</param>
    ValueTask ShowMuunAppearanceAsync(Player owner, Item? mainMuun, bool isRiding);

    /// <summary>
    /// Shows the options of the equipped Muuns.
    /// </summary>
    /// <param name="options">The options.</param>
    ValueTask ShowMuunOptionsAsync(IReadOnlyList<MuunActiveOption> options);

    /// <summary>
    /// Shows the offers of the exchange NPC.
    /// </summary>
    /// <param name="offers">The offers, with their index.</param>
    ValueTask ShowMuunExchangeAsync(IReadOnlyList<MuunExchangeOffer> offers);
}
