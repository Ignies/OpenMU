// <copyright file="NewPowerKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.NewPower;

/// <summary>
/// The kind of the fifth class New Power.
/// </summary>
public enum NewPowerKind
{
    /// <summary>
    /// Nuke: the greatest extra damage on one target.
    /// </summary>
    Nuke,

    /// <summary>
    /// Bolt: medium extra damage on up to two targets.
    /// </summary>
    Bolt,

    /// <summary>
    /// Wide (AoE): the lesser extra damage on up to ten targets.
    /// </summary>
    Wide,
}
