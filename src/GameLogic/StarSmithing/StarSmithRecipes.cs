// <copyright file="StarSmithRecipes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The recipe engine's pure parts: which recipe an attempt makes, its chance, and how the smith's level scales a roll.
/// </summary>
public static class StarSmithRecipes
{
    /// <summary>
    /// Finds the recipe a target and its ingredients make.
    /// </summary>
    /// <param name="recipes">The recipes.</param>
    /// <param name="target">The item worked on.</param>
    /// <param name="ingredients">The ingredients by item, with their counts.</param>
    /// <returns>The recipe, or null.</returns>
    public static StarSmithRecipe? Match(IEnumerable<StarSmithRecipe> recipes, StarSmithTarget target, IReadOnlyCollection<(byte Group, short Number, int Count)> ingredients) => null;

    /// <summary>
    /// The chance of an attempt, in hundredths of a percent.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="catalysts">The catalysts' bonus, in hundredths of a percent.</param>
    /// <returns>The chance.</returns>
    public static int Chance(StarSmithRecipe recipe, int smithLevel, int catalysts) => 0;

    /// <summary>
    /// A rolled magnitude, scaled by the smith's level.
    /// </summary>
    /// <param name="rolled">The rolled magnitude.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <returns>The scaled magnitude.</returns>
    public static double Magnitude(double rolled, int smithLevel) => 0;

    /// <summary>
    /// Whether a roll succeeds.
    /// </summary>
    /// <param name="roll">The roll, 0 to 9,999.</param>
    /// <param name="chance">The chance, in hundredths of a percent.</param>
    /// <returns>True on a success.</returns>
    public static bool Succeeds(int roll, int chance) => false;
}
