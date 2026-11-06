// <copyright file="SoulRewardKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// The kinds of reward of a monster soul converter, as the Babylon client numbers them.
/// </summary>
public enum SoulRewardKind
{
    /// <summary>
    /// Ruud and Ruud items.
    /// </summary>
    Ruud = 0,

    /// <summary>
    /// Jewels.
    /// </summary>
    Jewels = 1,

    /// <summary>
    /// Elemental options: runes, pentagrams and errtels.
    /// </summary>
    Options = 2,

    /// <summary>
    /// Accessories, only at the fifth tier.
    /// </summary>
    Accessory = 3,
}
