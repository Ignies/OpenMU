// <copyright file="StarSmithForge.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// One Constellation Forge attempt by geometry (Readme.md "PR 7a contract"): the smith places free nodes on the beat;
/// the forge matches each figure to a constellation, finds new ones, checks faults, banks figures (stash) and seals
/// them as layers. Pure and deterministic: the same seed, nodes and times give the same result.
/// </summary>
public sealed class StarSmithForge
{
    /// <summary>The most guide entries an answer carries.</summary>
    public const int MaximumGuide = 8;

    private readonly IReadOnlyList<StarSmithRecipe> recipes;

    private readonly StarSmithForgeRules rules;

    private readonly StarSmithBeat beat;

    private readonly long startMs;

    private readonly int roundTripMs;

    private readonly int smithLevel;

    private readonly HashSet<short> known;

    private readonly IReadOnlyList<StarSmithLayer> layers;

    private readonly bool guarded;

    private readonly List<StarSmithPoint> figure = [];

    private readonly List<StarSmithTiming> figureTimings = [];

    private readonly List<Banked> banked = [];

    private readonly List<StarSmithTiming> timings = [];

    private readonly List<short> found = [];

    private int closedAt = -1;

    private long lastBeat;

    private StarSmithForge(long seed, IReadOnlyList<StarSmithRecipe> recipes, int smithLevel, int ingredientCount, bool guarded, StarSmithForgeRules rules, StarSmithBeat beat, long startMs, int roundTripMs, IReadOnlySet<short> known, IReadOnlyList<StarSmithLayer> layers)
    {
        this.recipes = recipes.Where(r => r.Geometry is { IsWellFormed: true }).ToList();
        this.rules = rules;
        this.beat = beat;
        this.startMs = startMs;
        this.roundTripMs = roundTripMs;
        this.smithLevel = Math.Clamp(smithLevel, 1, StarSmithLevels.Maximum);
        this.known = [.. known];
        this.layers = layers.Take(StarSmithSlots.Maximum).ToList();
        this.guarded = guarded;
        this.Strikes = Math.Clamp(rules.BaseStrikes + ingredientCount, 1, Math.Max(1, rules.MaxStrikes));
        (this.Faults, this.Revealed) = StarSmithFaults.Deal(seed, rules, guarded);
    }

    /// <summary>Gets where the attempt stands.</summary>
    public StarSmithForgeState State { get; private set; }

    /// <summary>Gets the constellations the attempt can make. They are hidden: only matches and known ones are ever named.</summary>
    public IReadOnlyList<StarSmithRecipe> Constellations => this.recipes;

    /// <summary>Gets the fault lines. They are hidden: only the revealed one is ever sent.</summary>
    public IReadOnlyList<StarSmithFault> Faults { get; }

    /// <summary>Gets the index of the fault a Jewel of Guardian reveals, or -1.</summary>
    public int Revealed { get; }

    /// <summary>Gets how many placements the attempt allows.</summary>
    public int Strikes { get; }

    /// <summary>Gets the strikes left.</summary>
    public int StrikesLeft => Math.Max(0, this.Strikes - this.timings.Count);

    /// <summary>Gets the figure being drawn.</summary>
    public IReadOnlyList<StarSmithPoint> Figure => this.figure;

    /// <summary>Gets how many figures are banked.</summary>
    public int BankedCount => this.banked.Count;

    /// <summary>Gets the slots the smith's level gives.</summary>
    public int Slots => StarSmithSlots.For(this.smithLevel);

    /// <summary>Gets the slots still free on the item: the level's slots minus its layers and the banked new ones.</summary>
    public int FreeSlots => Math.Max(0, Math.Max(0, this.Slots - this.layers.Count) - this.banked.Count(b => b.Reseal < 0));

    /// <summary>Gets the item's layers at the start.</summary>
    public IReadOnlyList<StarSmithLayer> Layers => this.layers;

    /// <summary>Gets the constellations found in this attempt, in order.</summary>
    public IReadOnlyList<short> Found => this.found;

