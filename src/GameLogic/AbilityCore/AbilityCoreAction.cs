// <copyright file="AbilityCoreAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// An action of the Ability Enhancement Core window.
/// </summary>
public enum AbilityCoreAction : byte
{
    /// <summary>
    /// Puts a card of the inventory into a slot.
    /// </summary>
    Insert = 0,

    /// <summary>
    /// Takes a card out of a slot into the inventory.
    /// </summary>
    Remove = 1,

    /// <summary>
    /// Raises the level of a card of a slot or of the inventory.
    /// </summary>
    LevelUp = 2,

    /// <summary>
    /// Reduces a card of the inventory into stones.
    /// </summary>
    Reduce = 3,
}
