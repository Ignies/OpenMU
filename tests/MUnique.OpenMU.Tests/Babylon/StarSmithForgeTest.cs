// <copyright file="StarSmithForgeTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// Constellation Forge (Hidden-Babylon docs/star_smithing.md section 10): the sky, the recipe shapes and their guidance,
/// the beat, and the forge's drafting, sealing and cracking.
/// </summary>
[TestFixture]
public class StarSmithForgeTest
{
    private static readonly StarSmithRecipe Ember = new()
    {
        Number = 1,
        Targets = StarSmithTargetKind.Weapon,
        Shape = new() { Stars = [0, 1, 2], Match = StarSmithShapeMatch.Extendable },
        OnFail = StarSmithOnFail.ItemDowngraded,
    };

    private static readonly StarSmithRecipe Ward = new()
    {
        Number = 3,
        Targets = StarSmithTargetKind.Weapon,
        Shape = new() { Stars = [3, 4, 5], Ordered = false },
    };

    private static readonly StarSmithRecipe Nova = new()
    {
        Number = 4,
        Targets = StarSmithTargetKind.Weapon,
        Shape = new() { Stars = [2, 3, 7] },
        OnFail = StarSmithOnFail.ItemDestroyed,
        Risky = true,
    };

    private static readonly StarSmithRecipe Glow = new()
    {
        Number = 6,
        Targets = StarSmithTargetKind.Weapon,
        Shape = new() { Stars = [1, 2, 7] },
        OnFail = StarSmithOnFail.ItemDestroyed,
    };

    private static readonly StarSmithRecipe SureLine = new()
    {
        Number = 10,
        Targets = StarSmithTargetKind.Weapon,
        BaseChance = 10_000,
        Shape = new() { Stars = [0, 1, 2, 3], Match = StarSmithShapeMatch.Extendable },
    };

    private static readonly StarSmithRecipe SureTriangle = new()
    {
        Number = 11,
        Targets = StarSmithTargetKind.Weapon,
        BaseChance = 10_000,
        Shape = new() { Stars = [4, 5, 6], Ordered = false },
    };

    private static readonly StarSmithRecipe Longshot = new()
    {
        Number = 12,
        Targets = StarSmithTargetKind.Weapon,
        BaseChance = 0,
        Shape = new() { Stars = [7, 8, 9] },
    };

    private static readonly StarSmithRecipe RingOnly = new()
    {
        Number = 13,
        Targets = StarSmithTargetKind.Ring,
        BaseChance = 10_000,
        Shape = new() { Stars = [10, 11] },
    };

    private static readonly StarSmithRecipe[] DealBook = [SureLine, SureTriangle, Longshot, RingOnly];

    private static readonly StarSmithTarget Sword = new(StarSmithTargetKind.Weapon, Level: 9, Excellent: false);

    private static readonly (byte Group, short Number)[] NoCatalysts = [];

    private static readonly StarSmithBeat Beat = new(IntervalMs: 1000, ToleranceMs: 100, RoundTripCapMs: 150);

    /// <summary>
    /// The same seed deals the same sky: stars, hints, links and strikes; another seed deals another.
    /// </summary>
    [Test]
    public void ADealIsDeterministicForASeed()
    {
        var first = Deal(42, smithLevel: 30);
        var again = Deal(42, smithLevel: 30);
        var other = Deal(43, smithLevel: 30);

        Assert.That(first.Stars, Has.Count.EqualTo(12));
        Assert.That(again.Stars, Is.EqualTo(first.Stars));
        Assert.That(again.Hints, Is.EqualTo(first.Hints));
        Assert.That(again.Links, Is.EqualTo(first.Links));
        Assert.That(other.Stars, Is.Not.EqualTo(first.Stars));
        Assert.That(first.Stars.All(s => s.X is >= 0 and <= 1 && s.Y is >= 0 and <= 1), Is.True, "inside the item's box");
        Assert.That(first.Stars.All(s => s.Value is >= 1 and <= 5), Is.True);
        Assert.That(first.Recipes, Is.EquivalentTo(new[] { SureLine, SureTriangle, Longshot }), "a ring recipe does not fit a sword");
    }

