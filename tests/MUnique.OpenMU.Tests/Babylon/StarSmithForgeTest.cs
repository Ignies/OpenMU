// <copyright file="StarSmithForgeTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The Constellation Forge by geometry (Readme.md "PR 7a contract"): free nodes on the beat, the live match, discovery,
/// fault lines, stash, seal, slots, overlap and reseal.
/// </summary>
[TestFixture]
public class StarSmithForgeTest
{
    private const int Interval = 800;

    private static readonly StarSmithBeat Beat = new(Interval, 90, 150);

    private static readonly List<StarSmithRecipe> Defaults = StarSmithingConfiguration.DefaultRecipes();

    private static StarSmithRecipe Fire => Defaults.Single(r => r.Number == 101);

    private static StarSmithRecipe Poison => Defaults.Single(r => r.Number == 102);

    private static StarSmithRecipe Gold => Defaults.Single(r => r.Number == 113);

    /// <summary>
    /// Fire drawn on the beat: the close is the fourth placement, the figure matches at Virulent, and an unknown
    /// constellation is found.
    /// </summary>
    [Test]
    public void FireOnTheBeatIsFoundAtVirulent()
    {
        var forge = Start();
        var answers = PlaceAll(forge, Draw(Fire, 0, 0.1, 0.3, 0.5));
        var last = answers[^1];
        Assert.Multiple(() =>
        {
            Assert.That(answers.Select(a => a.Result), Is.All.EqualTo(StarSmithFigureResult.Done));
            Assert.That(answers.Take(3).Select(a => a.Match), Is.All.Null);
            Assert.That(last.Flags, Is.EqualTo(StarSmithFigureFlags.Closed | StarSmithFigureFlags.Found));
            Assert.That(last.Match?.Number, Is.EqualTo(101));
            Assert.That(last.Measure!.Tier, Is.EqualTo(StarSmithSignTier.Virulent));
            Assert.That(last.Measure.Score, Is.EqualTo(1000));
            Assert.That(last.StrikesLeft, Is.EqualTo(6));
            Assert.That(last.FigurePlacements, Is.EqualTo(4));
            Assert.That(last.Placements.Select(p => p.Accuracy), Is.EqualTo(new int?[] { 1000, 1000, 1000, 1000 }));
            Assert.That(last.Turn, Is.EqualTo(120).Within(0.5));
            Assert.That(last.Ratio, Is.EqualTo(1).Within(0.01));
            Assert.That(forge.Found, Is.EqualTo(new short[] { 101 }));
        });
    }

    /// <summary>
    /// A known constellation is no find, and a known one guides the figure from its third placement; an unknown one
    /// never does.
    /// </summary>
    [Test]
    public void OnlyKnownConstellationsGuide()
    {
        var known = Start(known: new HashSet<short> { 101, 102 });
        var points = Draw(Poison, 20, 0.1, 0.3, 0.6);
        var answers = PlaceAll(known, points);
        var blind = PlaceAll(Start(), points.Take(3).ToList());
        Assert.Multiple(() =>
        {
            Assert.That(answers[1].Guide, Is.Empty);
            Assert.That(answers[2].Guide, Is.EqualTo(new short[] { 102 }));
            Assert.That(answers[2].Placements[2].Accuracy, Is.EqualTo(1000));
            Assert.That(answers[3].Match?.Number, Is.EqualTo(102));
            Assert.That(answers[3].Flags.HasFlag(StarSmithFigureFlags.Found), Is.False);
            Assert.That(blind[2].Guide, Is.Empty);
            Assert.That(blind[2].Placements[2].Accuracy, Is.Null);
        });
    }

