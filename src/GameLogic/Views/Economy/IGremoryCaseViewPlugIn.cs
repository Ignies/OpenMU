// <copyright file="IGremoryCaseViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Economy;

using MUnique.OpenMU.GameLogic.GremoryCase;

/// <summary>
/// Interface of a view whose implementation shows the gremory cases of the player.
/// A player without it doesn't get rewards into the case, but the way they took before.
/// </summary>
public interface IGremoryCaseViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the content of a case.
    /// </summary>
    /// <param name="kind">The kind of the case.</param>
    /// <param name="capacity">The number of rewards the case holds.</param>
    /// <param name="items">The items.</param>
    /// <returns>The task.</returns>
    ValueTask ShowGremoryCaseAsync(GremoryCaseKind kind, int capacity, IReadOnlyCollection<Item> items);

    /// <summary>
    /// Shows a reward which was added to a case.
    /// </summary>
    /// <param name="kind">The kind of the case.</param>
    /// <param name="item">The item.</param>
    /// <returns>The task.</returns>
    ValueTask GremoryCaseItemAddedAsync(GremoryCaseKind kind, Item item);

    /// <summary>
    /// Shows the result of a claim.
    /// </summary>
    /// <param name="kind">The kind of the case.</param>
    /// <param name="slot">The slot of the item in the case.</param>
    /// <param name="result">The result.</param>
    /// <returns>The task.</returns>
    ValueTask GremoryCaseClaimResultAsync(GremoryCaseKind kind, byte slot, GremoryCaseClaimResult result);

    /// <summary>
    /// Shows that a reward expired and was removed from a case.
    /// </summary>
    /// <param name="kind">The kind of the case.</param>
    /// <param name="slot">The slot of the item in the case.</param>
    /// <returns>The task.</returns>
    ValueTask GremoryCaseItemExpiredAsync(GremoryCaseKind kind, byte slot);
}
