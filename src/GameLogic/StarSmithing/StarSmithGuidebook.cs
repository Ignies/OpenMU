// <copyright file="StarSmithGuidebook.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Globalization;

/// <summary>
/// The smith's Guidebook as stored on the character (<see cref="Character.StarSmithGuidebook"/>): "number:made" pairs
/// joined by ';', sorted by number. A recipe is hidden until made once; then it is known with its made count.
/// </summary>
public static class StarSmithGuidebook
{
    /// <summary>
    /// Reads the stored text; unreadable parts are skipped.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The found recipes with their made counts.</returns>
    public static IReadOnlyDictionary<short, int> Parse(string? text)
    {
        var found = new Dictionary<short, int>();
        foreach (var part in (text ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pair = part.Split(':');
            if (pair.Length == 2
                && short.TryParse(pair[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var recipe)
                && int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var made)
                && made > 0)
            {
                found[recipe] = made;
            }
        }

        return found;
    }

    /// <summary>
    /// Records one more making of a recipe (a new recipe starts at 1).
    /// </summary>
    /// <param name="text">The stored text.</param>
    /// <param name="recipe">The recipe's number.</param>
    /// <returns>The new text.</returns>
    public static string Record(string? text, short recipe)
    {
        var found = new Dictionary<short, int>(Parse(text));
        found[recipe] = found.GetValueOrDefault(recipe) + 1;
        return string.Join(';', found.OrderBy(f => f.Key).Select(f => string.Create(CultureInfo.InvariantCulture, $"{f.Key}:{f.Value}")));
    }
}