    /// <summary>
    /// Every edge of a recipe's shape can be drawn, the ingredients set how many stars may be struck, and voids never
    /// sit on the shape of a recipe whose roll succeeded; stars on no shape hold the exploration voids.
    /// </summary>
    [Test]
    public void TheDealKeepsShapesDrawable()
    {
        for (var seed = 1; seed <= 20; seed++)
        {
            var sky = Deal(seed, smithLevel: 1);
            foreach (var recipe in sky.Recipes)
            {
                var stars = recipe.Shape!.Stars;
                for (var i = 1; i < stars.Count; i++)
                {
                    Assert.That(sky.IsLinked(stars[i - 1], stars[i]), Is.True, $"seed {seed}: {recipe.Number} edge {i}");
                }
            }

            Assert.That(SureLine.Shape!.Stars.Concat(SureTriangle.Shape!.Stars).Any(s => sky.Stars[s].IsVoid), Is.False, $"seed {seed}: a sure shape");
            Assert.That(sky.Stars[10].IsVoid && sky.Stars[11].IsVoid, Is.True, $"seed {seed}: the exploration voids");
        }

        Assert.That(Deal(1, smithLevel: 1, ingredientCount: 1).Strikes, Is.EqualTo(3));
        Assert.That(Deal(1, smithLevel: 1, ingredientCount: 4).Strikes, Is.EqualTo(6));
        Assert.That(Deal(1, smithLevel: 1, ingredientCount: 10).Strikes, Is.EqualTo(12));
    }

    /// <summary>
    /// A higher smith level shows more hints, and clearer ones; hints are edges of the recipes' shapes.
    /// </summary>
    [Test]
    public void HintCountGrowsWithLevel()
    {
        var novice = Deal(7, smithLevel: 1);
        var master = Deal(7, smithLevel: 100);
        var edges = DealBook.Take(3).SelectMany(r => r.Shape!.Stars.Zip(r.Shape.Stars.Skip(1), (a, b) => (Math.Min(a, b), Math.Max(a, b)))).ToHashSet();

        Assert.That(novice.Hints, Has.Count.EqualTo(1));
        Assert.That(Deal(7, smithLevel: 50).Hints, Has.Count.EqualTo(3));
        Assert.That(master.Hints, Has.Count.EqualTo(6));
        Assert.That(master.Hints.All(h => edges.Contains((Math.Min(h.From, h.To), Math.Max(h.From, h.To)))), Is.True);
        Assert.That(master.Hints[0].Clarity, Is.GreaterThan(novice.Hints[0].Clarity));
    }

    /// <summary>
    /// A Jewel of Guardian among the catalysts reveals exactly one void before the first strike.
    /// </summary>
    [Test]
    public void GuardianRevealsAVoid()
    {
        var guarded = StarSmithSky.Deal(5, DealBook, Sword, smithLevel: 1, ingredientCount: 4, catalysts: [(14, 31)]);
        var plain = Deal(5, smithLevel: 1);

        Assert.That(guarded.Guarded, Is.True);
        Assert.That(guarded.Stars.Count(s => s.Revealed), Is.EqualTo(1));
        Assert.That(guarded.Stars.Single(s => s.Revealed).IsVoid, Is.True);
        Assert.That(plain.Guarded, Is.False);
        Assert.That(plain.Stars.Count(s => s.Revealed), Is.EqualTo(0));
    }

    /// <summary>
    /// Guidance: the recipes the drawn path is still a prefix of; an unordered shape may start from either end; only
    /// known recipes glow when the known set is given.
    /// </summary>
    [Test]
    public void GuidanceFollowsPrefixes()
    {
        var sky = HandSky();
        Assert.That(sky.Guidance([]), Is.EquivalentTo(new[] { Ember, Ward, Nova, Glow }));
        Assert.That(sky.Guidance([0]), Is.EqualTo(new[] { Ember }));
        Assert.That(sky.Guidance([0, 1, 2]), Is.EqualTo(new[] { Ember }));
        Assert.That(sky.Guidance([0, 1, 2, 3]), Is.Empty, "past the shape is blind");
        Assert.That(sky.Guidance([0, 2]), Is.Empty);
        Assert.That(sky.Guidance([1, 2]), Is.EqualTo(new[] { Glow }));
        Assert.That(sky.Guidance([5, 4]), Is.EqualTo(new[] { Ward }), "unordered, from the other end");
        Assert.That(sky.Guidance([4]), Is.Empty, "not an end of the shape");
        Assert.That(sky.Guidance([], new HashSet<short> { 1, 3 }), Is.EquivalentTo(new[] { Ember, Ward }));
    }

