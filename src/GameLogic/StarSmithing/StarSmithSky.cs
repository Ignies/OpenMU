// <copyright file="StarSmithSky.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A star's element, the flavour of an effect.
/// </summary>
public enum StarSmithElement
{
    /// <summary>Fire.</summary>
    Fire,

    /// <summary>Frost.</summary>
    Frost,

    /// <summary>Storm.</summary>
    Storm,

    /// <summary>Shadow.</summary>
    Shadow,

    /// <summary>Light.</summary>
    Light,
}

/// <summary>
/// A star of the sky. Its element, value and void are the server's until it is struck (or, for one void, revealed by
/// a Jewel of Guardian).
/// </summary>
/// <param name="X">Its position across the item's box, 0 to 1.</param>
/// <param name="Y">Its position down the item's box, 0 to 1.</param>
/// <param name="Element">Its element.</param>
/// <param name="Value">Its value, 1 to 5: its weight in the element mix.</param>
/// <param name="IsVoid">Whether it is a void, which cracks the attempt.</param>
/// <param name="Revealed">Whether the client is shown it is a void before the first strike.</param>
public readonly record struct StarSmithStar(double X, double Y, StarSmithElement Element, int Value, bool IsVoid, bool Revealed = false);

/// <summary>
/// A glimmering line between two stars, an edge of a recipe's shape.
/// </summary>
/// <param name="From">One star.</param>
/// <param name="To">The other star.</param>
/// <param name="Clarity">How clearly it shows, 0 to 1.</param>
public readonly record struct StarSmithHint(int From, int To, double Clarity);

/// <summary>
/// The sky of one attempt (section 10, rule 1): the stars inside the item, which pairs may be linked, the hints, how
/// many stars may be struck, and the recipes it can make. <see cref="Deal"/> makes it from a seed, so the server can
/// deal the same sky again to replay and verify an attempt.
/// </summary>
public sealed class StarSmithSky
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StarSmithSky"/> class.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="links">The pairs of stars that may be linked (either way).</param>
    /// <param name="hints">The hints.</param>
    /// <param name="strikes">How many stars may be struck.</param>
    /// <param name="recipes">The recipes it can make.</param>
    /// <param name="guarded">Whether a Jewel of Guardian is among the catalysts.</param>
    public StarSmithSky(IReadOnlyList<StarSmithStar> stars, IEnumerable<(int A, int B)> links, IReadOnlyList<StarSmithHint> hints, int strikes, IReadOnlyList<StarSmithRecipe> recipes, bool guarded)
    {
        this.Stars = stars;
        this.Links = [];
        this.Hints = hints;
        this.Strikes = strikes;
        this.Recipes = recipes;
        this.Guarded = guarded;
    }

    /// <summary>Gets the stars.</summary>
    public IReadOnlyList<StarSmithStar> Stars { get; }

    /// <summary>Gets the pairs of stars that may be linked, the lower index first, sorted.</summary>
    public IReadOnlyList<(int A, int B)> Links { get; }

    /// <summary>Gets the hints.</summary>
    public IReadOnlyList<StarSmithHint> Hints { get; }

    /// <summary>Gets how many stars may be struck.</summary>
    public int Strikes { get; }

    /// <summary>Gets the recipes it can make, those with a shape that fits the sky.</summary>
    public IReadOnlyList<StarSmithRecipe> Recipes { get; }

    /// <summary>Gets a value indicating whether a Jewel of Guardian is among the catalysts.</summary>
    public bool Guarded { get; }

    /// <summary>
    /// Deals a sky.
    /// </summary>
    /// <param name="seed">The attempt's seed.</param>
    /// <param name="recipes">The recipes the ingredients allow; those whose target rule does not fit are left out.</param>
    /// <param name="target">The item worked on.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="ingredientCount">How many ingredients are in the grid.</param>
    /// <param name="catalysts">The catalysts by item.</param>
    /// <param name="catalystBonus">The catalysts' chance bonus, in hundredths of a percent.</param>
    /// <param name="rules">The Forge's rules, or null for the defaults.</param>
    /// <returns>The sky.</returns>
    public static StarSmithSky Deal(long seed, IEnumerable<StarSmithRecipe> recipes, StarSmithTarget target, int smithLevel, int ingredientCount, IReadOnlyCollection<(byte Group, short Number)> catalysts, int catalystBonus = 0, StarSmithForgeRules? rules = null)
        => new([], [], [], 0, [], false);

    /// <summary>
    /// Whether two stars may be linked.
    /// </summary>
    /// <param name="a">One star.</param>
    /// <param name="b">The other star.</param>
    /// <returns>True when they may.</returns>
    public bool IsLinked(int a, int b) => false;

    /// <summary>
    /// The guidance for a drawn path: the recipes it is still a prefix of.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <param name="known">The recipes the smith knows, or null for all.</param>
    /// <returns>The recipes, in the sky's order.</returns>
    public IReadOnlyList<StarSmithRecipe> Guidance(IReadOnlyList<int> path, IReadOnlySet<short>? known = null) => [];

    /// <summary>
    /// The recipe a sealed path makes: of the shapes it completes, the longest, then the lowest number.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <returns>The recipe and the links past its shape, or null and 0 for the attunement.</returns>
    public (StarSmithRecipe? Recipe, int Extension) Completed(IReadOnlyList<int> path) => (null, 0);

    /// <summary>
    /// The recipe a path is on, for its onFail when it cracks: of the shapes it follows, the longest, then the lowest
    /// number.
    /// </summary>
    /// <param name="path">The struck stars, in order, the void included.</param>
    /// <returns>The recipe, or null when it follows none.</returns>
    public StarSmithRecipe? Followed(IReadOnlyList<int> path) => null;
}
