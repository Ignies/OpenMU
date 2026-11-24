// <copyright file="StarSmithForgeRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The Constellation Forge's rules (Readme.md "PR 7a contract"): the fault lines, the radii in the item's box, the
/// strikes and the Jewel of Guardian. Distances are in the unit square of the box.
/// </summary>
public class StarSmithForgeRules
{
    /// <summary>The most faults an attempt deals.</summary>
    public const int MostFaults = 8;

    /// <summary>Gets or sets how many fault lines an attempt deals (0 to 8).</summary>
    public int FaultCount { get; set; } = 3;

    /// <summary>Gets or sets the margin a fault's first end keeps from the box's edges.</summary>
    public double FaultMargin { get; set; } = 0.1;

    /// <summary>Gets or sets the shortest fault.</summary>
    public double FaultMinLength { get; set; } = 0.06;

    /// <summary>Gets or sets the longest fault.</summary>
    public double FaultMaxLength { get; set; } = 0.12;

    /// <summary>Gets or sets how near a fault a node cracks the attempt.</summary>
    public double FaultRadius { get; set; } = 0.025;

    /// <summary>Gets or sets how near the figure's first node a node closes the figure.</summary>
    public double CloseRadius { get; set; } = 0.04;

    /// <summary>Gets or sets the least distance between two nodes of an attempt.</summary>
    public double NodeGap { get; set; } = 0.03;

    /// <summary>Gets or sets how many placements an attempt allows on top of one per ingredient.</summary>
    public int BaseStrikes { get; set; } = 2;

    /// <summary>Gets or sets the most placements an attempt allows.</summary>
    public int MaxStrikes { get; set; } = 12;

    /// <summary>Gets or sets the item group of the Jewel of Guardian, which reveals a fault and protects the item.</summary>
    public byte GuardianGroup { get; set; } = 14;

    /// <summary>Gets or sets the item number of the Jewel of Guardian.</summary>
    public short GuardianNumber { get; set; } = 31;
}
