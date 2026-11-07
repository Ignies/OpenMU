// <copyright file="ArcaExperienceReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The experience a participant gets for its points at the end of the war.
/// </summary>
public class ArcaExperienceReward
{
    /// <summary>
    /// Gets or sets the points from which the reward applies.
    /// </summary>
    public int MinimumPoints { get; set; }

    /// <summary>
    /// Gets or sets the experience.
    /// </summary>
    public long Experience { get; set; }
}
