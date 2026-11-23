// <copyright file="StarSmithForgeRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The Constellation Forge's dealing rules (Readme.md).
/// </summary>
public class StarSmithForgeRules
{
    /// <summary>Gets or sets how many stars a sky holds.</summary>
    public int StarCount { get; set; } = 12;

    /// <summary>Gets or sets how many voids lie on stars of no shape, the risk of blind exploration.</summary>
    public int ExplorationVoids { get; set; } = 2;

    /// <summary>Gets or sets how many nearest stars each star may be linked to (shape edges come on top).</summary>
    public int Neighbours { get; set; } = 3;

    /// <summary>Gets or sets the least distance between two stars, in the item's box (0 to 1).</summary>
    public double MinimumSpacing { get; set; } = 0.14;

    /// <summary>Gets or sets the margin kept free at the box's edges (0 to 0.5).</summary>
    public double Margin { get; set; } = 0.08;

    /// <summary>Gets or sets how many stars may be struck on top of one per ingredient.</summary>
    public int BaseStrikes { get; set; } = 2;

    /// <summary>Gets or sets the item group of the Jewel of Guardian, which reveals a void and protects the item.</summary>
    public byte GuardianGroup { get; set; } = 14;

    /// <summary>Gets or sets the item number of the Jewel of Guardian.</summary>
    public short GuardianNumber { get; set; } = 31;
}
