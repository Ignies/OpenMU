// <copyright file="TextModerationPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.TextModeration;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Properties;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Keeps the word list player texts are checked against (item names and lore, Readme.md). Active by default: it only
/// holds the list, which the website edits (<c>items.setWords</c>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.TextModerationPlugIn_Name), Description = nameof(PlugInResources.TextModerationPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("BD3EC1B2-3B76-4772-B8A5-8DFA8E9A2140")]
public class TextModerationPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<TextModerationConfiguration>, ISupportDefaultCustomConfiguration
{
    /// <summary>
    /// Gets or sets the word list.
    /// </summary>
    public TextModerationConfiguration? Configuration { get; set; }

    /// <summary>
    /// Checks a player text with the word list of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="kind">A name or a lore.</param>
    /// <param name="text">The text.</param>
    /// <returns>The verdict.</returns>
    public static TextVerdict Check(IGameContext gameContext, TextKind kind, string? text) =>
        TextModeration.Check(kind, text, gameContext.FeaturePlugIns.GetPlugIn<TextModerationPlugIn>()?.Configuration);

    /// <inheritdoc />
    public object CreateDefaultConfig() => new TextModerationConfiguration();
}