    /// <summary>Gets the result once ended.</summary>
    public StarSmithForgeResult? Result { get; private set; }

    /// <summary>
    /// Gets the risk the client is told: the worst fate among the attempt's constellations (a destroy only on a risky
    /// one without a Jewel of Guardian).
    /// </summary>
    public StarSmithOnFail Risk => this.recipes.Count == 0 ? StarSmithOnFail.IngredientsLost : this.recipes.Max(this.FateOf);

    /// <summary>
    /// Starts an attempt; the faults come from the seed.
    /// </summary>
    /// <param name="seed">The attempt's seed.</param>
    /// <param name="recipes">The constellations the attempt can make.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="ingredientCount">How many ingredients are in the grid.</param>
    /// <param name="guarded">Whether a Jewel of Guardian is among the catalysts.</param>
    /// <param name="rules">The rules.</param>
    /// <param name="beat">The beat.</param>
    /// <param name="startMs">The server time it starts.</param>
    /// <param name="roundTripMs">The measured round trip.</param>
    /// <param name="known">The constellations in the smith's Guidebook.</param>
    /// <param name="layers">The item's layers.</param>
    /// <returns>The attempt, drafting.</returns>
    public static StarSmithForge Start(long seed, IReadOnlyList<StarSmithRecipe> recipes, int smithLevel, int ingredientCount, bool guarded, StarSmithForgeRules rules, StarSmithBeat beat, long startMs, int roundTripMs, IReadOnlySet<short> known, IReadOnlyList<StarSmithLayer> layers)
    {
        ArgumentNullException.ThrowIfNull(recipes);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(beat);
        ArgumentNullException.ThrowIfNull(known);
        ArgumentNullException.ThrowIfNull(layers);
        return new StarSmithForge(seed, recipes, smithLevel, ingredientCount, guarded, rules, beat, startMs, roundTripMs, known, layers);
    }

    /// <summary>
    /// The fate of a crack on a constellation: its onFail, but a destroy only on a risky one without a Jewel of
    /// Guardian.
    /// </summary>
    /// <param name="recipe">The constellation, or null.</param>
    /// <returns>The fate.</returns>
    public StarSmithOnFail FateOf(StarSmithRecipe? recipe)
    {
        var fail = recipe?.OnFail ?? StarSmithOnFail.IngredientsLost;
        return fail == StarSmithOnFail.ItemDestroyed && (!recipe!.Risky || this.guarded) ? StarSmithOnFail.IngredientsLost : fail;
    }

