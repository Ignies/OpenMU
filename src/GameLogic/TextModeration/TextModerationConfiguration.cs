// <copyright file="TextModerationConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.TextModeration;

/// <summary>
/// The word list of <c>TextModerationPlugIn</c>, which the website edits.
/// </summary>
public class TextModerationConfiguration
{
    /// <summary>Gets or sets the entries.</summary>
    public ICollection<ModeratedWord> Words { get; set; } = new List<ModeratedWord>();
}
