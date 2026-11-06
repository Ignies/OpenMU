// <copyright file="EliteZoneElement.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

/// <summary>
/// The element of an elite zone, numbered like the spawn files of the Season 21 data.
/// </summary>
public enum EliteZoneElement
{
    /// <summary>
    /// No element.
    /// </summary>
    None = 0,

    /// <summary>
    /// The fire zone.
    /// </summary>
    Fire = 1,

    /// <summary>
    /// The water zone.
    /// </summary>
    Water = 2,

    /// <summary>
    /// The earth zone.
    /// </summary>
    Earth = 3,

    /// <summary>
    /// The wind zone.
    /// </summary>
    Wind = 4,

    /// <summary>
    /// The darkness zone.
    /// </summary>
    Darkness = 5,
}
