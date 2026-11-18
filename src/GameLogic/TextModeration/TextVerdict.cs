// <copyright file="TextVerdict.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.TextModeration;

/// <summary>
/// The answer of <see cref="TextModeration.Check"/>.
/// </summary>
/// <param name="Text">The text to store (NFC, trimmed, lines joined with a line feed), or null when refused.</param>
/// <param name="Refusal">Why it is refused, or <see cref="TextRefusal.None"/>.</param>
/// <param name="Word">The word list entry it hit, for <see cref="TextRefusal.Word"/>.</param>
public readonly record struct TextVerdict(string? Text, TextRefusal Refusal, string? Word)
{
    /// <summary>Gets a value indicating whether the text passes.</summary>
    public bool Ok => this.Refusal == TextRefusal.None;
}
