// <copyright file="StarSmithDyes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The dye rules of a recolour (Readme.md "Dyes").
/// </summary>
public static class StarSmithDyes
{
    /// <summary>The most dyes a world keeps (EE 98 counts them in a byte; the site shows them on one page).</summary>
    public const int MaximumDyes = 64;

    private const int ModeAt = 0;
    private const int FlagsAt = 1;
    private const int ColourAt = 2;
    private const int StrengthAt = 5;
    private const int GlowAt = 11;
    private const byte HueMode = 2;
    private const byte KeepFlag = 1;
    private const byte GlowFlag = 2;

    /// <summary>
    /// Decides whether the dyes given allow a look: a colour dye for any new look, one of its swatches, at most its
    /// strength, the hue shift only with a Prismatic Dye, the keep mask only with a rare dye, and a glow only with a
    /// Radiant Dye and in a swatch of the colour dye.
    /// </summary>
    /// <param name="look">The look bytes of the recolour (<see cref="ItemCustomization.ItemLookBytes"/>).</param>
    /// <param name="colour">The colour dye in the dye slot, or null.</param>
    /// <param name="radiant">The Radiant Dye in its slot, or null.</param>
    /// <returns>Done, <see cref="StarSmithResultCode.NoDye"/> or <see cref="StarSmithResultCode.WrongDye"/>.</returns>
    public static StarSmithResultCode Check(ReadOnlySpan<byte> look, StarSmithDye? colour, StarSmithDye? radiant)
    {
        if (!NeedsDye(look))
        {
            return StarSmithResultCode.Done;
        }

        var glow = (look[FlagsAt] & GlowFlag) != 0;
        if (colour is null || colour.Radiant || (glow && radiant?.Radiant != true))
        {
            return StarSmithResultCode.NoDye;
        }

        var allowed = look[StrengthAt] <= colour.MaximumStrength
            && (look[ModeAt] == HueMode ? colour.Hue : colour.Swatches.Contains(Rgb(look[ColourAt..])))
            && ((look[FlagsAt] & KeepFlag) == 0 || colour.Keep)
            && (!glow || colour.Swatches.Contains(Rgb(look[GlowAt..])));
        return allowed ? StarSmithResultCode.Done : StarSmithResultCode.WrongDye;
    }

    /// <summary>
    /// Whether a look needs a dye: anything but the original colours (all zero).
    /// </summary>
    /// <param name="look">The look bytes.</param>
    /// <returns>True when a colour dye is spent.</returns>
    public static bool NeedsDye(ReadOnlySpan<byte> look) => look.Length > GlowAt + 2 && (look[ModeAt] != 0 || (look[FlagsAt] & GlowFlag) != 0);

    /// <summary>
    /// The dye an item is.
    /// </summary>
    /// <param name="dyes">The configured dyes.</param>
    /// <param name="group">The item's group.</param>
    /// <param name="number">The item's number.</param>
    /// <returns>The dye, or null.</returns>
    public static StarSmithDye? Find(IEnumerable<StarSmithDye> dyes, byte group, short number) =>
        dyes.FirstOrDefault(dye => dye.Group == group && dye.Number == number);

    /// <summary>
    /// Checks a dye table before it is saved: at most <see cref="MaximumDyes"/>, each an item that exists, none twice,
    /// a colour dye with 1 to <see cref="StarSmithDye.MaximumSwatches"/> colours, a strength cap up to 100, and the
    /// Radiant Dye with no colours, hue or keep mask.
    /// </summary>
    /// <param name="dyes">The dyes.</param>
    /// <param name="itemExists">Whether an item (group, number) is in the configuration.</param>
    /// <returns>Why the table is refused, or null.</returns>
    public static string? Validate(IReadOnlyList<StarSmithDye> dyes, Func<byte, short, bool> itemExists)
    {
        if (dyes.Count > MaximumDyes)
        {
            return $"There are more than {MaximumDyes} dyes.";
        }

        var seen = new HashSet<(byte, short)>();
        foreach (var dye in dyes)
        {
            var name = $"{dye.Group}/{dye.Number}";
            if (!seen.Add((dye.Group, dye.Number)))
            {
                return $"Item {name} is listed twice.";
            }

            if (!itemExists(dye.Group, dye.Number))
            {
                return $"Item {name} is not in the configuration.";
            }

            if (dye.MaximumStrength > 100 || dye.Swatches.Any(c => c is < 0 or > 0xFFFFFF))
            {
                return $"Dye {name} has a strength above 100 or a colour that is not 0xRRGGBB.";
            }

            if (dye.Radiant ? dye.Swatches.Count > 0 || dye.Hue || dye.Keep : dye.Swatches.Count is 0 or > StarSmithDye.MaximumSwatches)
            {
                return dye.Radiant
                    ? $"The Radiant Dye {name} holds no colours, hue or keep mask."
                    : $"Dye {name} needs 1 to {StarSmithDye.MaximumSwatches} colours.";
            }
        }

        return null;
    }

    private static int Rgb(ReadOnlySpan<byte> rgb) => (rgb[0] << 16) | (rgb[1] << 8) | rgb[2];
}
