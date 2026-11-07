// <copyright file="AbilityCoreResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// The result of an <see cref="AbilityCoreAction"/>.
/// </summary>
public enum AbilityCoreResult : byte
{
    /// <summary>
    /// It was done.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The Ability Enhancement Core is off.
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// The character is not a fifth evolution.
    /// </summary>
    NotFifthEvolution = 2,

    /// <summary>
    /// There is no item at the slot.
    /// </summary>
    ItemNotFound = 3,

    /// <summary>
    /// The item is no ability card, or the card can't do this (an extra damage card has no level up or reduction).
    /// </summary>
    NotACard = 4,

    /// <summary>
    /// The slot is not open yet.
    /// </summary>
    SlotLocked = 5,

    /// <summary>
    /// The card doesn't fit the slot: another kind, another row color, or a card of another class.
    /// </summary>
    WrongSlot = 6,

    /// <summary>
    /// The slot holds a card already.
    /// </summary>
    SlotOccupied = 7,

    /// <summary>
    /// The skill card needs its fourth evolution skill of the skill enhancement tree, or the elite card the five
    /// option cards of its row.
    /// </summary>
    RequirementMissing = 8,

    /// <summary>
    /// The inventory has no room.
    /// </summary>
    InventoryFull = 9,

    /// <summary>
    /// The card is at its highest level.
    /// </summary>
    MaximumLevel = 10,

    /// <summary>
    /// There are not enough other cards of the same item.
    /// </summary>
    NotEnoughCards = 11,

    /// <summary>
    /// There is not enough zen.
    /// </summary>
    NotEnoughMoney = 12,

    /// <summary>
    /// There are not enough stones.
    /// </summary>
    NotEnoughStones = 13,

    /// <summary>
    /// The elite card's level is above the lowest level of the option cards of its row.
    /// </summary>
    EliteLevelTooHigh = 14,
}
