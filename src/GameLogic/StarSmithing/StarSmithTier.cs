// <copyright file="StarSmithTier.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A rarity tier: its name and colour, and how many effects and boosts an item of it carries.
/// </summary>
public class StarSmithTier
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the colour, 0xRRGGBB.</summary>
    public int Color { get; set; }

    /// <summary>Gets or sets the effect slots, 0 to 2.</summary>
    public int EffectSlots { get; set; }

    /// <summary>Gets or sets the boost slots, 0 to 3.</summary>
    public int BoostSlots { get; set; }
}
