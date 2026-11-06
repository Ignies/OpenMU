// <copyright file="SoulMissionProgress.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// The progress of a soul mission of an active converter.
/// </summary>
public class SoulMissionProgress
{
    /// <summary>
    /// Gets or sets the map of the monster.
    /// </summary>
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the monster whose souls count.
    /// </summary>
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets the souls absorbed so far.
    /// </summary>
    public int Souls { get; set; }

    /// <summary>
    /// Gets or sets the souls the mission needs.
    /// </summary>
    public int RequiredSouls { get; set; }

    /// <summary>
    /// Gets or sets the chance of a kill to absorb a soul, in 1/1000000.
    /// </summary>
    public int AbsorbRate { get; set; }

    /// <summary>
    /// Gets a value indicating whether the mission is done.
    /// </summary>
    public bool IsDone => this.Souls >= this.RequiredSouls;
}
