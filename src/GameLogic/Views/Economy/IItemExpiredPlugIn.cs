// <copyright file="IItemExpiredPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Economy;

using MUnique.OpenMU.GameLogic.ItemExpiry;

/// <summary>
/// Interface of a view whose implementation tells the player that an item expired and was removed.
/// Without it, the player gets a message.
/// </summary>
public interface IItemExpiredPlugIn : IViewPlugIn
{
    /// <summary>
    /// Tells the player that an item expired and was removed.
    /// </summary>
    /// <param name="storage">The storage from which the item was removed.</param>
    /// <param name="slot">The slot of the item.</param>
    /// <param name="item">The item.</param>
    /// <returns>The task.</returns>
    ValueTask ItemExpiredAsync(ExpiredItemStorage storage, byte slot, Item item);
}