    /// <summary>
    /// A shaky node lowers the tier by the contract's numbers; a late one more.
    /// </summary>
    [Test]
    public void TheBeatSetsTheTier()
    {
        var shaky = Start();
        var points = Draw(Poison, 0, 0.1, 0.3, 0.6);
        StarSmithFigureAnswer last = default!;
        for (var i = 0; i < points.Count; i++)
        {
            last = shaky.Place(points[i], ((i + 1) * Interval) + (i == 1 ? 150 : 0));
        }

        var late = Start();
        for (var i = 0; i < points.Count; i++)
        {
            last = i == 3 ? late.Place(points[i], ((i + 1) * Interval) + 400) : late.Place(points[i], (i + 1) * Interval);
        }

        Assert.Multiple(() =>
        {
            Assert.That(shaky.Figure.Count, Is.EqualTo(4));
            Assert.That(last.Measure!.Score, Is.EqualTo(750));
            Assert.That(last.Measure.Tier, Is.EqualTo(StarSmithSignTier.Lustre));
        });
        var again = Start();
        var answers = new List<StarSmithFigureAnswer>();
        for (var i = 0; i < points.Count; i++)
        {
            answers.Add(again.Place(points[i], ((i + 1) * Interval) + (i == 1 ? 150 : 0)));
        }

        Assert.That(answers[^1].Measure!.Score, Is.EqualTo(875));
        Assert.That(answers[^1].Measure!.Tier, Is.EqualTo(StarSmithSignTier.Venom));
    }

    /// <summary>
    /// The extension placed perfectly and clean is Chroma.
    /// </summary>
    [Test]
    public void ThePerfectExtensionIsChroma()
    {
        var forge = Start();
        var answers = PlaceAll(forge, Draw(Fire, 45, 0.08, 0.4, 0.5, extension: true));
        Assert.Multiple(() =>
        {
            Assert.That(answers[^1].Flags, Is.EqualTo(StarSmithFigureFlags.Extended));
            Assert.That(answers[^1].Measure!.Tier, Is.EqualTo(StarSmithSignTier.Chroma));
        });
    }

    /// <summary>
    /// Refusals change nothing and spend no strike: too near a node, a full figure, no strikes left.
    /// </summary>
    [Test]
    public void RefusedNodesChangeNothing()
    {
        var forge = Start(ingredients: 1);
        var first = forge.Place(new(30000, 30000), Interval);
        var near = forge.Place(new(30500, 30000), 2 * Interval);
        Assert.Multiple(() =>
        {
            Assert.That(first.Result, Is.EqualTo(StarSmithFigureResult.Done));
            Assert.That(near.Result, Is.EqualTo(StarSmithFigureResult.TooNear));
            Assert.That(near.StrikesLeft, Is.EqualTo(2));
            Assert.That(forge.Figure, Has.Count.EqualTo(1));
        });
        forge.Place(new(40000, 30000), 3 * Interval);
        forge.Place(new(40000, 40000), 4 * Interval);
        var none = forge.Place(new(50000, 50000), 5 * Interval);
        Assert.That(none.Result, Is.EqualTo(StarSmithFigureResult.NoStrikesLeft));

        var full = Start();
        PlaceAll(full, Draw(Fire, 0, 0.1, 0.3, 0.5, extension: true));
        Assert.That(full.Place(new(60000, 60000), 20 * Interval).Result, Is.EqualTo(StarSmithFigureResult.FigureFull));
    }

