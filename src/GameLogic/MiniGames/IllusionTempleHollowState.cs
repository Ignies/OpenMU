// <copyright file="IllusionTempleHollowState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

/// <summary>
/// Where the Cursed Hollow of the renewed Illusion Temple is.
/// </summary>
public enum IllusionTempleHollowState
{
    /// <summary>
    /// In the standing Cursed Statue.
    /// </summary>
    Statue,

    /// <summary>
    /// Carried by a player.
    /// </summary>
    Carried,

    /// <summary>
    /// On the ground.
    /// </summary>
    OnGround,

    /// <summary>
    /// Nowhere: the statue comes back.
    /// </summary>
    None,
}
