// <copyright file="AbilityCardColor.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// The color of an option or elite card, which is also the row of the option slots it belongs to.
/// </summary>
public enum AbilityCardColor : byte
{
    /// <summary>
    /// No color: skill and extra damage cards.
    /// </summary>
    None = 0,

    /// <summary>
    /// Blue, the first row.
    /// </summary>
    Blue = 1,

    /// <summary>
    /// Green, the second row.
    /// </summary>
    Green = 2,

    /// <summary>
    /// Purple, the third row.
    /// </summary>
    Purple = 3,
}
