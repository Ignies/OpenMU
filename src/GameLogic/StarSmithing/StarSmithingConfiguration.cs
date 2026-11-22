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

    /// <summary>Gets or sets the dyes a recolour spends (Readme.md "Dyes").</summary>
    public List<StarSmithDye> Dyes { get; set; } = DefaultDyes();

    /// <summary>
    /// The default dyes, group 14 from number 1000: eight basic ones (four shades of their colour, strength up to 85),
    /// five rare ones (three special colours, full strength, the keep mask), the Prismatic Dye (the hue shift) and the
    /// Radiant Dye (the glow).
    /// </summary>
    /// <returns>The dyes.</returns>
    public static List<StarSmithDye> DefaultDyes()
    {
        StarSmithDye Basic(short number, params int[] swatches) => new() { Number = number, Swatches = [.. swatches], MaximumStrength = 85 };
        StarSmithDye Rare(short number, params int[] swatches) => new() { Number = number, Swatches = [.. swatches], MaximumStrength = 100, Keep = true };
        return
        [
            Basic(1000, 0xff8a8a, 0xe03a3a, 0xa81c1c, 0x5c0c0c), // Crimson
            Basic(1001, 0xffc28a, 0xf08a2a, 0xb85a10, 0x6a3006), // Amber
            Basic(1002, 0xfff0a0, 0xf2c94c, 0xc0952a, 0x6e5310), // Gold
            Basic(1003, 0xa8f0a0, 0x4cc24c, 0x237a2a, 0x0f3d14), // Verdant
            Basic(1004, 0x7fc4ff, 0x1e90ff, 0x1060c0, 0x0a3470), // Azure
            Basic(1005, 0xd4a8ff, 0x9a4cf2, 0x6420b0, 0x34105e), // Violet
            Basic(1006, 0xffffff, 0xece6d6, 0xc8bfa8, 0x9a907a), // Ivory
            Basic(1007, 0x8a8a8a, 0x4a4a4a, 0x262626, 0x0c0c0c), // Ebony
            Rare(1008, 0xff2a2a, 0x8a0018, 0x2a0006), // Bloodmoon
            Rare(1009, 0x9ff6ff, 0xff7a2a, 0x3a1cff), // Frostfire
            Rare(1010, 0xfff6d0, 0xb8d8ff, 0xffd86a), // Starlight
            Rare(1011, 0x2a1a4a, 0x0a3a3a, 0x050510), // Abyssal
            Rare(1012, 0x6a1ab8, 0xd4a017, 0x0f2a6a), // Royal
            new() { Number = 1013, Swatches = [0xff3b3b, 0xffd23b, 0x3bff6e, 0x3bb8ff, 0xb23bff, 0xff3bd2], MaximumStrength = 100, Hue = true, Keep = true }, // Prismatic
            new() { Number = 1014, Radiant = true, MaximumStrength = 0 }, // Radiant
        ];
    }
}
