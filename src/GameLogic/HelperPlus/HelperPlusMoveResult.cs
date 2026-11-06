// <copyright file="HelperPlusMoveResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HelperPlus;

/// <summary>
/// The result of a move of the helper plus, as the Babylon client numbers it.
/// </summary>
public enum HelperPlusMoveResult
{
    /// <summary>
    /// The character was moved.
    /// </summary>
    Moved = 0,

    /// <summary>
    /// The zone is unknown.
    /// </summary>
    UnknownZone = 1,

    /// <summary>
    /// The level is too low for the zone.
    /// </summary>
    LevelTooLow = 2,

    /// <summary>
    /// The character has not enough money.
    /// </summary>
    NotEnoughMoney = 3,

    /// <summary>
    /// The plus mode is not on.
    /// </summary>
    PlusModeOff = 4,

    /// <summary>
    /// The helper plus rules are off.
    /// </summary>
    Off = 5,

    /// <summary>
    /// The character can not move now.
    /// </summary>
    CanNotMove = 6,
}