    /// <summary>
    /// A node on a fault cracks the attempt; the stashed figures are lost; the fate is the figure's constellation's.
    /// </summary>
    [Test]
    public void AFaultCracksAndLosesTheStash()
    {
        var risky = new StarSmithRecipe
        {
            Number = 7,
            Name = "Risky Fire",
            Targets = StarSmithTargetKind.Weapon,
            Geometry = new() { Nodes = 4, Turns = [90, 90], Lengths = [1, 2, 1], Extension = null },
            OnFail = StarSmithOnFail.ItemDestroyed,
            Risky = true,
        };
        var forge = Start(recipes: [Fire, risky], faults: 1, seed: 5);
        var fault = forge.Faults[0];
        PlaceAll(forge, Draw(Fire, 0, 0.06, 0.02, 0.06));
        Assert.That(forge.Stash().Result, Is.EqualTo(StarSmithFigureResult.Done));
        var mid = new StarSmithPoint((ushort)((fault.A.X + fault.B.X) / 2), (ushort)((fault.A.Y + fault.B.Y) / 2));
        var cracked = forge.Place(mid, 30 * Interval);
        Assert.Multiple(() =>
        {
            Assert.That(cracked.Result, Is.EqualTo(StarSmithFigureResult.Cracked));
            Assert.That(cracked.Flags.HasFlag(StarSmithFigureFlags.Fault), Is.True);
            Assert.That(forge.State, Is.EqualTo(StarSmithForgeState.Cracked));
            Assert.That(forge.Result!.Lost, Is.EqualTo(1));
            Assert.That(forge.Result.Fail, Is.EqualTo(StarSmithOnFail.IngredientsLost));
            Assert.That(forge.Result.FateRecipe?.Number, Is.EqualTo(101));
            Assert.That(forge.Seal(), Is.Null);
        });
    }

    /// <summary>
    /// A crack on the way to a risky constellation destroys the item, unless a Jewel of Guardian protects it.
    /// </summary>
    [TestCase(false, StarSmithOnFail.ItemDestroyed)]
    [TestCase(true, StarSmithOnFail.IngredientsLost)]
    public void ARiskyPrefixDecidesTheFate(bool guarded, StarSmithOnFail expected)
    {
        var risky = new StarSmithRecipe
        {
            Number = 7,
            Name = "Spine",
            Targets = StarSmithTargetKind.Weapon,
            Geometry = new() { Nodes = 5, Turns = [90, 90, -90], Lengths = [1, 1, 1, 1] },
            OnFail = StarSmithOnFail.ItemDestroyed,
            Risky = true,
        };
        var forge = Start(recipes: [risky], faults: 1, seed: 5, guarded: guarded);
        var fault = forge.Faults[0];
        var start = new StarSmithPoint((ushort)Math.Clamp(fault.A.X - 13000, 3000, 62000), (ushort)Math.Clamp(fault.A.Y - 13000, 3000, 62000));
        var points = StarSmithGeometryTest.Draw(risky.Geometry!, 0, 0.1, false, false, start.X / 65535.0, start.Y / 65535.0).Take(3).ToList();
        var answers = PlaceAll(forge, points);
        Assume.That(answers.Select(a => a.Result), Is.All.EqualTo(StarSmithFigureResult.Done));
        var mid = new StarSmithPoint((ushort)((fault.A.X + fault.B.X) / 2), (ushort)((fault.A.Y + fault.B.Y) / 2));
        Assume.That(forge.Place(mid, 10 * Interval).Result, Is.EqualTo(StarSmithFigureResult.Cracked));
        Assert.That(forge.Result!.Fail, Is.EqualTo(expected));
        Assert.That(forge.Result.FateRecipe?.Number, Is.EqualTo(7));
    }

    /// <summary>
    /// Stash banks a finished figure and the next node starts a new one; Seal seals every banked figure as a layer.
    /// </summary>
    [Test]
    public void StashAndSealMakeLayers()
    {
        var forge = Start(level: 21);
        PlaceAll(forge, Draw(Fire, 0, 0.08, 0.1, 0.3));
        var stash = forge.Stash();
        PlaceAll(forge, Draw(Gold, 0, 0.08, 0.6, 0.6), from: 10);
        var result = forge.Seal();
        Assert.Multiple(() =>
        {
            Assert.That(stash.Result, Is.EqualTo(StarSmithFigureResult.Done));
            Assert.That(stash.Kind, Is.EqualTo(StarSmithFigureKind.Stash));
            Assert.That(stash.Match?.Number, Is.EqualTo(101));
            Assert.That(stash.Banked, Is.EqualTo(1));
            Assert.That(stash.FigurePlacements, Is.EqualTo(0));
            Assert.That(stash.Hull, Has.Count.EqualTo(3));
            Assert.That(result!.State, Is.EqualTo(StarSmithForgeState.Sealed));
            Assert.That(result.Figures.Select(f => (f.Recipe.Number, f.Tier, f.Change, f.LayerIndex)), Is.EqualTo(new[]
            {
                ((short)101, StarSmithSignTier.Virulent, StarSmithLayerChange.Added, 0),
                ((short)113, StarSmithSignTier.Virulent, StarSmithLayerChange.Added, 1),
            }));
            Assert.That(result.Layers.Select(l => (l.Number, l.Look)), Is.EqualTo(new[] { ((short)101, (byte)1), ((short)113, (byte)13) }));
            Assert.That(result.Rarity, Is.EqualTo(StarSmithRarity.Legendary));
            Assert.That(result.Placements, Is.EqualTo(9));
            Assert.That(result.CleanPlacements, Is.EqualTo(9));
        });
    }

