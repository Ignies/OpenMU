// <copyright file="StarSmithingConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The settings of Star Smithing (Readme.md).
/// </summary>
public class StarSmithingConfiguration
{
    /// <summary>Gets or sets the item group of the Star Smith's Hammer.</summary>
    public byte HammerGroup { get; set; } = 2;

    /// <summary>Gets or sets the item number of the Star Smith's Hammer.</summary>
    public short HammerNumber { get; set; } = 1000;

    /// <summary>Gets or sets the durability a smithing action takes from the hammer (a lock takes none).</summary>
    public byte DurabilityPerAction { get; set; } = 5;

    /// <summary>Gets or sets the Zen a recolour costs.</summary>
    public int RecolorZen { get; set; } = 500_000;
}
