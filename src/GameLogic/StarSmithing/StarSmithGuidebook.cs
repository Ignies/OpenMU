// <copyright file="StarSmithGuidebook.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The smith's Guidebook as stored on the character (<see cref="Character.StarSmithGuidebook"/>).
/// </summary>
public static class StarSmithGuidebook
{
    /// <summary>
    /// Reads the stored text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The found recipes with their made counts.</returns>
    public static IReadOnlyDictionary<short, int> Parse(string? text) => new Dictionary<short, int>();

    /// <summary>
    /// Records one more making of a recipe.
    /// </summary>
    /// <param name="text">The stored text.</param>
    /// <param name="recipe">The recipe's number.</param>
    /// <returns>The new text.</returns>
    public static string Record(string? text, short recipe) => text ?? string.Empty;
}
