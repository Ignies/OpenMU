// <copyright file="StarSmithGuidebook.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Globalization;

/// <summary>
/// The smith's Guidebook as stored on the character (<see cref="Character.StarSmithGuidebook"/>): "number:made:seen"
/// entries joined by ';', sorted by number; "seen" is the bit mask of the outcomes the smith rolled, by index, and is
/// left out while it is 0 (a PR 5 "number:made" pair reads as seen 0). A recipe is hidden until made once; then it is
/// known with its made count.
/// </summary>
public static class StarSmithGuidebook
{
    /// <summary>
    /// Reads the stored text; unreadable parts are skipped.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The found recipes with their made counts.</returns>
    public static IReadOnlyDictionary<short, int> Parse(string? text) => Read(text).ToDictionary(e => e.Key, e => e.Value.Made);

    /// <summary>
    /// Reads the outcomes seen per found recipe, a bit mask by outcome index (0 when none is stored).
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The masks by recipe.</returns>
    public static IReadOnlyDictionary<short, int> Seen(string? text) => Read(text).ToDictionary(e => e.Key, e => e.Value.Seen);

    /// <summary>
    /// Records one more making of a recipe (a new recipe starts at 1) and the outcome it rolled.
    /// </summary>
    /// <param name="text">The stored text.</param>
    /// <param name="recipe">The recipe's number.</param>
    /// <param name="outcome">The index of the outcome rolled (0 to 30), or -1 for none.</param>
    /// <returns>The new text.</returns>
    public static string Record(string? text, short recipe, int outcome = -1)
    {
        var found = Read(text);
        var (made, seen) = found.GetValueOrDefault(recipe);
        found[recipe] = (made + 1, outcome is >= 0 and < 31 ? seen | (1 << outcome) : seen);
        return string.Join(';', found.OrderBy(f => f.Key).Select(f => f.Value.Seen == 0
            ? string.Create(CultureInfo.InvariantCulture, $"{f.Key}:{f.Value.Made}")
            : string.Create(CultureInfo.InvariantCulture, $"{f.Key}:{f.Value.Made}:{f.Value.Seen}")));
    }

    private static Dictionary<short, (int Made, int Seen)> Read(string? text)
    {
        var found = new Dictionary<short, (int Made, int Seen)>();
        foreach (var part in (text ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pair = part.Split(':');
            var seen = 0;
            if (pair.Length is 2 or 3
                && short.TryParse(pair[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var recipe)
                && int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var made)
                && made > 0
                && (pair.Length == 2 || int.TryParse(pair[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out seen)))
            {
                found[recipe] = (made, Math.Max(0, seen));
            }
        }

        return found;
    }
}
