// <copyright file="ItemCustomizationText.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

using System.Globalization;
using System.Text;

/// <summary>
/// Names and lore fitted to their fixed wire fields, and the flags of an entry (Readme.md).
/// </summary>
public static class ItemCustomizationText
{
    /// <summary>
    /// The bytes of the name field.
    /// </summary>
    public const int NameBytes = 72;

    /// <summary>
    /// The bytes of the lore field.
    /// </summary>
    public const int LoreBytes = 420;

    /// <summary>
    /// The most characters of a name.
    /// </summary>
    public const int NameCharacters = 24;

    /// <summary>
    /// The most characters of a lore.
    /// </summary>
    public const int LoreCharacters = 140;

    /// <summary>
    /// The most lines of a lore.
    /// </summary>
    public const int LoreLines = 3;

    /// <summary>
    /// Writes a name: at most <see cref="NameCharacters"/>, UTF-8, zero padded.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="target">The field.</param>
    public static void WriteName(string? name, Span<byte> target) => Write(name?.Trim(), NameCharacters, target);

    /// <summary>
    /// Writes a lore: at most <see cref="LoreLines"/> lines and <see cref="LoreCharacters"/>, UTF-8, zero padded.
    /// </summary>
    /// <param name="lore">The lore.</param>
    /// <param name="target">The field.</param>
    public static void WriteLore(string? lore, Span<byte> target)
    {
        var lines = lore?.Replace("\r\n", "\n").Split('\n').Take(LoreLines);
        Write(lines is null ? null : string.Join('\n', lines).Trim(), LoreCharacters, target);
    }

    /// <summary>
    /// Gets the flags of an entry: 1 look, 2 name, 4 lore waiting.
    /// </summary>
    /// <param name="hasLook">If the item has a look.</param>
    /// <param name="name">The name.</param>
    /// <param name="lore">The lore.</param>
    /// <returns>The flags.</returns>
    public static byte Flags(bool hasLook, string? name, string? lore) =>
        (byte)((hasLook ? 1 : 0) | (string.IsNullOrWhiteSpace(name) ? 0 : 2) | (string.IsNullOrWhiteSpace(lore) ? 0 : 4));

    /// <summary>
    /// Writes the first characters of a text that fit, cut on a character (a text element), never inside one.
    /// </summary>
    private static void Write(string? text, int characters, Span<byte> target)
    {
        target.Clear();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var elements = StringInfo.GetTextElementEnumerator(text.Normalize(NormalizationForm.FormC));
        var written = 0;
        var count = 0;
        while (count < characters && elements.MoveNext())
        {
            var element = (string)elements.Current;
            var size = Encoding.UTF8.GetByteCount(element);
            if (written + size > target.Length)
            {
                break;
            }

            written += Encoding.UTF8.GetBytes(element, target[written..]);
            count++;
        }
    }
}