    /// <summary>
    /// Places a node, checked in the contract's order; a refused node changes nothing.
    /// </summary>
    /// <param name="point">The point, in box units.</param>
    /// <param name="serverTimeMs">The server time it arrived.</param>
    /// <returns>The answer.</returns>
    public StarSmithFigureAnswer Place(StarSmithPoint point, long serverTimeMs)
    {
        if (this.State != StarSmithForgeState.Drafting)
        {
            return this.Refuse(StarSmithFigureKind.Node, StarSmithFigureResult.NoAttempt);
        }

        if (this.closedAt >= 0 && this.figure.Count > this.closedAt + 1)
        {
            return this.Refuse(StarSmithFigureKind.Node, StarSmithFigureResult.FigureFull);
        }

        if (this.StrikesLeft <= 0)
        {
            return this.Refuse(StarSmithFigureKind.Node, StarSmithFigureResult.NoStrikesLeft);
        }

        var closing = this.figure.Count >= 3 && this.closedAt < 0 && point.DistanceTo(this.figure[0]) <= this.rules.CloseRadius;
        if (closing)
        {
            point = this.figure[0];
        }
        else if (this.figure.Concat(this.banked.SelectMany(b => b.Nodes)).Any(n => n.DistanceTo(point) < this.rules.NodeGap))
        {
            return this.Refuse(StarSmithFigureKind.Node, StarSmithFigureResult.TooNear);
        }

        var beatNumber = this.beat.BeatOf(this.startMs, serverTimeMs);
        var timing = beatNumber <= this.lastBeat ? StarSmithTiming.Late : this.beat.Judge(this.startMs, serverTimeMs, this.roundTripMs);
        this.lastBeat = Math.Max(this.lastBeat, beatNumber);
        var before = this.figure.ToList();
        var closedBefore = this.closedAt;
        this.figure.Add(point);
        this.figureTimings.Add(timing);
        this.timings.Add(timing);
        if (closing)
        {
            this.closedAt = this.figure.Count - 1;
        }

        var (turn, ratio) = StarSmithAccuracy.Measured(this.figure, this.figure.Count - 1);
        if (!closing && this.Faults.Any(f => StarSmithFaults.Hits(point, f, this.rules.FaultRadius)))
        {
            this.Crack(before, closedBefore, this.figureTimings.Take(before.Count).ToList());
            return new StarSmithFigureAnswer(StarSmithFigureKind.Node, StarSmithFigureResult.Cracked, timing, this.StrikesLeft, StarSmithFigureFlags.Fault, this.figure.Count, 0, null, null, turn, ratio, [], [], []);
        }

        var (match, measure) = this.Match(this.figure, this.closedAt, this.figureTimings);
        var guide = this.Guide();
        var flags = closing ? StarSmithFigureFlags.Closed : StarSmithFigureFlags.None;
        if (match is not null && this.known.Add(match.Number))
        {
            this.found.Add(match.Number);
            flags |= StarSmithFigureFlags.Found;
        }

        if (measure?.Extended == true)
        {
            flags |= StarSmithFigureFlags.Extended;
        }

        var hull = StarSmithHull.Of(this.figure);
        if (this.UsedHulls().Any(h => StarSmithHull.Overlaps(hull, h)))
        {
            flags |= StarSmithFigureFlags.Overlaps;
        }

        if (match is not null && this.Classify(hull, match.Number) >= 0)
        {
            flags |= StarSmithFigureFlags.Reseal;
        }

        var against = measure ?? guide.FirstOrDefault().Measure;
        var placements = this.figure.Select((_, i) => (this.figureTimings[i], against is null ? (int?)null : against.Accuracy[i])).ToList();
        return new StarSmithFigureAnswer(StarSmithFigureKind.Node, StarSmithFigureResult.Done, timing, this.StrikesLeft, flags, this.figure.Count, this.banked.Count, match, measure, turn, ratio, guide.Select(g => g.Recipe.Number).ToList(), placements, hull);
    }

    /// <summary>
    /// Banks the figure: it must match, its hull must not overlap a used one (but a reseal), and a slot must be free.
    /// </summary>
    /// <returns>The answer.</returns>
    public StarSmithFigureAnswer Stash()
    {
        if (this.State != StarSmithForgeState.Drafting)
        {
            return this.Refuse(StarSmithFigureKind.Stash, StarSmithFigureResult.NoAttempt);
        }

        var (result, bank) = this.TryBank();
        if (bank is null)
        {
            return this.Refuse(StarSmithFigureKind.Stash, result);
        }

        var flags = bank.Reseal >= 0 ? StarSmithFigureFlags.Reseal : StarSmithFigureFlags.None;
        if (bank.Measure.Extended)
        {
            flags |= StarSmithFigureFlags.Extended;
        }

        return new StarSmithFigureAnswer(StarSmithFigureKind.Stash, StarSmithFigureResult.Done, null, this.StrikesLeft, flags, 0, this.banked.Count, bank.Recipe, bank.Measure, 0, 0, [], [], bank.Hull);
    }