    /// <summary>
    /// A strike within the tolerance of a beat is clean, within twice that shaky, beyond it late; the round trip widens
    /// the window up to its cap.
    /// </summary>
    [Test]
    public void BeatJudgementWidensByTheRoundTrip()
    {
        Assert.That(Beat.Window(0), Is.EqualTo(100));
        Assert.That(Beat.Window(80), Is.EqualTo(180));
        Assert.That(Beat.Window(500), Is.EqualTo(250), "capped");

        Assert.That(Beat.Judge(startMs: 0, timeMs: 1050, roundTripMs: 0), Is.EqualTo(StarSmithTiming.Clean));
        Assert.That(Beat.Judge(0, 950, 0), Is.EqualTo(StarSmithTiming.Clean), "a little early");
        Assert.That(Beat.Judge(0, 1150, 0), Is.EqualTo(StarSmithTiming.Shaky));
        Assert.That(Beat.Judge(0, 1150, 80), Is.EqualTo(StarSmithTiming.Clean), "the round trip widens it");
        Assert.That(Beat.Judge(0, 1300, 500), Is.EqualTo(StarSmithTiming.Shaky), "but only up to the cap");
        Assert.That(Beat.Judge(0, 1450, 0), Is.EqualTo(StarSmithTiming.Late));
        Assert.That(Beat.Judge(5000, 7020, 0), Is.EqualTo(StarSmithTiming.Clean), "beats count from the start");
        Assert.That(Beat.BeatOf(0, 1450), Is.EqualTo(1));
        Assert.That(Beat.BeatOf(0, 1550), Is.EqualTo(2));

        var forge = StarSmithForge.Start(HandSky(), Beat, startMs: 0, roundTripMs: 0, smithLevel: 1, known: new HashSet<short>());
        Assert.That(forge.Strike(0, 1000).Timing, Is.EqualTo(StarSmithTiming.Clean));
        Assert.That(forge.Strike(1, 1050).Timing, Is.EqualTo(StarSmithTiming.Late), "one strike a beat");
        Assert.That(forge.Strike(2, 3000).Timing, Is.EqualTo(StarSmithTiming.Clean));
    }

    /// <summary>
    /// A strike must pick a star of the sky, unused, linked to the last one, with strikes left, while drafting; a refused
    /// strike changes nothing.
    /// </summary>
    [Test]
    public void IllegalStrikesAreRefused()
    {
        var forge = Start(HandSky());
        Assert.That(forge.Strike(99, 1000).Result, Is.EqualTo(StarSmithStrikeResult.NoSuchStar));
        var first = forge.Strike(0, 1000);
        Assert.That(first.Result, Is.EqualTo(StarSmithStrikeResult.Linked));
        Assert.That(first.Star.Element, Is.EqualTo(StarSmithElement.Fire), "the strike reveals the star");
        Assert.That(forge.Strike(0, 2000).Result, Is.EqualTo(StarSmithStrikeResult.StarUsed));
        Assert.That(forge.Strike(5, 2000).Result, Is.EqualTo(StarSmithStrikeResult.NotAdjacent));
        Assert.That(forge.Path, Is.EqualTo(new[] { 0 }));

        forge.Strike(1, 2000);
        forge.Strike(2, 3000);
        forge.Strike(3, 4000);
        forge.Strike(4, 5000);
        Assert.That(forge.StrikesLeft, Is.EqualTo(0));
        Assert.That(forge.Strike(5, 6000).Result, Is.EqualTo(StarSmithStrikeResult.NoStrikesLeft));

        Assert.That(forge.Seal(), Is.Not.Null);
        Assert.That(forge.State, Is.EqualTo(StarSmithForgeState.Sealed));
        Assert.That(forge.Strike(5, 7000).Result, Is.EqualTo(StarSmithStrikeResult.NotDrafting));
        Assert.That(forge.Seal(), Is.Null, "sealed once");
        Assert.That(Start(HandSky()).Seal(), Is.Null, "nothing drawn");
    }

