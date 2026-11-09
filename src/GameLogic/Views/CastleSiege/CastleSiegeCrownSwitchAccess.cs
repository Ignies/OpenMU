// <copyright file="CastleSiegeCrownSwitchAccess.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.CastleSiege;

/// <summary>
/// The answer to a player who talks to or held a Castle Siege Crown switch.
/// </summary>
public enum CastleSiegeCrownSwitchAccess
{
    /// <summary>
    /// The player released the switch.
    /// </summary>
    Released = 0,

    /// <summary>
    /// The player pushes the switch.
    /// </summary>
    Pushing = 1,

    /// <summary>
    /// Another player pushes the switch.
    /// </summary>
    PushedByOther = 2,
}