    /// <summary>
    /// A level 1 smith has one slot: a second stash is refused.
    /// </summary>
    [Test]
    public void SlotsLimitTheStash()
    {
        var forge = Start(level: 1);
        PlaceAll(forge, Draw(Fire, 0, 0.08, 0.1, 0.3));
        Assert.That(forge.Stash().Result, Is.EqualTo(StarSmithFigureResult.Done));
        PlaceAll(forge, Draw(Gold, 0, 0.08, 0.6, 0.6), from: 10);
        Assert.Multiple(() =>
        {
            Assert.That(forge.FreeSlots, Is.EqualTo(0));
            Assert.That(forge.Stash().Result, Is.EqualTo(StarSmithFigureResult.NoSlot));
        });
    }

    /// <summary>
    /// A hull over a banked one is refused; a stash of nothing that matches is refused.
    /// </summary>
    [Test]
    public void OverlapAndNoMatchAreRefused()
    {
        var forge = Start(level: 50);
        PlaceAll(forge, Draw(Gold, 0, 0.2, 0.3, 0.7));
        Assert.That(forge.Stash().Result, Is.EqualTo(StarSmithFigureResult.Done));
        var answers = PlaceAll(forge, Draw(Fire, 0, 0.1, 0.35, 0.6), from: 10);
        Assert.Multiple(() =>
        {
            Assert.That(answers[^1].Flags.HasFlag(StarSmithFigureFlags.Overlaps), Is.True);
            Assert.That(forge.Stash().Result, Is.EqualTo(StarSmithFigureResult.Overlap));
        });

        var empty = Start();
        empty.Place(new(10000, 10000), Interval);
        Assert.That(empty.Stash().Result, Is.EqualTo(StarSmithFigureResult.NoMatch));
    }

    /// <summary>
    /// Resealing the same constellation over its layer only ever raises the tier: a better one is raised and takes
    /// the new hull, a weaker one keeps the layer, and neither needs a slot.
    /// </summary>
    [Test]
    public void AResealOnlyRaises()
    {
        var old = new StarSmithLayer(101, StarSmithSignTier.Hue, 1, StarSmithHull.Of(Draw(Fire, 0, 0.1, 0.3, 0.5)));
        var better = Start(level: 1, layers: [old]);
        var answers = PlaceAll(better, Draw(Fire, 10, 0.1, 0.32, 0.5));
        var raised = better.Seal();

        var weaker = Start(level: 1, layers: [old with { Tier = StarSmithSignTier.Virulent }]);
        var points = Draw(Fire, 10, 0.1, 0.32, 0.5);
        for (var i = 0; i < points.Count; i++)
        {
            weaker.Place(points[i], ((i + 1) * Interval) + (i == 0 ? 150 : 0));
        }

        var kept = weaker.Seal();
        Assert.Multiple(() =>
        {
            Assert.That(answers[^1].Flags.HasFlag(StarSmithFigureFlags.Reseal), Is.True);
            Assert.That(raised!.Figures.Single().Change, Is.EqualTo(StarSmithLayerChange.Raised));
            Assert.That(raised.Layers.Single().Tier, Is.EqualTo(StarSmithSignTier.Virulent));
            Assert.That(raised.Layers.Single().Hull, Is.Not.EqualTo(old.Hull));
            Assert.That(kept!.Figures.Single().Change, Is.EqualTo(StarSmithLayerChange.Kept));
            Assert.That(kept.Figures.Single().Tier, Is.EqualTo(StarSmithSignTier.Venom));
            Assert.That(kept.Layers.Single().Tier, Is.EqualTo(StarSmithSignTier.Virulent));
            Assert.That(kept.Layers.Single().Hull, Is.EqualTo(old.Hull));
        });
    }

