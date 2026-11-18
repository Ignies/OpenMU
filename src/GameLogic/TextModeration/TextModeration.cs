// <copyright file="TextModeration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.TextModeration;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// The rules of player text on items (Readme.md): a name or a lore line is checked on NFC text, then against the word
/// list with look-alike letters, digits and symbols folded to the letters they stand for.
/// </summary>
public static partial class TextModeration
{
    /// <summary>The fewest text elements of a name.</summary>
    public const int NameMinimum = 3;

    /// <summary>The most text elements of a name.</summary>
    public const int NameMaximum = 24;

    /// <summary>The most text elements of a lore.</summary>
    public const int LoreMaximum = 140;

    /// <summary>The most lines of a lore.</summary>
    public const int LoreLines = 3;

    private static readonly Dictionary<char, char> LookAlikes = BuildLookAlikes();

    /// <summary>
    /// Checks a text.
    /// </summary>
    /// <param name="kind">A name or a lore.</param>
    /// <param name="text">The text as written.</param>
    /// <param name="configuration">The word list, or null for the rules alone.</param>
    /// <returns>The text to store, or why it is refused.</returns>
    public static TextVerdict Check(TextKind kind, string? text, TextModerationConfiguration? configuration)
    {
        var normal = (text ?? string.Empty).Normalize(NormalizationForm.FormC).Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
        if (normal.Length == 0)
        {
            return new(null, TextRefusal.Empty, null);
        }

        var refusal = kind == TextKind.Name ? NameRefusal(normal) : LoreRefusal(normal);
        if (refusal != TextRefusal.None)
        {
            return new(null, refusal, null);
        }

        return WordOf(normal, configuration) is { } word ? new(null, TextRefusal.Word, word) : new(normal, TextRefusal.None, null);
    }

    /// <summary>
    /// Folds a text for the word check: lower case, accents dropped, look-alike letters and leet digits and symbols read
    /// as the latin letters they stand for.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The folded text.</returns>
    public static string Fold(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var folded = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var lower = char.ToLowerInvariant(c);
            folded.Append(LookAlikes.TryGetValue(lower, out var latin) ? latin : lower);
        }

        return folded.ToString();
    }

    private static TextRefusal NameRefusal(string name)
    {
        var length = new StringInfo(name).LengthInTextElements;
        if (length < NameMinimum)
        {
            return TextRefusal.TooShort;
        }

        if (length > NameMaximum)
        {
            return TextRefusal.TooLong;
        }

        if (name.Contains("  ", StringComparison.Ordinal))
        {
            return TextRefusal.DoubleSpace;
        }

        foreach (var c in name)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            var allowed = char.IsLetter(c) || category is UnicodeCategory.DecimalDigitNumber or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark
                || c is ' ' or '\'' or '-';
            if (!allowed)
            {
                return TextRefusal.BadCharacter;
            }
        }

        return TextRefusal.None;
    }

    private static TextRefusal LoreRefusal(string lore)
    {
        foreach (var c in lore)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (c != '\n' && category is UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator)
            {
                return TextRefusal.BadCharacter;
            }
        }

        if (new StringInfo(lore.Replace("\n", string.Empty, StringComparison.Ordinal)).LengthInTextElements > LoreMaximum)
        {
            return TextRefusal.TooLong;
        }

        if (lore.Split('\n').Length > LoreLines)
        {
            return TextRefusal.TooManyLines;
        }

        if (LinkPattern().IsMatch(lore))
        {
            return TextRefusal.Link;
        }

        return ColourPattern().IsMatch(lore) ? TextRefusal.ColourCode : TextRefusal.None;
    }

    private static string? WordOf(string text, TextModerationConfiguration? configuration)
    {
        if (configuration?.Words is not { Count: > 0 } words)
        {
            return null;
        }

        var folded = Fold(text);
        string[]? tokens = null;
        foreach (var entry in words)
        {
            var word = Fold(entry.Word.Trim());
            if (word.Length == 0)
            {
                continue;
            }

            if (entry.Substring ? folded.Contains(word, StringComparison.Ordinal) : (tokens ??= TokenPattern().Split(folded)).Contains(word, StringComparer.Ordinal))
            {
                return entry.Word;
            }
        }

        return null;
    }

    private static Dictionary<char, char> BuildLookAlikes()
    {
        // Cyrillic and Greek letters drawn like latin ones, and the digits and symbols written for letters.
        const string pairs = "аaеeоoрpсcуyхxкkмmтtвbнhіiјjѕsԁdӏlαaοoρpεeιiκkνvτtυuχxβbζzηhμm0o1i3e4a5s7t8b@a$s";
        var map = new Dictionary<char, char>();
        for (var i = 0; i < pairs.Length; i += 2)
        {
            map[pairs[i]] = pairs[i + 1];
        }

        return map;
    }

    [GeneratedRegex(@"[a-z][a-z0-9+.\-]*://|\bwww\.|\b[\w\-]+\.(?:com|net|org|io|gg|me|info|biz|xyz|ru|es|br|pl|de|uk|co|tv|ly|link|site|online|club|top|app|dev|eu|fr|it|cn|jp|kr|us)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LinkPattern();

    [GeneratedRegex(@"#(?:[0-9a-f]{6}|[0-9a-f]{3})\b|\^[0-9]|&[0-9a-fk-or]\b|\{colou?r\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ColourPattern();

    [GeneratedRegex(@"[^\p{L}\p{N}]+", RegexOptions.CultureInvariant)]
    private static partial Regex TokenPattern();
}
