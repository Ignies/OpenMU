// <copyright file="StarSmithForge.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// One Constellation Forge attempt (section 10): the smith strikes stars on the beat, then seals, or a void cracks it.
/// Pure and deterministic: the same sky, beat and strikes give the same result.
/// </summary>
public sealed class StarSmithForge
{
    private readonly StarSmithSky sky;

    private readonly StarSmithBeat beat;

    private readonly long startMs;

    private readonly int roundTripMs;

    private readonly int smithLevel;

    private readonly IReadOnlySet<short> known;

    private readonly List<int> path = [];

    private readonly List<StarSmithTiming> timings = [];

    private readonly bool[] used;

    private long lastBeat;

    private StarSmithForge(StarSmithSky sky, StarSmithBeat beat, long startMs, int roundTripMs, int smithLevel, IReadOnlySet<short> known)
    {
        this.sky = sky;
        this.beat = beat;
        this.startMs = startMs;
        this.roundTripMs = roundTripMs;
        this.smithLevel = Math.Clamp(smithLevel, 1, StarSmithLevels.Maximum);
        this.known = known;
        this.used = new bool[sky.Stars.Count];
    }

    /// <summary>Gets where the attempt stands.</summary>
    public StarSmithForgeState State { get; private set; }

    /// <summary>Gets the struck stars, in order.</summary>
    public IReadOnlyList<int> Path => this.path;

    /// <summary>Gets how each strike met the beat, in order.</summary>
    public IReadOnlyList<StarSmithTiming> Timings => this.timings;

    /// <summary>Gets how many stars may still be struck.</summary>
    public int StrikesLeft => Math.Max(0, this.sky.Strikes - this.path.Count);

    /// <summary>Gets the result once sealed or cracked.</summary>
    public StarSmithForgeResult? Result { get; private set; }

    /// <summary>
    /// Starts an attempt.
    /// </summary>
    /// <param name="sky">The dealt sky.</param>
    /// <param name="beat">The beat.</param>
    /// <param name="startMs">The server time it starts.</param>
    /// <param name="roundTripMs">The client's measured round trip.</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <param name="known">The recipes in the smith's Guidebook.</param>
    /// <returns>The attempt, drafting.</returns>
    public static StarSmithForge Start(StarSmithSky sky, StarSmithBeat beat, long startMs, int roundTripMs, int smithLevel, IReadOnlySet<short> known)
    {
        ArgumentNullException.ThrowIfNull(sky);
        ArgumentNullException.ThrowIfNull(beat);
        ArgumentNullException.ThrowIfNull(known);
        return new StarSmithForge(sky, beat, startMs, roundTripMs, smithLevel, known);
    }

    /// <summary>
    /// How far a sealed shape got, in permille: the struck stars over the shape's length; an extendable shape counts
    /// its extensions up to its cap. The attunement (no shape) counts 1,000.
    /// </summary>
    /// <param name="shape">The completed shape, or null.</param>
    /// <param name="strikes">All strikes.</param>
    /// <returns>The progress.</returns>
    public static int Progress(StarSmithShape? shape, int strikes)
    {
        if (shape is null || shape.Stars.Count == 0)
        {
            return 1000;
        }

        var length = shape.Stars.Count;
        var extensions = shape.Match == StarSmithShapeMatch.Extendable ? Math.Max(0, shape.MaximumExtension) : 0;
        return Math.Clamp(strikes, 0, length + extensions) * 1000 / length;
    }

    /// <summary>
    /// The rarity tier of a result, which rewards pushing your luck: the clean share times the progress (both in
    /// permille), plus 250 * level / 100. Common below 300, Uncommon below 500, Rare below 800, Epic below 1,300,
    /// Legendary below 2,000, Mythic from there. So a full shape struck clean is Epic, and Mythic needs the extensions.
    /// </summary>
    /// <param name="clean">The clean strikes.</param>
    /// <param name="strikes">All strikes.</param>
    /// <param name="progress">How far the shape got, in permille (see <see cref="Progress"/>).</param>
    /// <param name="smithLevel">The smith's level.</param>
    /// <returns>The tier.</returns>
    public static StarSmithRarity Rarity(int clean, int strikes, int progress, int smithLevel)
    {
        if (strikes <= 0)
        {
            return StarSmithRarity.Common;
        }

        var share = (long)Math.Clamp(clean, 0, strikes) * Math.Max(0, progress) / strikes;
        var score = share + (250 * Math.Clamp(smithLevel, 0, StarSmithLevels.Maximum) / StarSmithLevels.Maximum);
        return score switch
        {
            < 300 => StarSmithRarity.Common,
            < 500 => StarSmithRarity.Uncommon,
            < 800 => StarSmithRarity.Rare,
            < 1300 => StarSmithRarity.Epic,
            < 2000 => StarSmithRarity.Legendary,
            _ => StarSmithRarity.Mythic,
        };
    }