    /// <summary>
    /// Seals: banks the figure being drawn when it qualifies, then makes every banked figure a layer; with nothing
    /// banked it is the attunement.
    /// </summary>
    /// <returns>The result, or null when nothing is placed or the attempt is no longer drafting.</returns>
    public StarSmithForgeResult? Seal()
    {
        if (this.State != StarSmithForgeState.Drafting || this.timings.Count == 0)
        {
            return null;
        }

        if (this.figure.Count > 0)
        {
            this.TryBank();
        }

        var after = this.layers.ToList();
        var figures = new List<StarSmithSealedFigure>();
        foreach (var bank in this.banked)
        {
            var layer = new StarSmithLayer(bank.Recipe.Number, bank.Measure.Tier, (byte)bank.Recipe.Look, bank.Hull);
            if (bank.Reseal < 0)
            {
                after.Add(layer);
                figures.Add(new StarSmithSealedFigure(bank.Recipe, bank.Measure.Tier, bank.Measure.Score, StarSmithLayerChange.Added, after.Count - 1));
            }
            else if (bank.Measure.Tier > after[bank.Reseal].Tier)
            {
                after[bank.Reseal] = layer;
                figures.Add(new StarSmithSealedFigure(bank.Recipe, bank.Measure.Tier, bank.Measure.Score, StarSmithLayerChange.Raised, bank.Reseal));
            }
            else
            {
                figures.Add(new StarSmithSealedFigure(bank.Recipe, bank.Measure.Tier, bank.Measure.Score, StarSmithLayerChange.Kept, bank.Reseal));
            }
        }

        var attunement = this.banked.Count == 0;
        var own = attunement
            ? (this.timings.Average(StarSmithAccuracy.Weight) >= 400 ? StarSmithRarity.Uncommon : StarSmithRarity.Common)
            : (StarSmithRarity)this.banked.Max(b => (byte)b.Measure.Tier);
        var best = after.Count == 0 ? own : (StarSmithRarity)Math.Max((byte)own, after.Max(l => (byte)l.Tier));
        var boost = this.banked.Where(b => b.Recipe.Outcomes.Count > 0).OrderByDescending(b => b.Measure.Tier).FirstOrDefault();
        this.State = StarSmithForgeState.Sealed;
        this.Result = new StarSmithForgeResult(
            StarSmithForgeState.Sealed,
            figures,
            0,
            null,
            null,
            this.timings.Count,
            this.CleanPlacements(),
            attunement,
            best,
            after,
            boost?.Recipe,
            boost?.Measure.Tier ?? StarSmithSignTier.Faint);
        return this.Result;
    }

    /// <summary>
    /// Abandons the attempt (also the idle timeout): the banked figures and the ingredients are lost, the item stays.
    /// </summary>
    /// <returns>The result, or null when the attempt is no longer drafting.</returns>
    public StarSmithForgeResult? Abandon()
    {
        if (this.State != StarSmithForgeState.Drafting)
        {
            return null;
        }

        this.State = StarSmithForgeState.Abandoned;
        this.Result = new StarSmithForgeResult(StarSmithForgeState.Abandoned, [], this.banked.Count, null, StarSmithOnFail.IngredientsLost, this.timings.Count, this.CleanPlacements(), false, StarSmithRarity.Common, this.layers, null, StarSmithSignTier.Faint);
        return this.Result;
    }

    private StarSmithFigureAnswer Refuse(StarSmithFigureKind kind, StarSmithFigureResult result)
        => StarSmithFigureAnswer.Refused(kind, result, this.StrikesLeft, this.figure.Count, this.banked.Count);

    private int CleanPlacements() => this.timings.Count(t => t == StarSmithTiming.Clean);

    private IEnumerable<IReadOnlyList<StarSmithPoint>> UsedHulls() => this.layers.Select(l => l.Hull).Concat(this.banked.Select(b => b.Hull));

    /// <summary>
    /// The best match of a figure: the highest score, then the lowest number.
    /// </summary>
    private (StarSmithRecipe? Recipe, StarSmithMeasure? Measure) Match(IReadOnlyList<StarSmithPoint> points, int closed, IReadOnlyList<StarSmithTiming> figureTimings)
    {
        var best = this.recipes
            .Select(r => (Recipe: r, Measure: StarSmithAccuracy.Measure(points, closed, figureTimings, r.Geometry!)))
            .Where(m => m.Measure.Matches)
            .OrderByDescending(m => m.Measure.Score)
            .ThenBy(m => m.Recipe.Number)
            .FirstOrDefault();
        return best.Recipe is null ? (null, null) : best;
    }

