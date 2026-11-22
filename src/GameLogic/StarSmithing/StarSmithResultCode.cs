// <copyright file="StarSmithResultCode.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The answer to a smithing action (the Result byte of EE 97).
/// </summary>
public enum StarSmithResultCode : byte
{
    /// <summary>Done.</summary>
    Done,

    /// <summary>No Star Smith's Hammer in a hand.</summary>
    NoHammer,

    /// <summary>The hammer has not enough durability left.</summary>
    HammerBroken,

    /// <summary>The item is locked.</summary>
    Locked,

    /// <summary>Not here: in a trade, a shop, a duel or on an event map.</summary>
    NotHere,

    /// <summary>No such item (or the hammer itself).</summary>
    NoItem,

    /// <summary>The text moderation refused the name or lore.</summary>
    TextRefused,

    /// <summary>Not enough Zen for a recolour.</summary>
    NotEnoughZen,

    /// <summary>Star Smithing is off on this world.</summary>
    Off,

    /// <summary>A recolour without the dye it needs in the slot it names.</summary>
    NoDye,

    /// <summary>The colour, strength, mode or glow is not one the dye allows.</summary>
    WrongDye,
}
