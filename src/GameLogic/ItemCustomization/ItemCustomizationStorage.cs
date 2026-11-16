// <copyright file="ItemCustomizationStorage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// Where the items of an item customisation request live.
/// </summary>
public enum ItemCustomizationStorage : byte
{
    /// <summary>The items a player in view wears.</summary>
    Worn = 0,

    /// <summary>The own inventory, worn and in the bag.</summary>
    Inventory = 1,

    /// <summary>The own vault, while it's open.</summary>
    Vault = 2,

    /// <summary>The trade offer of the trading partner.</summary>
    Trade = 3,

    /// <summary>The open personal shop of a player in view.</summary>
    PersonalShop = 4,

    /// <summary>The own character's Gremory case.</summary>
    GremoryCase = 5,
}