    /// <summary>
    /// The known constellations the figure is a prefix of, by number, at most 8.
    /// </summary>
    private List<(StarSmithRecipe Recipe, StarSmithMeasure Measure)> Guide()
        => this.recipes
            .Where(r => this.known.Contains(r.Number))
            .Select(r => (Recipe: r, Measure: StarSmithAccuracy.Measure(this.figure, this.closedAt, this.figureTimings, r.Geometry!)))
            .Where(m => m.Measure.Prefix)
            .OrderBy(m => m.Recipe.Number)
            .Take(MaximumGuide)
            .ToList();

    /// <summary>
    /// Where a hull may go: -1 free, the index of the item layer it reseals, or -2 an overlap.
    /// </summary>
    private int Classify(IReadOnlyList<StarSmithPoint> hull, short number)
    {
        if (this.banked.Any(b => StarSmithHull.Overlaps(hull, b.Hull)))
        {
            return -2;
        }

        var over = Enumerable.Range(0, this.layers.Count).Where(i => StarSmithHull.Overlaps(hull, this.layers[i].Hull)).ToList();
        if (over.Count == 0)
        {
            return -1;
        }

        return over.Count == 1 && this.layers[over[0]].Number == number && this.banked.All(b => b.Reseal != over[0]) ? over[0] : -2;
    }

    private (StarSmithFigureResult Result, Banked? Bank) TryBank()
    {
        var (match, measure) = this.Match(this.figure, this.closedAt, this.figureTimings);
        if (match is null || measure is null)
        {
            return (StarSmithFigureResult.NoMatch, null);
        }

        var hull = StarSmithHull.Of(this.figure);
        var place = this.Classify(hull, match.Number);
        if (place == -2)
        {
            return (StarSmithFigureResult.Overlap, null);
        }

        if (place == -1 && this.FreeSlots <= 0)
        {
            return (StarSmithFigureResult.NoSlot, null);
        }

        var bank = new Banked(match, measure, hull, this.figure.ToList(), place);
        this.banked.Add(bank);
        this.figure.Clear();
        this.figureTimings.Clear();
        this.closedAt = -1;
        return (StarSmithFigureResult.Done, bank);
    }

    /// <summary>
    /// A crack: every banked figure is lost; the fate is the onFail of the constellation the figure was on (its match
    /// or a prefix, the longest, then the lowest number), else of the first banked figure.
    /// </summary>
    private void Crack(IReadOnlyList<StarSmithPoint> before, int closedBefore, IReadOnlyList<StarSmithTiming> timingsBefore)
    {
        var on = this.recipes
            .Select(r => (Recipe: r, Measure: StarSmithAccuracy.Measure(before, closedBefore, timingsBefore, r.Geometry!)))
            .Where(m => m.Measure.Matches || m.Measure.Prefix)
            .OrderByDescending(m => m.Recipe.Geometry!.Placements)
            .ThenBy(m => m.Recipe.Number)
            .Select(m => m.Recipe)
            .FirstOrDefault() ?? this.banked.FirstOrDefault()?.Recipe;
        this.State = StarSmithForgeState.Cracked;
        this.Result = new StarSmithForgeResult(StarSmithForgeState.Cracked, [], this.banked.Count, on, this.FateOf(on), this.timings.Count, this.CleanPlacements(), false, StarSmithRarity.Common, this.layers, null, StarSmithSignTier.Faint);
    }

    private sealed record Banked(StarSmithRecipe Recipe, StarSmithMeasure Measure, IReadOnlyList<StarSmithPoint> Hull, IReadOnlyList<StarSmithPoint> Nodes, int Reseal);
}
