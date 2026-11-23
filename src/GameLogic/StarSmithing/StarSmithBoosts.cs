// <copyright file="StarSmithBoosts.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Globalization;

/// <summary>
/// The boosts as stored on the item's record (ItemCustomization.Boosts): "stat:value"
/// pairs joined by ';', the value times 100.
/// </summary>
public static class StarSmithBoosts
{
    /// <summary>The most boosts an item carries.</summary>
    public const int Maximum = 3;

    /// <summary>
    /// Reads the stored text; unreadable parts and unknown stats are skipped.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The boosts.</returns>
    public static IReadOnlyList<StarSmithBoost> Parse(string? text)
    {
        var boosts = new List<StarSmithBoost>();
        foreach (var part in (text ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pair = part.Split(':');
            if (pair.Length == 2
                && byte.TryParse(pair[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var stat)
                && Enum.IsDefined((StarSmithBoostStat)stat)
                && int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                && boosts.Count < Maximum)
            {
                boosts.Add(new StarSmithBoost((StarSmithBoostStat)stat, value));
            }
        }

        return boosts;
    }

    /// <summary>
    /// Writes boosts as stored text; null when there are none.
    /// </summary>
    /// <param name="boosts">The boosts.</param>
    /// <returns>The text.</returns>
    public static string? Format(IEnumerable<StarSmithBoost> boosts)
    {
        var text = string.Join(';', boosts.Take(Maximum).Select(b => string.Create(CultureInfo.InvariantCulture, $"{(byte)b.Stat}:{b.Value}")));
        return text.Length == 0 ? null : text;
    }
}