    /// <summary>
    /// A figure over a layer of another constellation is refused, even with a slot free.
    /// </summary>
    [Test]
    public void ALayerOfAnotherConstellationBlocks()
    {
        var old = new StarSmithLayer(113, StarSmithSignTier.Hue, 13, StarSmithHull.Of(Draw(Gold, 0, 0.2, 0.3, 0.7)));
        var forge = Start(level: 50, layers: [old]);
        PlaceAll(forge, Draw(Fire, 0, 0.1, 0.35, 0.6));
        Assert.That(forge.Stash().Result, Is.EqualTo(StarSmithFigureResult.Overlap));
    }

    /// <summary>
    /// A seal with nothing banked attunes, at most Uncommon; a seal before any node is refused.
    /// </summary>
    [Test]
    public void ASealWithoutAMatchAttunes()
    {
        Assert.That(Start().Seal(), Is.Null);
        var forge = Start();
        forge.Place(new(10000, 10000), Interval);
        forge.Place(new(20000, 10000), 2 * Interval);
        var result = forge.Seal();
        Assert.Multiple(() =>
        {
            Assert.That(result!.Attunement, Is.True);
            Assert.That(result.Figures, Is.Empty);
            Assert.That(result.Rarity, Is.EqualTo(StarSmithRarity.Uncommon));
        });
    }

    /// <summary>
    /// The seal banks the figure being drawn when it qualifies.
    /// </summary>
    [Test]
    public void TheSealTakesTheFigureBeingDrawn()
    {
        var forge = Start();
        PlaceAll(forge, Draw(Poison, 0, 0.1, 0.3, 0.5));
        var result = forge.Seal();
        Assert.Multiple(() =>
        {
            Assert.That(result!.Attunement, Is.False);
            Assert.That(result.Figures.Single().Recipe.Number, Is.EqualTo(102));
            Assert.That(result.Rarity, Is.EqualTo(StarSmithRarity.Legendary));
        });
    }

    /// <summary>
    /// The item's rarity after a seal is the best of the seal and every layer.
    /// </summary>
    [Test]
    public void TheRarityIsTheBestLayer()
    {
        var mythic = new StarSmithLayer(113, StarSmithSignTier.Chroma, 13, StarSmithHull.Of(Draw(Gold, 0, 0.1, 0.7, 0.8)));
        var forge = Start(level: 21, layers: [mythic]);
        var points = Draw(Poison, 0, 0.1, 0.1, 0.3);
        for (var i = 0; i < points.Count; i++)
        {
            forge.Place(points[i], ((i + 1) * Interval) + 400);
        }

        var result = forge.Seal();
        Assert.Multiple(() =>
        {
            Assert.That(result!.Figures.Single().Tier, Is.EqualTo(StarSmithSignTier.Faint));
            Assert.That(result.Rarity, Is.EqualTo(StarSmithRarity.Mythic));
        });
    }