    /// <summary>
    /// A void cracks the attempt; the recipe being followed decides the item's fate by its onFail, a destroy only on a
    /// risky recipe and never with a Jewel of Guardian.
    /// </summary>
    [Test]
    public void AVoidCracksAndAppliesOnFail()
    {
        Assert.That(Crack(HandSky(), 2, 3, 7), Is.EqualTo(StarSmithOnFail.ItemDestroyed), "risky Nova");
        Assert.That(Crack(HandSky(guarded: true), 2, 3, 7), Is.EqualTo(StarSmithOnFail.IngredientsLost), "the Guardian protects");
        Assert.That(Crack(HandSky(), 1, 2, 7), Is.EqualTo(StarSmithOnFail.IngredientsLost), "Glow is not risky");
        Assert.That(Crack(HandSky(), 0, 1, 2, 7), Is.EqualTo(StarSmithOnFail.ItemDowngraded), "extending Ember");
        Assert.That(Crack(HandSky(), 3, 7), Is.EqualTo(StarSmithOnFail.IngredientsLost), "on no shape");

        var forge = Start(HandSky());
        forge.Strike(2, 1000);
        forge.Strike(3, 2000);
        var crack = forge.Strike(7, 3000);
        Assert.That(crack.Result, Is.EqualTo(StarSmithStrikeResult.Cracked));
        Assert.That(crack.Star.IsVoid, Is.True);
        Assert.That(forge.State, Is.EqualTo(StarSmithForgeState.Cracked));
        Assert.That(forge.Result!.Recipe, Is.SameAs(Nova));
        Assert.That(forge.Result.FirstCompletion, Is.False);
        Assert.That(forge.Seal(), Is.Null);
    }

    /// <summary>
    /// The share of clean strikes plus the smith level sets the rarity tier; the attunement fallback stops at Uncommon.
    /// </summary>
    [Test]
    public void RarityFromTheCleanShare()
    {
        Assert.That(StarSmithForge.Rarity(clean: 0, strikes: 4, smithLevel: 100), Is.EqualTo(StarSmithRarity.Common));
        Assert.That(StarSmithForge.Rarity(1, 4, 1), Is.EqualTo(StarSmithRarity.Common));
        Assert.That(StarSmithForge.Rarity(2, 4, 1), Is.EqualTo(StarSmithRarity.Rare));
        Assert.That(StarSmithForge.Rarity(2, 4, 100), Is.EqualTo(StarSmithRarity.Epic));
        Assert.That(StarSmithForge.Rarity(4, 4, 1), Is.EqualTo(StarSmithRarity.Legendary));
        Assert.That(StarSmithForge.Rarity(4, 4, 100), Is.EqualTo(StarSmithRarity.Mythic));

        Assert.That(Seal(HandSky(), 1, [1000, 2000, 3000], 0, 1, 2).Rarity, Is.EqualTo(StarSmithRarity.Legendary));
        Assert.That(Seal(HandSky(), 1, [1450, 2450, 3450], 0, 1, 2).Rarity, Is.EqualTo(StarSmithRarity.Common));
        var attuned = Seal(HandSky(), 1, [1000, 2000], 0, 1);
        Assert.That(attuned.Recipe, Is.Null, "no shape completed");
        Assert.That(attuned.Rarity, Is.EqualTo(StarSmithRarity.Uncommon));
    }