    /// <summary>
    /// Strikes a star. Checked in this order: drafting, the star exists, strikes are left, the star is unused, it may be
    /// linked to the last struck star. A refused strike changes nothing. Only one strike counts on each beat: a second
    /// one on the same beat is late.
    /// </summary>
    /// <param name="star">The star's index in the sky.</param>
    /// <param name="serverTimeMs">The server time the strike arrived.</param>
    /// <returns>The answer.</returns>
    public StarSmithStrike Strike(int star, long serverTimeMs)
    {
        var refusal = this.Refusal(star);
        if (refusal is { } result)
        {
            return new StarSmithStrike(result, StarSmithTiming.Late, default);
        }

        var beatNumber = this.beat.BeatOf(this.startMs, serverTimeMs);
        var timing = beatNumber <= this.lastBeat ? StarSmithTiming.Late : this.beat.Judge(this.startMs, serverTimeMs, this.roundTripMs);
        this.lastBeat = Math.Max(this.lastBeat, beatNumber);
        this.path.Add(star);
        this.used[star] = true;
        this.timings.Add(timing);

        var struck = this.sky.Stars[star];
        if (!struck.IsVoid)
        {
            return new StarSmithStrike(StarSmithStrikeResult.Linked, timing, struck);
        }

        this.State = StarSmithForgeState.Cracked;
        this.Result = this.Crack();
        return new StarSmithStrike(StarSmithStrikeResult.Cracked, timing, struck);
    }

    /// <summary>
    /// Seals the drawn constellation and keeps its result.
    /// </summary>
    /// <returns>The result, or null when nothing is drawn or the attempt is no longer drafting.</returns>
    public StarSmithForgeResult? Seal()
    {
        if (this.State != StarSmithForgeState.Drafting || this.path.Count == 0)
        {
            return null;
        }

        var (recipe, extension) = this.sky.Completed(this.path);
        var counted = recipe is null ? 0 : Math.Clamp(extension, 0, Math.Max(0, recipe.Shape!.MaximumExtension));
        var multiplier = recipe is null ? 1.0 : 1 + (counted * recipe.Shape!.ExtensionPercent / 100.0);
        var clean = this.CleanStrikes();
        var rarity = Rarity(clean, this.path.Count, Progress(recipe?.Shape, this.path.Count), this.smithLevel);
        if (recipe is null && rarity > StarSmithRarity.Uncommon)
        {
            rarity = StarSmithRarity.Uncommon;
        }

        var (mix, dominant) = this.Mix();
        this.State = StarSmithForgeState.Sealed;
        this.Result = new StarSmithForgeResult(
            StarSmithForgeState.Sealed,
            recipe,
            this.path.ToArray(),
            mix,
            dominant,
            clean,
            rarity,
            counted,
            StarSmithRecipes.Magnitude(multiplier, this.smithLevel),
            recipe is not null && !this.known.Contains(recipe.Number),
            null);
        return this.Result;
    }

    /// <summary>
    /// The guidance for the path so far: the smith's known recipes it is still a prefix of.
    /// </summary>
    /// <returns>The recipes.</returns>
    public IReadOnlyList<StarSmithRecipe> Guidance() => this.sky.Guidance(this.path, this.known);

    private StarSmithStrikeResult? Refusal(int star)
    {
        if (this.State != StarSmithForgeState.Drafting)
        {
            return StarSmithStrikeResult.NotDrafting;
        }

        if (star < 0 || star >= this.sky.Stars.Count)
        {
            return StarSmithStrikeResult.NoSuchStar;
        }

        if (this.StrikesLeft <= 0)
        {
            return StarSmithStrikeResult.NoStrikesLeft;
        }

        if (this.used[star])
        {
            return StarSmithStrikeResult.StarUsed;
        }

        if (this.path.Count > 0 && !this.sky.IsLinked(this.path[^1], star))
        {
            return StarSmithStrikeResult.NotAdjacent;
        }

        return null;
    }

    /// <summary>
    /// The result of a crack: the recipe the path was on decides the item's fate by its onFail; a destroy only happens
    /// on a risky recipe without a Jewel of Guardian, otherwise the ingredients are lost. Off every shape (the
    /// attunement), the ingredients are lost.
    /// </summary>
    private StarSmithForgeResult Crack()
    {
        var recipe = this.sky.Followed(this.path);
        var fail = recipe?.OnFail ?? StarSmithOnFail.IngredientsLost;
        if (fail == StarSmithOnFail.ItemDestroyed && (!recipe!.Risky || this.sky.Guarded))
        {
            fail = StarSmithOnFail.IngredientsLost;
        }

        var (mix, dominant) = this.Mix();
        return new StarSmithForgeResult(
            StarSmithForgeState.Cracked,
            recipe,
            this.path.ToArray(),
            mix,
            dominant,
            this.CleanStrikes(),
            StarSmithRarity.Common,
            0,
            0,
            false,
            fail);
    }

    private int CleanStrikes() => this.timings.Count(t => t == StarSmithTiming.Clean);

    private (IReadOnlyDictionary<StarSmithElement, int> Mix, StarSmithElement? Dominant) Mix()
    {
        var mix = Enum.GetValues<StarSmithElement>().ToDictionary(e => e, _ => 0);
        foreach (var star in this.path.Select(s => this.sky.Stars[s]).Where(s => !s.IsVoid))
        {
            mix[star.Element] += star.Value;
        }

        var top = mix.OrderByDescending(m => m.Value).ThenBy(m => m.Key).First();
        return (mix, top.Value > 0 ? top.Key : (StarSmithElement?)null);
    }
}
