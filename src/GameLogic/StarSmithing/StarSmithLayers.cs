// <copyright file="StarSmithLayers.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Globalization;

/// <summary>
/// A sealed constellation on an item: its number, tier, look and hull.
/// </summary>
/// <param name="Number">The constellation (recipe) number.</param>
/// <param name="Tier">Its tier.</param>
/// <param name="Look">Its look, kept on the item.</param>
/// <param name="Hull">The part it covers, 3 to 8 points in box units.</param>
public sealed record StarSmithLayer(short Number, StarSmithSignTier Tier, byte Look, IReadOnlyList<StarSmithPoint> Hull);

/// <summary>
/// The text of <c>ItemCustomization.Layers</c> (Readme.md "PR 7a contract"): up to 4 entries joined by ';', each
/// "number:tier:look:x1,y1,x2,y2,...".
/// </summary>
public static class StarSmithLayers
{
    /// <summary>
    /// Reads the layers; a broken entry is skipped, and at most 4 are read.
    /// </summary>
    /// <param name="text">The stored text.</param>
    /// <returns>The layers, in the order sealed.</returns>
    public static IReadOnlyList<StarSmithLayer> Parse(string? text)
    {
        var layers = new List<StarSmithLayer>();
        foreach (var entry in (text ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Read(entry) is { } layer)
            {
                layers.Add(layer);
            }

            if (layers.Count == StarSmithSlots.Maximum)
            {
                break;
            }
        }

        return layers;
    }

    /// <summary>
    /// Writes the layers; null for none.
    /// </summary>
    /// <param name="layers">The layers.</param>
    /// <returns>The text.</returns>
    public static string? Format(IEnumerable<StarSmithLayer> layers)
    {
        var text = string.Join(';', layers.Take(StarSmithSlots.Maximum).Select(l => string.Create(
            CultureInfo.InvariantCulture,
            $"{l.Number}:{(byte)l.Tier}:{l.Look}:{string.Join(',', l.Hull.SelectMany(p => new[] { p.X, p.Y }))}")));
        return text.Length == 0 ? null : text;
    }

    private static StarSmithLayer? Read(string entry)
    {
        var parts = entry.Split(':');
        if (parts.Length != 4
            || !short.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            || !byte.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var tier)
            || tier > (byte)StarSmithSignTier.Chroma
            || !byte.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var look))
        {
            return null;
        }

        var coordinates = new List<ushort>();
        foreach (var value in parts[3].Split(','))
        {
            if (!ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var coordinate))
            {
                return null;
            }

            coordinates.Add(coordinate);
        }

        if (coordinates.Count % 2 != 0 || coordinates.Count < 6 || coordinates.Count > 2 * StarSmithHull.MaximumPoints)
        {
            return null;
        }

        var hull = Enumerable.Range(0, coordinates.Count / 2).Select(i => new StarSmithPoint(coordinates[2 * i], coordinates[(2 * i) + 1])).ToList();
        return new StarSmithLayer(number, (StarSmithSignTier)tier, look, hull);
    }
}
