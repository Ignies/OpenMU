// <copyright file="RuudShopBuyResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Ruud;

/// <summary>
/// The result of a purchase in the Ruud shop. The values are sent to the client.
/// </summary>
public enum RuudShopBuyResult : byte
{
    /// <summary>
    /// The item was bought and put into the inventory.
    /// </summary>
    Bought,

    /// <summary>
    /// The character doesn't have enough Ruud.
    /// </summary>
    NotEnoughRuud,

    /// <summary>
    /// The inventory has no room for the item.
    /// </summary>
    InventoryFull,

    /// <summary>
    /// The shop doesn't sell such an item to the class of the character.
    /// </summary>
    UnknownItem,

    /// <summary>
    /// The shop isn't open: the character doesn't talk to its NPC, or Ruud is off.
    /// </summary>
    ShopClosed,
}
