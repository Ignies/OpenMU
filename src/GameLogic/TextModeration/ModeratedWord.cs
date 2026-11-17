// <copyright file="ModeratedWord.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.TextModeration;

/// <summary>
/// An entry of the word list.
/// </summary>
public class ModeratedWord
{
    /// <summary>Gets or sets the word, as the admin wrote it (it is folded like the text before the check).</summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the word is refused inside other words too (else a whole word only).</summary>
    public bool Substring { get; set; }

    /// <summary>Gets or sets the language the admin files it under (empty for any); every entry applies to every text.</summary>
    public string Language { get; set; } = string.Empty;
}
