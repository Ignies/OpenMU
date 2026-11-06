// <copyright file="EvolutionTrialEntrance.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EvolutionQuests;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// A town NPC which takes a character to the trial map of its next evolution.
/// </summary>
public class EvolutionTrialEntrance
{
    /// <summary>
    /// Gets or sets the number of the NPC.
    /// </summary>
    public short NpcNumber { get; set; }

    /// <summary>
    /// Gets or sets the evolution which the trial leads to; the character's next class has to be of it.
    /// </summary>
    public ClassEvolution Evolution { get; set; }

    /// <summary>
    /// Gets or sets the number of the trial map.
    /// </summary>
    public byte Map { get; set; }

    /// <summary>
    /// Gets or sets the left edge of the arrival area.
    /// </summary>
    public byte X1 { get; set; }

    /// <summary>
    /// Gets or sets the top edge of the arrival area.
    /// </summary>
    public byte Y1 { get; set; }

    /// <summary>
    /// Gets or sets the right edge of the arrival area.
    /// </summary>
    public byte X2 { get; set; }

    /// <summary>
    /// Gets or sets the bottom edge of the arrival area.
    /// </summary>
    public byte Y2 { get; set; }
}
