// <copyright file="StarSmithGuidebookEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A row of the smith's Guidebook (EE 90): a found recipe in full, or a sealed one with its hint only.
/// </summary>
/// <param name="Kind">Found or sealed.</param>
/// <param name="Recipe">A found recipe's number; 0 for a sealed row.</param>
/// <param name="Made">How often the smith made a found recipe; 0 for a sealed row.</param>
/// <param name="Text">A found recipe's name, a sealed row's riddle; empty otherwise.</param>
public readonly record struct StarSmithGuidebookEntry(StarSmithGuidebookEntryKind Kind, short Recipe, int Made, string Text)
{
    /// <summary>Gets a found recipe's shape length in stars; 0 for a sealed row.</summary>
    public int ShapeLength { get; init; }

    /// <summary>Gets a sealed row's hint kind; none for a found row.</summary>
    public StarSmithHintKind HintKind { get; init; }

    /// <summary>Gets the item group of an ingredient hint.</summary>
    public byte HintGroup { get; init; }

    /// <summary>Gets the item number of an ingredient hint.</summary>
    public short HintNumber { get; init; }

    /// <summary>Gets a found recipe's ingredients; empty for a sealed row.</summary>
    public IReadOnlyList<StarSmithIngredient> Ingredients { get; init; } = [];

    /// <summary>Gets, per stat, the range of the outcomes the smith has seen of a found recipe; empty for a sealed row.</summary>
    public IReadOnlyList<StarSmithBoostSeen> BoostsSeen { get; init; } = [];

    /// <summary>
    /// A found recipe.
    /// </summary>
    /// <param name="recipe">Its number.</param>
    /// <param name="made">How often the smith made it.</param>
    /// <param name="name">Its name.</param>
    /// <returns>The row.</returns>
    public static StarSmithGuidebookEntry Found(short recipe, int made, string name) => new(StarSmithGuidebookEntryKind.Found, recipe, made, name);

    /// <summary>
    /// A sealed row: only the hint, nothing that names the recipe.
    /// </summary>
    /// <param name="kind">The hint's kind.</param>
    /// <param name="riddle">A riddle's text.</param>
    /// <param name="group">An ingredient hint's item group.</param>
    /// <param name="number">An ingredient hint's item number.</param>
    /// <returns>The row.</returns>
    public static StarSmithGuidebookEntry Sealed(StarSmithHintKind kind, string riddle, byte group, short number)
        => new(StarSmithGuidebookEntryKind.Sealed, 0, 0, riddle) { HintKind = kind, HintGroup = group, HintNumber = number };
}
