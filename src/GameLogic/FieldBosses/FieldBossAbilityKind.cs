// <copyright file="FieldBossAbilityKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// What a special skill of a field boss does.
/// </summary>
public enum FieldBossAbilityKind
{
    /// <summary>
    /// Hits every player within the distance with a share of the boss's damage (whirlwind, flame, darkness).
    /// </summary>
    AreaStrike = 0,

    /// <summary>
    /// Calls monsters, up to a number which may live at once (the pets of the swamp monster).
    /// </summary>
    Summon = 1,
}
