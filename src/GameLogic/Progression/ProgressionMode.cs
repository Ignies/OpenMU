// <copyright file="ProgressionMode.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The progression mode of a world which ships the Season 21 maps.
/// </summary>
/// <remarks>The values are persisted in the plugin configuration; append new members only.</remarks>
public enum ProgressionMode
{
    /// <summary>
    /// The original Season 21 numbers: total levels up to 1750, the original monster rows.
    /// </summary>
    Original,

    /// <summary>
    /// Season 21 requirements mapped into level 400 plus master level, monsters on the Season 6 curve.
    /// </summary>
    Normalized,

    /// <summary>
    /// Like <see cref="Normalized"/>, but requirements above the first star band ask for stars (resets) instead of master levels.
    /// </summary>
    Stars,
}
