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
/// <param name="Text">A found recipe's name, a sealed row's hint.</param>
public readonly record struct StarSmithGuidebookEntry(StarSmithGuidebookEntryKind Kind, short Recipe, int Made, string Text)
{
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
    /// <param name="hint">The hint.</param>
    /// <returns>The row.</returns>
    public static StarSmithGuidebookEntry Sealed(string hint) => new(StarSmithGuidebookEntryKind.Sealed, 0, 0, hint);
}
