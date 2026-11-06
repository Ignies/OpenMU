// <copyright file="SeasonPassKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// The passes of the season pass, as the Babylon client numbers them.
/// </summary>
public enum SeasonPassKind
{
    /// <summary>
    /// The free MU Pass.
    /// </summary>
    Free = 0,

    /// <summary>
    /// The gold pass.
    /// </summary>
    Gold = 1,

    /// <summary>
    /// The premium pass.
    /// </summary>
    Premium = 2,
}
