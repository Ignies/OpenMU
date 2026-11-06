// <copyright file="MuunActionResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The result of a <see cref="MuunAction"/> or a Muun item move.
/// </summary>
public enum MuunActionResult : byte
{
    /// <summary>
    /// It was done.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The Muun system is off.
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// An item is missing.
    /// </summary>
    ItemNotFound = 2,

    /// <summary>
    /// The material doesn't fit the target.
    /// </summary>
    WrongMaterial = 3,

    /// <summary>
    /// The Muun is at its highest level already.
    /// </summary>
    MaximumLevel = 4,

    /// <summary>
    /// The Muun is not at its highest level yet.
    /// </summary>
    LevelTooLow = 5,

    /// <summary>
    /// The Muun has no evolution.
    /// </summary>
    CannotEvolve = 6,

    /// <summary>
    /// The player has not enough items for the exchange.
    /// </summary>
    NotEnoughItems = 7,

    /// <summary>
    /// The player doesn't talk to the exchange NPC.
    /// </summary>
    NpcNotOpened = 8,

    /// <summary>
    /// There is no free slot.
    /// </summary>
    StorageFull = 9,

    /// <summary>
    /// Riding is not allowed here.
    /// </summary>
    NotAllowedHere = 10,

    /// <summary>
    /// The Muun of the main slot can't be ridden.
    /// </summary>
    NotAMount = 11,

    /// <summary>
    /// The target slot holds an item.
    /// </summary>
    SlotOccupied = 12,

    /// <summary>
    /// Only a Muun can be equipped, and only Muun items go into the Muun inventory.
    /// </summary>
    NotAMuun = 13,

    /// <summary>
    /// The same Muun is equipped in another slot.
    /// </summary>
    SameMuunEquipped = 14,

    /// <summary>
    /// The slot isn't open yet (the hero slot, before the highest level) or takes no such Muun.
    /// </summary>
    SlotLocked = 15,
}
