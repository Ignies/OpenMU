// <copyright file="MuunSlots.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The slots of the Muun inventory (<see cref="Character.MuunInventory"/>): two equipped Muuns and a bag of 100,
/// as the Muun window of the Season 21 client has them.
/// </summary>
public static class MuunSlots
{
    /// <summary>
    /// The slot of the main Muun: it follows the character, and a mount is ridden from it.
    /// </summary>
    public const byte Main = 0;

    /// <summary>
    /// The slot of the sub Muun.
    /// </summary>
    public const byte Sub = 1;

    /// <summary>
    /// The first slot of the bag.
    /// </summary>
    public const byte FirstBagSlot = 2;

    /// <summary>
    /// The number of slots.
    /// </summary>
    public const byte Count = 102;

    /// <summary>
    /// Determines whether a slot is an equipped slot.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns><c>true</c>, if the Muun of the slot is equipped.</returns>
    public static bool IsEquipped(byte slot) => slot is Main or Sub;
}
