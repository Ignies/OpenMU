// <copyright file="ProgressionLevelKey.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The level a requirement (event bracket, item level) is checked against.
/// </summary>
/// <remarks>The values are persisted in the plugin configuration; append new members only.</remarks>
public enum ProgressionLevelKey
{
    /// <summary>
    /// The normal level.
    /// </summary>
    Level,

    /// <summary>
    /// The normal level plus the master level, capped at the level cap.
    /// </summary>
    LevelAndMasterLevel,

    /// <summary>
    /// The power level: the normal level plus the worth of the stars, capped at the level cap.
    /// </summary>
    PowerLevel,

    /// <summary>
    /// The Season 21 total level (normal plus master level), uncapped, on the Season 21 grades.
    /// </summary>
    Season21Total,
}