    /// <summary>
    /// The sealed result: the recipe the shape completes, the element mix by star value, and a bigger effect for each
    /// link that extends an extendable shape; an exact shape drawn further is no longer that recipe.
    /// </summary>
    [Test]
    public void ExtendingAShapeGivesABiggerEffect()
    {
        var plain = Seal(HandSky(), 50, [1000, 2000, 3000], 0, 1, 2);
        var longer = Seal(HandSky(), 50, [1000, 2000, 3000, 4000], 0, 1, 2, 3);

        Assert.That(plain.State, Is.EqualTo(StarSmithForgeState.Sealed));
        Assert.That(plain.Recipe, Is.SameAs(Ember));
        Assert.That(plain.Extension, Is.EqualTo(0));
        Assert.That(plain.ElementMix[StarSmithElement.Fire], Is.EqualTo(5));
        Assert.That(plain.ElementMix[StarSmithElement.Frost], Is.EqualTo(4));
        Assert.That(plain.Dominant, Is.EqualTo(StarSmithElement.Fire));
        Assert.That(plain.MagnitudeFactor, Is.EqualTo(StarSmithRecipes.Magnitude(1, 50)).Within(1e-9));
        Assert.That(plain.Magnitude(10), Is.EqualTo(StarSmithRecipes.Magnitude(10, 50)).Within(1e-9));
        Assert.That(plain.Fail, Is.Null);

        Assert.That(longer.Recipe, Is.SameAs(Ember));
        Assert.That(longer.Extension, Is.EqualTo(1));
        Assert.That(longer.MagnitudeFactor, Is.EqualTo(StarSmithRecipes.Magnitude(1.1, 50)).Within(1e-9));
        Assert.That(longer.MagnitudeFactor, Is.GreaterThan(plain.MagnitudeFactor));

        Assert.That(Seal(HandSky(), 50, [1000, 2000, 3000], 5, 4, 3).Recipe, Is.SameAs(Ward), "unordered, backwards");
        Assert.That(Seal(HandSky(), 50, [1000, 2000, 3000, 4000], 3, 4, 5, 6).Recipe, Is.Null, "an exact shape does not extend");
    }

    /// <summary>
    /// The first completion of a recipe the smith has not made yet is flagged for the Guidebook.
    /// </summary>
    [Test]
    public void TheFirstCompletionIsFlagged()
    {
        Assert.That(Seal(HandSky(), 50, [1000, 2000, 3000], 0, 1, 2).FirstCompletion, Is.True);

        var known = StarSmithForge.Start(HandSky(), Beat, 0, 0, smithLevel: 50, known: new HashSet<short> { 1 });
        known.Strike(0, 1000);
        known.Strike(1, 2000);
        known.Strike(2, 3000);
        Assert.That(known.Seal()!.FirstCompletion, Is.False);
        Assert.That(Seal(HandSky(), 50, [1000, 2000], 0, 1).FirstCompletion, Is.False, "the attunement is no Guidebook recipe");
    }

    private static StarSmithSky Deal(long seed, int smithLevel, int ingredientCount = 4)
        => StarSmithSky.Deal(seed, DealBook, Sword, smithLevel, ingredientCount, NoCatalysts);

    private static StarSmithSky HandSky(bool guarded = false)
    {
        StarSmithStar[] stars =
        [
            new(0.1, 0.1, StarSmithElement.Fire, 3, false),
            new(0.2, 0.2, StarSmithElement.Fire, 2, false),
            new(0.3, 0.3, StarSmithElement.Frost, 4, false),
            new(0.4, 0.4, StarSmithElement.Storm, 1, false),
            new(0.5, 0.5, StarSmithElement.Shadow, 2, false),
            new(0.6, 0.6, StarSmithElement.Light, 5, false),
            new(0.7, 0.7, StarSmithElement.Fire, 1, false),
            new(0.3, 0.6, StarSmithElement.Shadow, 1, true),
        ];
        (int A, int B)[] links = [(0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6), (2, 7), (3, 7)];
        return new StarSmithSky(stars, links, [], strikes: 5, recipes: [Ember, Ward, Nova, Glow], guarded);
    }

    private static StarSmithForge Start(StarSmithSky sky)
        => StarSmithForge.Start(sky, Beat, startMs: 0, roundTripMs: 0, smithLevel: 50, known: new HashSet<short>());

    private static StarSmithOnFail? Crack(StarSmithSky sky, params int[] path)
    {
        var forge = Start(sky);
        for (var i = 0; i < path.Length; i++)
        {
            forge.Strike(path[i], 1000 * (i + 1));
        }

        Assert.That(forge.State, Is.EqualTo(StarSmithForgeState.Cracked));
        return forge.Result!.Fail;
    }

    private static StarSmithForgeResult Seal(StarSmithSky sky, int smithLevel, long[] times, params int[] path)
    {
        var forge = StarSmithForge.Start(sky, Beat, startMs: 0, roundTripMs: 0, smithLevel, known: new HashSet<short>());
        for (var i = 0; i < path.Length; i++)
        {
            Assert.That(forge.Strike(path[i], times[i]).Result, Is.EqualTo(StarSmithStrikeResult.Linked));
        }

        return forge.Seal()!;
    }
}
