// <copyright file="MuunStorageKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// A storage of a Muun item move.
/// </summary>
public enum MuunStorageKind : byte
{
    /// <summary>
    /// The inventory of the character.
    /// </summary>
    Inventory = 0,

    /// <summary>
    /// The Muun inventory of the character (<see cref="Character.MuunInventory"/>).
    /// </summary>
    MuunInventory = 1,
}
