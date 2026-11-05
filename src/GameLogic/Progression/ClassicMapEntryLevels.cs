// <copyright file="ClassicMapEntryLevels.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Which entry levels the classic (Season 6) maps use in the normalized modes.
/// </summary>
/// <remarks>The values are persisted in the plugin configuration; append new members only.</remarks>
public enum ClassicMapEntryLevels
{
    /// <summary>
    /// The Season 6 entry levels, which match the Season 6 monsters of these maps.
    /// </summary>
    Season6,

    /// <summary>
    /// The Season 21 entry levels, unchanged.
    /// </summary>
    Season21,

    /// <summary>
    /// The Season 21 entry levels mapped through the normalization curve.
    /// </summary>
    Season21Normalized,
}
