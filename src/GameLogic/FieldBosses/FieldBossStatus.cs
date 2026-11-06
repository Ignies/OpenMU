// <copyright file="FieldBossStatus.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// The status of a field boss.
/// </summary>
public enum FieldBossStatus
{
    /// <summary>
    /// Waiting for its spawn.
    /// </summary>
    Waiting = 0,

    /// <summary>
    /// Alive, nobody hit it yet.
    /// </summary>
    Alive = 1,

    /// <summary>
    /// Alive and in a fight.
    /// </summary>
    Engaged = 2,

    /// <summary>
    /// Killed; the players leave the arena.
    /// </summary>
    Defeated = 3,
}