    /// <summary>
    /// An abandon loses the stash and leaves the item.
    /// </summary>
    [Test]
    public void AnAbandonLosesTheStash()
    {
        var forge = Start();
        PlaceAll(forge, Draw(Fire, 0, 0.1, 0.3, 0.5));
        forge.Stash();
        var result = forge.Abandon();
        Assert.Multiple(() =>
        {
            Assert.That(result!.State, Is.EqualTo(StarSmithForgeState.Abandoned));
            Assert.That(result.Lost, Is.EqualTo(1));
            Assert.That(result.Fail, Is.EqualTo(StarSmithOnFail.IngredientsLost));
            Assert.That(forge.Abandon(), Is.Null);
        });
    }

    /// <summary>
    /// The risk is the worst fate among the attempt's constellations.
    /// </summary>
    [Test]
    public void TheRiskIsTheWorstFate()
    {
        var risky = new StarSmithRecipe { Number = 7, Geometry = Fire.Geometry, OnFail = StarSmithOnFail.ItemDestroyed, Risky = true };
        Assert.Multiple(() =>
        {
            Assert.That(Start(recipes: [Fire]).Risk, Is.EqualTo(StarSmithOnFail.IngredientsLost));
            Assert.That(Start().Risk, Is.EqualTo(StarSmithOnFail.ItemDestroyed), "Storm Spine is risky");
            Assert.That(Start(recipes: [risky]).Risk, Is.EqualTo(StarSmithOnFail.ItemDestroyed));
            Assert.That(Start(recipes: [risky], guarded: true).Risk, Is.EqualTo(StarSmithOnFail.IngredientsLost));
        });
    }

    /// <summary>
    /// The attempt replays from its seed and its actions to the same result.
    /// </summary>
    [Test]
    public void AnAttemptReplays()
    {
        var attempt = StarSmithForgeAttempt.Start(9, Defaults, new StarSmithTarget(StarSmithTargetKind.Weapon, 0, false), 21, 3, [], new StarSmithForgeRules(), Beat, 0, 0, new HashSet<short>(), [], 4, Guid.NewGuid());
        var points = Draw(Fire, 0, 0.06, 0.02, 0.06);
        for (var i = 0; i < points.Count; i++)
        {
            attempt.Place(points[i], (i + 1) * Interval);
        }

        attempt.Stash();
        var result = attempt.Seal();
        var replay = attempt.Replay();
        Assert.Multiple(() =>
        {
            Assert.That(replay.State, Is.EqualTo(attempt.Forge.State));
            Assert.That(replay.Result!.Figures.Select(f => (f.Recipe.Number, f.Tier)), Is.EqualTo(result!.Figures.Select(f => (f.Recipe.Number, f.Tier))));
            Assert.That(replay.Faults, Is.EqualTo(attempt.Forge.Faults));
        });
    }

    private static StarSmithForge Start(int level = 1, int ingredients = 8, IReadOnlyList<StarSmithRecipe>? recipes = null, ISet<short>? known = null, IReadOnlyList<StarSmithLayer>? layers = null, int faults = 0, long seed = 1, bool guarded = false)
    {
        var rules = new StarSmithForgeRules { FaultCount = faults };
        return StarSmithForge.Start(seed, recipes ?? Defaults, level, ingredients, guarded, rules, Beat, 0, 0, (IReadOnlySet<short>?)(known is null ? null : new HashSet<short>(known)) ?? new HashSet<short>(), layers ?? []);
    }

    private static List<StarSmithPoint> Draw(StarSmithRecipe recipe, double heading, double scale, double x, double y, bool extension = false)
    {
        var points = StarSmithGeometryTest.Draw(recipe.Geometry!, heading, scale, extension, false, x, y);
        if (recipe.Geometry!.Closed)
        {
            var at = recipe.Geometry.Placements - 1;
            points[at] = new StarSmithPoint((ushort)(points[at].X + 300), points[at].Y);
        }

        return points;
    }

    private static List<StarSmithFigureAnswer> PlaceAll(StarSmithForge forge, IReadOnlyList<StarSmithPoint> points, int from = 0)
        => points.Select((p, i) => forge.Place(p, (from + i + 1) * Interval)).ToList();
}
