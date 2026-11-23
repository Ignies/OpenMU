// <copyright file="StarSmithRecipes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The recipe engine's pure parts: which recipe an attempt makes, its chance, and how the smith's level scales a roll.
/// </summary>
public static class StarSmithRecipes
{
    /// <summary>The most a smith's level adds to a chance, in hundredths of a percent.</summary>
    public const int MaximumLevelBonus = 1500;

    /// <summary>What a smith level adds to a chance, in hundredths of a percent.</summary>
    public const int LevelBonus = 15;

    /// <summary>
    /// Finds the recipe a target and its ingredients make.
    /// </summary>
    /// <param name="recipes">The recipes.</param>
    /// <param name="target">The item worked on.</param>
    /// <param name="ingredients">The ingredients by item, with their counts.</param>
    /// <returns>The recipe, or null.</returns>
    public static StarSmithRecipe? Match(IEnumerable<StarSmithRecipe> recipes, StarSmithTarget target, IReadOnlyCollection<(byte Group, short Number, int Count)> ingredients)
        => Candidates(recipes, target, ingredients).FirstOrDefault();

    /// <summary>
    /// All recipes a target and its ingredients can make, the most specific first: more kinds of ingredient, then a
    /// higher item level, then the lower number. The Constellation Forge deals its sky from these and the drawn shape
    /// picks one.
    /// </summary>
    /// <param name="recipes">The recipes.</param>
    /// <param name="target">The item worked on.</param>
    /// <param name="ingredients">The ingredients by item, with their counts.</param>
    /// <returns>The fitting recipes, the most specific first.</returns>
    public static IReadOnlyList<StarSmithRecipe> Candidates(IEnumerable<StarSmithRecipe> recipes, StarSmithTarget target, IReadOnlyCollection<(byte Group, short Number, int Count)> ingredients)
    {
        var counts = new Dictionary<(byte Group, short Number), int>();
        foreach (var (group, number, count) in ingredients)
        {
            counts[(group, number)] = counts.GetValueOrDefault((group, number)) + count;
        }

        return recipes
            .Where(r => Fits(r, target) && Takes(r, counts))
            .OrderByDescending(r => r.Ingredients.Count)
            .ThenByDescending(r => r.MinimumItemLevel)
            .ThenBy(r => r.Number)
            .ToList();
    }

    /// <summary>
    /// Whether a recipe's target rule fits an item: its kind, its lowest level and its excellent rule.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <param name="target">The item worked on.</param>
    /// <returns>True when it fits.</returns>
    public static bool Fits(StarSmithRecipe recipe, StarSmithTarget target)
        => (recipe.Targets & target.Kind) != 0
           && target.Level >= recipe.MinimumItemLevel
           && (recipe.Excellent is null || recipe.Excellent == target.Excellent);

    /// <summary>
    /// The chance of an attempt, in hundredths of a percent.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="catalysts">The catalysts' bonus, in hundredths of a percent.</param>
    /// <returns>The chance.</returns>
    public static int Chance(StarSmithRecipe recipe, int smithLevel, int catalysts) => Chance(recipe, smithLevel, catalysts, null);

    /// <summary>
    /// The chance of an attempt, in hundredths of a percent, with the balance's level bonus.
    /// </summary>
    /// <param name="recipe">The recipe.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="catalysts">The catalysts' bonus, in hundredths of a percent.</param>
    /// <param name="bonus">What a level adds and the most it adds, or null for the default.</param>
    /// <returns>The chance.</returns>
    public static int Chance(StarSmithRecipe recipe, int smithLevel, int catalysts, StarSmithChanceBonus? bonus)
    {
        var levelBonus = Math.Min(bonus?.Maximum ?? MaximumLevelBonus, (bonus?.PerLevel ?? LevelBonus) * Math.Clamp(smithLevel, 0, StarSmithLevels.Maximum));
        var chance = recipe.BaseChance + levelBonus + Math.Max(0, catalysts);
        return Math.Clamp(chance, 0, Math.Clamp(recipe.MaximumChance, 0, 10_000));
    }

    /// <summary>
    /// A rolled magnitude, scaled by the smith's level.
    /// </summary>
    /// <param name="rolled">The rolled magnitude.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <returns>The scaled magnitude.</returns>
    public static double Magnitude(double rolled, int smithLevel)
        => rolled * (1 + (0.5 * Math.Pow(Math.Clamp(smithLevel, 0, StarSmithLevels.Maximum) / (double)StarSmithLevels.Maximum, 0.8)));

    /// <summary>
    /// Whether a roll succeeds.
    /// </summary>
    /// <param name="roll">The roll, 0 to 9,999.</param>
    /// <param name="chance">The chance, in hundredths of a percent.</param>
    /// <returns>True on a success.</returns>
    public static bool Succeeds(int roll, int chance) => roll < chance;

    private static bool Takes(StarSmithRecipe recipe, Dictionary<(byte Group, short Number), int> counts)
    {
        foreach (var ((group, number), count) in counts)
        {
            var ingredient = recipe.Ingredients.FirstOrDefault(i => i.Group == group && i.Number == number);
            if (ingredient is null || count < ingredient.Minimum || count > ingredient.Maximum)
            {
                return false;
            }
        }

        return recipe.Ingredients.All(i => i.Minimum == 0 || counts.ContainsKey((i.Group, i.Number)));
    }
}
