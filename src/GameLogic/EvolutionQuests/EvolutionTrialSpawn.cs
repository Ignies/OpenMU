// <copyright file="EvolutionTrialSpawn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EvolutionQuests;

/// <summary>
/// A monster of a trial step, spawned at its tile (or a walkable tile within the range of its definition).
/// </summary>
public class EvolutionTrialSpawn
{
    /// <summary>
    /// Gets or sets the number of the monster.
    /// </summary>
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate.
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate.
    /// </summary>
    public byte Y { get; set; }
}
