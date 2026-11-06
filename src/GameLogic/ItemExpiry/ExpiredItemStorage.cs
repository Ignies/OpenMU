// <copyright file="ExpiredItemStorage.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemExpiry;

/// <summary>
/// The storage from which an expired item was removed.
/// </summary>
public enum ExpiredItemStorage : byte
{
    /// <summary>
    /// The inventory of the character, including its equipped items.
    /// </summary>
    Inventory,

    /// <summary>
    /// The vault of the account.
    /// </summary>
    Vault,

    /// <summary>
    /// The gremory case of the character.
    /// </summary>
    CharacterGremoryCase,

    /// <summary>
    /// The gremory case of the account.
    /// </summary>
    AccountGremoryCase,
}
