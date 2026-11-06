// <copyright file="GremoryCaseClaimResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GremoryCase;

/// <summary>
/// The result of a claim of an item of a gremory case. The values are sent to the client.
/// </summary>
public enum GremoryCaseClaimResult : byte
{
    /// <summary>
    /// The item was moved into the inventory.
    /// </summary>
    Claimed,

    /// <summary>
    /// The inventory has no room for the item.
    /// </summary>
    InventoryFull,

    /// <summary>
    /// The case has no item in the slot.
    /// </summary>
    NotFound,

    /// <summary>
    /// The item expired; it was removed.
    /// </summary>
    Expired,

    /// <summary>
    /// The gremory case is off on this server.
    /// </summary>
    Disabled,
}
