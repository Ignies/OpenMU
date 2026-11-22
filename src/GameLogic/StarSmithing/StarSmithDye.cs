// <copyright file="StarSmithDye.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A dye a recolour spends (Readme.md "Dyes"): the item, the colours it holds and what else it allows.
/// </summary>
public class StarSmithDye
{
    /// <summary>The most swatches a dye holds (EE 98 has room for this many).</summary>
    public const int MaximumSwatches = 8;

    /// <summary>Gets or sets the item group of the dye.</summary>
    public byte Group { get; set; } = 14;

    /// <summary>Gets or sets the item number of the dye.</summary>
    public short Number { get; set; }

    /// <summary>Gets or sets the colours it holds, as 0xRRGGBB.</summary>
    public List<int> Swatches { get; set; } = [];

    /// <summary>Gets or sets the highest strength a recolour with it may have, 0 to 100.</summary>
    public byte MaximumStrength { get; set; } = 85;

    /// <summary>Gets or sets a value indicating whether it allows the hue shift (a Prismatic Dye).</summary>
    public bool Hue { get; set; }

    /// <summary>Gets or sets a value indicating whether it allows the keep mask (the rare dyes).</summary>
    public bool Keep { get; set; }

    /// <summary>Gets or sets a value indicating whether it is the Radiant Dye, spent beside a colour dye for the upgrade glow.</summary>
    public bool Radiant { get; set; }
}
