// <copyright file="StarSmithSky.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The sky of one attempt (section 10, rule 1): the stars inside the item, which pairs may be linked, the hints, how
/// many stars may be struck, and the recipes it can make. <see cref="Deal"/> makes it from a seed, so the server can
/// deal the same sky again to replay and verify an attempt.
/// </summary>
public sealed class StarSmithSky
{
    private const int ElementCount = 5;

    private const int HighestValue = 5;

    private readonly HashSet<(int A, int B)> linkSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="StarSmithSky"/> class.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="links">The pairs of stars that may be linked (either way).</param>
    /// <param name="hints">The hints.</param>
    /// <param name="strikes">How many stars may be struck.</param>
    /// <param name="recipes">The recipes it can make; those without a shape that fits the sky are left out.</param>
    /// <param name="guarded">Whether a Jewel of Guardian is among the catalysts.</param>
    public StarSmithSky(IReadOnlyList<StarSmithStar> stars, IEnumerable<(int A, int B)> links, IReadOnlyList<StarSmithHint> hints, int strikes, IReadOnlyList<StarSmithRecipe> recipes, bool guarded)
    {
        this.Stars = stars;
        this.linkSet = links
            .Where(l => l.A != l.B && l.A >= 0 && l.B >= 0 && l.A < stars.Count && l.B < stars.Count)
            .Select(Normalized)
            .ToHashSet();
        this.Links = this.linkSet.OrderBy(l => l.A).ThenBy(l => l.B).ToList();
        this.Hints = hints;
        this.Strikes = Math.Clamp(strikes, 0, stars.Count);
        this.Recipes = recipes.Where(r => IsDrawable(r.Shape, stars.Count)).ToList();
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
    /// Gets the risk the client is told: the worst fate among the recipes after protection (a destroy only on a risky
    /// recipe without a Jewel of Guardian, as a crack decides it).
    /// </summary>
    public StarSmithOnFail Risk => this.Recipes.Count == 0 ? StarSmithOnFail.IngredientsLost : this.Recipes.Max(this.FateOf);

    /// <summary>
    /// Deals a sky. The order of the draws is part of the contract: a replay with the same inputs deals the same sky.
    /// <list type="number">
    /// <item>Positions inside the item's box, kept apart by the minimum spacing where the box allows.</item>
    /// <item>Elements and values.</item>
    /// <item>Voids: each recipe rolls its chance (section 3); a failed roll puts a void on a star of its shape past the
    /// first, so following that shape cracks. Then the exploration voids go on stars of no shape.</item>
    /// <item>The void a Jewel of Guardian reveals.</item>
    /// <item>The hints, from the shapes' edges: 1 + level / 20 of them, clearer with the level.</item>
    /// </list>
    /// Links join each star to its nearest neighbours, and every shape edge is a link, so every shape can be drawn.
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
    {
        rules ??= new StarSmithForgeRules();
        var dice = new StarSmithDice(seed);
        var count = Math.Max(2, rules.StarCount);
        var level = Math.Clamp(smithLevel, 1, StarSmithLevels.Maximum);
        var fitting = recipes.Where(r => StarSmithRecipes.Fits(r, target) && IsDrawable(r.Shape, count)).ToList();

        var points = Place(dice, count, rules);
        var elements = new StarSmithElement[count];
        var values = new int[count];
        for (var i = 0; i < count; i++)
        {
            elements[i] = (StarSmithElement)dice.Next(ElementCount);
            values[i] = dice.Next(1, HighestValue + 1);
        }

        var voids = new bool[count];
        foreach (var recipe in fitting)
        {
            if (!StarSmithRecipes.Succeeds(dice.Next(10_000), StarSmithRecipes.Chance(recipe, level, catalystBonus)))
            {
                var spots = VoidSpots(recipe, fitting);
                voids[spots[dice.Next(spots.Count)]] = true;
            }
        }

        var onShape = fitting.SelectMany(r => r.Shape!.Stars).ToHashSet();
        var free = Enumerable.Range(0, count).Where(s => !onShape.Contains(s)).ToList();
        dice.Shuffle(free);
        foreach (var star in free.Take(Math.Max(0, rules.ExplorationVoids)))
        {
            voids[star] = true;
        }

        var guarded = catalysts.Any(c => c.Group == rules.GuardianGroup && c.Number == rules.GuardianNumber);
        var revealed = -1;
        var allVoids = Enumerable.Range(0, count).Where(s => voids[s]).ToList();
        if (guarded && allVoids.Count > 0)
        {
            revealed = allVoids[dice.Next(allVoids.Count)];
        }

        var edges = fitting.SelectMany(r => Edges(r.Shape!)).Distinct().ToList();
        dice.Shuffle(edges);
        var clarity = 0.25 + (0.75 * level / StarSmithLevels.Maximum);
        var hints = edges.Take(1 + (level / 20)).Select(e => new StarSmithHint(e.A, e.B, clarity)).ToList();

        var stars = Enumerable.Range(0, count)
            .Select(i => new StarSmithStar(points[i].X, points[i].Y, elements[i], values[i], voids[i], i == revealed))
            .ToList();
        var links = Nearest(points, rules.Neighbours).Concat(edges);
        var strikes = Math.Clamp(rules.BaseStrikes + ingredientCount, 1, count);
        return new StarSmithSky(stars, links, hints, strikes, fitting, guarded);
    }

    /// <summary>
    /// Whether two stars may be linked.
    /// </summary>
    /// <param name="a">One star.</param>
    /// <param name="b">The other star.</param>
    /// <returns>True when they may.</returns>
    public bool IsLinked(int a, int b) => this.linkSet.Contains(Normalized((a, b)));

    /// <summary>
    /// The fate of a crack on a recipe: its onFail, but a destroy only on a risky recipe without a Jewel of Guardian.
    /// </summary>
    /// <param name="recipe">The recipe, or null off every shape.</param>
    /// <returns>The fate.</returns>
    public StarSmithOnFail FateOf(StarSmithRecipe? recipe)
    {
        var fail = recipe?.OnFail ?? StarSmithOnFail.IngredientsLost;
        return fail == StarSmithOnFail.ItemDestroyed && (!recipe!.Risky || this.Guarded) ? StarSmithOnFail.IngredientsLost : fail;
    }

    /// <summary>
    /// The guidance for a drawn path: the recipes it is still a prefix of.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <param name="known">The recipes the smith knows, or null for all.</param>
    /// <returns>The recipes, in the sky's order.</returns>
    public IReadOnlyList<StarSmithRecipe> Guidance(IReadOnlyList<int> path, IReadOnlySet<short>? known = null)
        => this.Recipes.Where(r => (known is null || known.Contains(r.Number)) && r.Shape!.IsPrefix(path)).ToList();

    /// <summary>
    /// The recipe a sealed path makes: of the shapes it completes, the longest, then the lowest number.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <returns>The recipe and the links past its shape, or null and 0 for the attunement.</returns>
    public (StarSmithRecipe? Recipe, int Extension) Completed(IReadOnlyList<int> path)
    {
        var best = this.Recipes
            .Select(r => (Recipe: r, Extension: r.Shape!.Extension(path)))
            .Where(c => c.Extension >= 0)
            .OrderByDescending(c => c.Recipe.Shape!.Stars.Count)
            .ThenBy(c => c.Recipe.Number)
            .FirstOrDefault();
        return best.Recipe is null ? (null, 0) : best;
    }

    /// <summary>
    /// The recipe a path is on, for its onFail when it cracks: of the shapes it follows, the longest, then the lowest
    /// number.
    /// </summary>
    /// <param name="path">The struck stars, in order, the void included.</param>
    /// <returns>The recipe, or null when it follows none.</returns>
    public StarSmithRecipe? Followed(IReadOnlyList<int> path)
        => this.Recipes
            .Where(r => r.Shape!.Follows(path))
            .OrderByDescending(r => r.Shape!.Stars.Count)
            .ThenBy(r => r.Number)
            .FirstOrDefault();

    private static (int A, int B) Normalized((int A, int B) link) => link.A <= link.B ? link : (link.B, link.A);

    private static bool IsDrawable(StarSmithShape? shape, int starCount)
        => shape is { Stars.Count: >= 2 }
           && shape.Stars.All(s => s >= 0 && s < starCount)
           && shape.Stars.Distinct().Count() == shape.Stars.Count;

    private static IEnumerable<(int A, int B)> Edges(StarSmithShape shape)
        => shape.Stars.Zip(shape.Stars.Skip(1), (a, b) => Normalized((a, b)));

    /// <summary>
    /// The stars of a recipe's shape a failed roll may put its void on: past the first star (past both ends when
    /// unordered), preferring stars no other shape uses, so one failed roll spoils one recipe.
    /// </summary>
    private static List<int> VoidSpots(StarSmithRecipe recipe, List<StarSmithRecipe> fitting)
    {
        var shape = recipe.Shape!;
        var inner = shape.Stars.Skip(1).Take(shape.Ordered ? shape.Stars.Count - 1 : shape.Stars.Count - 2).ToList();
        if (inner.Count == 0)
        {
            inner = [shape.Stars[^1]];
        }

        var own = inner.Where(s => !fitting.Any(o => o != recipe && o.Shape!.Stars.Contains(s))).ToList();
        return own.Count > 0 ? own : inner;
    }

    private static List<(double X, double Y)> Place(StarSmithDice dice, int count, StarSmithForgeRules rules)
    {
        const int tries = 64;
        var margin = Math.Clamp(rules.Margin, 0, 0.49);
        var span = 1 - (2 * margin);
        var spacing = rules.MinimumSpacing * rules.MinimumSpacing;
        var points = new List<(double X, double Y)>(count);
        for (var i = 0; i < count; i++)
        {
            (double X, double Y) point = default;
            for (var attempt = 0; attempt < tries; attempt++)
            {
                point = (margin + (dice.NextDouble() * span), margin + (dice.NextDouble() * span));
                if (points.All(p => DistanceSquared(p, point) >= spacing))
                {
                    break;
                }
            }

            points.Add(point);
        }

        return points;
    }

    private static IEnumerable<(int A, int B)> Nearest(List<(double X, double Y)> points, int neighbours)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var from = i;
            foreach (var to in Enumerable.Range(0, points.Count)
                         .Where(j => j != from)
                         .OrderBy(j => DistanceSquared(points[from], points[j]))
                         .ThenBy(j => j)
                         .Take(Math.Max(0, neighbours)))
            {
                yield return (from, to);
            }
        }
    }

    private static double DistanceSquared((double X, double Y) a, (double X, double Y) b)
        => ((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y));
}
