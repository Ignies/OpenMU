// <copyright file="MuunAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// An action on the Muun items of a player.
/// </summary>
public enum MuunAction : byte
{
    /// <summary>
    /// Raises the level of a Muun by a Muun of the same kind of level 1.
    /// </summary>
    LevelUp = 1,

    /// <summary>
    /// Evolves a Muun of its highest level by its evolution stone.
    /// </summary>
    Evolve = 2,

    /// <summary>
    /// Charges a Muun energy converter with a Muun or an evolution stone; at full energy it becomes a wizardry stone.
    /// </summary>
    ChargeEnergy = 3,

    /// <summary>
    /// Restores the health of a Muun by a jewel of life.
    /// </summary>
    Restore = 4,

    /// <summary>
    /// Exchanges wizardry stones for an evolution stone at the exchange NPC.
    /// </summary>
    Exchange = 5,

    /// <summary>
    /// Mounts or dismounts the Muun of the main slot.
    /// </summary>
    Ride = 6,
}
