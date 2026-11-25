// <copyright file="StarSmithGeometryTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// PR 7a (Readme.md "PR 7a contract"): a constellation is its geometry; the accuracy curve, the score and the tier,
/// matching by turns and length ratios (any rotation and size, never mirrored), the hull, its overlap, the fault lines
/// and the slots by level.
/// </summary>
[TestFixture]
public class StarSmithGeometryTest
{
    private static readonly StarSmithGeometry Fire = new()
    {
        Nodes = 3,
        Closed = true,
        Turns = [120, 120],
        Lengths = [1, 1, 1],
        Extension = new() { Turn = -90, Length = 0.5 },
    };

    private static readonly StarSmithGeometry Poison = new()
    {
        Nodes = 4,
        Turns = [60, -60],
        Lengths = [1, 1, 1],
        Extension = new() { Turn = 90, Length = 0.5 },
    };

    /// <summary>
    /// The accuracy curve: perfect within its perfect limit, nothing from its miss, a cosine between.
    /// </summary>
    [TestCase(0, 1000)]
    [TestCase(2, 1000)]
    [TestCase(5, 874)]
    [TestCase(8.5, 500)]
    [TestCase(10, 323)]
    [TestCase(15, 0)]
    [TestCase(40, 0)]
    public void TheAngleCurveIsACosineBetweenPerfectAndMiss(double error, int expected)
    {
        Assert.That(StarSmithAccuracy.Curve(error, 2, 15), Is.EqualTo(expected));
    }

    /// <summary>
    /// The length curve: 2% perfect, 20% a miss.
    /// </summary>
    [TestCase(0.02, 1000)]
    [TestCase(0.05, 933)]
    [TestCase(0.11, 500)]
    [TestCase(0.2, 0)]
    public void TheLengthCurveIsACosineBetweenPerfectAndMiss(double error, int expected)
    {
        Assert.That(StarSmithAccuracy.Curve(error, 0.02, 0.2), Is.EqualTo(expected));
    }

    /// <summary>
    /// The turn: left positive, counterclockwise as seen on the screen (y down).
    /// </summary>
    [Test]
    public void ALeftTurnOnTheScreenIsPositive()
    {
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithAccuracy.Turn((1, 0), (0, -1)), Is.EqualTo(90).Within(1e-9));
            Assert.That(StarSmithAccuracy.Turn((1, 0), (0, 1)), Is.EqualTo(-90).Within(1e-9));
            Assert.That(StarSmithAccuracy.Turn((1, 0), (-1, -1)), Is.EqualTo(135).Within(1e-9));
        });
    }

    /// <summary>
    /// The score is the mean accuracy of the measured placements times the mean timing weight of all, rounded down.
    /// </summary>
    [Test]
    public void TheScoreIsGeometryTimesRhythm()
    {
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithAccuracy.Score([1000, 1000], [1000, 1000, 1000, 1000]), Is.EqualTo(1000));
            Assert.That(StarSmithAccuracy.Score([1000, 1000], [1000, 1000, 500, 1000]), Is.EqualTo(875));
            Assert.That(StarSmithAccuracy.Score([1000, 1000], [1000, 1000, 0, 1000]), Is.EqualTo(750));
            Assert.That(StarSmithAccuracy.Score([874], [1000, 1000, 500]), Is.EqualTo(728));
            Assert.That(StarSmithAccuracy.Score([500, 1000], [1000, 1000, 1000, 1000]), Is.EqualTo(750));
        });
    }

    /// <summary>
    /// The ladder: Faint, Hue, Lustre, Venom, Virulent; Chroma needs a perfect, clean extension too.
    /// </summary>
    [TestCase(0, false, StarSmithSignTier.Faint)]
    [TestCase(399, false, StarSmithSignTier.Faint)]
    [TestCase(400, false, StarSmithSignTier.Hue)]
    [TestCase(599, false, StarSmithSignTier.Hue)]
    [TestCase(600, false, StarSmithSignTier.Lustre)]
    [TestCase(799, false, StarSmithSignTier.Lustre)]
    [TestCase(800, false, StarSmithSignTier.Venom)]
    [TestCase(899, true, StarSmithSignTier.Venom)]
    [TestCase(900, false, StarSmithSignTier.Virulent)]
    [TestCase(900, true, StarSmithSignTier.Chroma)]
    [TestCase(1000, false, StarSmithSignTier.Virulent)]
    public void TheTierFollowsTheLadder(int score, bool perfectExtension, StarSmithSignTier expected)
    {
        Assert.That(StarSmithAccuracy.Tier(score, perfectExtension), Is.EqualTo(expected));
    }

    /// <summary>
    /// Every default constellation drawn exactly, at any rotation and size, matches itself at Virulent, and with its
    /// extension at Chroma; mirrored it matches nothing.
    /// </summary>
    [Test]
    public void EveryDefaultDrawnExactlyMatchesItselfOnly()
    {
        var recipes = StarSmithingConfiguration.DefaultRecipes();
        foreach (var recipe in recipes)
        {
            foreach (var (heading, scale) in new[] { (0.0, 0.08), (73.0, 0.05), (-140.0, 0.1) })
            {
                var drawn = Draw(recipe.Geometry!, heading, scale, extension: false, mirrored: false);
                var match = Best(recipes, drawn);
                Assert.That(match.Recipe?.Number, Is.EqualTo(recipe.Number), $"{recipe.Name} at {heading}");
                Assert.That(match.Measure!.Tier, Is.EqualTo(StarSmithSignTier.Virulent), recipe.Name);
                Assert.That(match.Measure.Score, Is.EqualTo(1000), recipe.Name);

                var extended = Draw(recipe.Geometry!, heading, scale, extension: true, mirrored: false);
                var chroma = Best(recipes, extended);
                Assert.That(chroma.Recipe?.Number, Is.EqualTo(recipe.Number), $"{recipe.Name} extended");
                Assert.That(chroma.Measure!.Tier, Is.EqualTo(StarSmithSignTier.Chroma), recipe.Name);

                var mirror = Draw(recipe.Geometry!, heading, scale, extension: false, mirrored: true);
                Assert.That(Best(recipes, mirror).Recipe?.Number, Is.Not.EqualTo(recipe.Number), $"{recipe.Name} mirrored");
            }
        }
    }

    /// <summary>
    /// No two default shapes, extended or not, can match one figure: they differ somewhere by 30 degrees or a length
    /// factor of 1.5.
    /// </summary>
    [Test]
    public void TheDefaultShapesAreApart()
    {
        var shapes = StarSmithingConfiguration.DefaultRecipes()
            .SelectMany(r => new[] { (r.Name, r.Geometry!, false), (r.Name + "+", r.Geometry!, true) })
            .ToList();
        for (var i = 0; i < shapes.Count; i++)
        {
            for (var j = i + 1; j < shapes.Count; j++)
            {
                Assert.That(StarSmithGeometry.Apart(shapes[i].Item2, shapes[i].Item3, shapes[j].Item2, shapes[j].Item3), Is.True, $"{shapes[i].Item1} and {shapes[j].Item1}");
            }
        }
    }

    /// <summary>
    /// Two shapes close at every placement are not apart.
    /// </summary>
    [Test]
    public void CloseShapesAreNotApart()
    {
        var near = new StarSmithGeometry { Nodes = 4, Turns = [80, -60], Lengths = [1, 1.2, 1] };
        var far = new StarSmithGeometry { Nodes = 4, Turns = [90, -60], Lengths = [1, 1.5, 1] };
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithGeometry.Apart(Poison, false, near, false), Is.False);
            Assert.That(StarSmithGeometry.Apart(Poison, false, far, false), Is.True);
            Assert.That(StarSmithGeometry.Apart(Poison, true, near, false), Is.True, "five placements against four");
        });
    }

    /// <summary>
    /// A closed shape needs its closing placement; an open figure of the same points does not match it.
    /// </summary>
    [Test]
    public void AClosedShapeNeedsTheClose()
    {
        var points = Draw(Fire, 0, 0.1, false, false);
        var timings = Clean(points.Count);
        var closed = StarSmithAccuracy.Measure(points, points.Count - 1, timings, Fire);
        var open = StarSmithAccuracy.Measure(points, -1, timings, Fire);
        Assert.Multiple(() =>
        {
            Assert.That(points, Has.Count.EqualTo(4));
            Assert.That(closed.Matches, Is.True);
            Assert.That(open.Matches, Is.False);
        });
    }

    /// <summary>
    /// A figure is a prefix of a longer shape from its third placement while every measured placement fits.
    /// </summary>
    [Test]
    public void APrefixNeedsThreeFittingPlacements()
    {
        var points = Draw(Poison, 30, 0.1, false, false);
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithAccuracy.Measure(points.Take(2).ToList(), -1, Clean(2), Poison).Prefix, Is.False);
            Assert.That(StarSmithAccuracy.Measure(points.Take(3).ToList(), -1, Clean(3), Poison).Prefix, Is.True);
            Assert.That(StarSmithAccuracy.Measure(points, -1, Clean(4), Poison).Prefix, Is.False, "complete is a match, no prefix");
            Assert.That(StarSmithAccuracy.Measure(points, -1, Clean(4), Poison).Matches, Is.True);
        });
    }

    /// <summary>
    /// The same drawing on the same beat gives the same tier, and a shaky node lowers it by the contract's numbers.
    /// </summary>
    [Test]
    public void TheSameDrawingGivesTheSameTier()
    {
        var points = Draw(Poison, 10, 0.1, false, false);
        var timings = new[] { StarSmithTiming.Clean, StarSmithTiming.Shaky, StarSmithTiming.Clean, StarSmithTiming.Clean };
        var first = StarSmithAccuracy.Measure(points, -1, timings, Poison);
        var second = StarSmithAccuracy.Measure(points.ToList(), -1, timings.ToList(), Poison);
        Assert.Multiple(() =>
        {
            Assert.That(first.Score, Is.EqualTo(875));
            Assert.That(first.Tier, Is.EqualTo(StarSmithSignTier.Venom));
            Assert.That(second, Is.EqualTo(first).Using<StarSmithMeasure>((a, b) => a.Score == b.Score && a.Tier == b.Tier && a.Accuracy.SequenceEqual(b.Accuracy)));
        });
    }

    /// <summary>
    /// A turn 20 degrees off is a miss: no match.
    /// </summary>
    [Test]
    public void ATurnOffByTheMissIsNoMatch()
    {
        var bent = new StarSmithGeometry { Nodes = 4, Turns = [80, -60], Lengths = [1, 1, 1] };
        var points = Draw(bent, 0, 0.1, false, false);
        var measure = StarSmithAccuracy.Measure(points, -1, Clean(4), Poison);
        Assert.Multiple(() =>
        {
            Assert.That(measure.Matches, Is.False);
            Assert.That(measure.Accuracy[2], Is.EqualTo(0));
        });
    }

    /// <summary>
    /// The hull: convex, collinear and inner points dropped, from the least x, at most 8 points.
    /// </summary>
    [Test]
    public void TheHullIsConvexFromTheLeastX()
    {
        var hull = StarSmithHull.Of([P(100, 100), P(300, 100), P(300, 300), P(100, 300), P(200, 200), P(200, 100)]);
        Assert.That(hull, Is.EqualTo(new[] { P(100, 100), P(300, 100), P(300, 300), P(100, 300) }));

        var circle = Enumerable.Range(0, 12)
            .Select(i => P((ushort)(30000 + (10000 * Math.Cos(i * Math.PI / 6))), (ushort)(30000 + (10000 * Math.Sin(i * Math.PI / 6)))))
            .ToList();
        Assert.That(StarSmithHull.Of(circle), Has.Count.EqualTo(8));
    }

    /// <summary>
    /// Hulls overlap when no axis separates them; touching is no overlap.
    /// </summary>
    [Test]
    public void HullsOverlapOnlyWhenTheyShareArea()
    {
        var a = Square(100, 100, 200);
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithHull.Overlaps(a, Square(200, 200, 200)), Is.True);
            Assert.That(StarSmithHull.Overlaps(a, Square(300, 100, 200)), Is.False, "touching");
            Assert.That(StarSmithHull.Overlaps(a, Square(1000, 1000, 50)), Is.False);
            Assert.That(StarSmithHull.Overlaps(a, Square(150, 150, 20)), Is.True, "inside");
            Assert.That(StarSmithHull.Overlaps(a, [P(0, 200), P(500, 200)]), Is.True, "a segment across");
            Assert.That(StarSmithHull.Overlaps(a, [P(0, 50), P(500, 50)]), Is.False, "a segment beside");
        });
    }

    /// <summary>
    /// Faults are dealt from the seed alone, short, inside the margin; the Guardian reveals one.
    /// </summary>
    [Test]
    public void FaultsComeFromTheSeed()
    {
        var rules = new StarSmithForgeRules();
        var (faults, revealed) = StarSmithFaults.Deal(42, rules, guarded: true);
        var (again, revealedAgain) = StarSmithFaults.Deal(42, rules, guarded: true);
        var (_, unguarded) = StarSmithFaults.Deal(42, rules, guarded: false);
        Assert.Multiple(() =>
        {
            Assert.That(faults, Has.Count.EqualTo(3));
            Assert.That(again, Is.EqualTo(faults));
            Assert.That(revealedAgain, Is.EqualTo(revealed));
            Assert.That(revealed, Is.InRange(0, 2));
            Assert.That(unguarded, Is.EqualTo(-1));
            foreach (var fault in faults)
            {
                var length = Math.Sqrt(Math.Pow((fault.A.X - fault.B.X) / 65535.0, 2) + Math.Pow((fault.A.Y - fault.B.Y) / 65535.0, 2));
                Assert.That(length, Is.InRange(0.0, 0.1201));
                Assert.That(fault.A.X / 65535.0, Is.InRange(0.1, 0.9));
                Assert.That(fault.A.Y / 65535.0, Is.InRange(0.1, 0.9));
            }
        });
    }

    /// <summary>
    /// A point within the fault radius of a fault segment hits it.
    /// </summary>
    [Test]
    public void APointNearAFaultHitsIt()
    {
        var fault = new StarSmithFault(P(10000, 20000), P(20000, 20000));
        var radius = (int)(0.025 * 65535);
        Assert.Multiple(() =>
        {
            Assert.That(StarSmithFaults.Hits(P(15000, 20000), fault, 0.025), Is.True);
            Assert.That(StarSmithFaults.Hits(P(15000, (ushort)(20000 + radius - 2)), fault, 0.025), Is.True);
            Assert.That(StarSmithFaults.Hits(P(15000, (ushort)(20000 + radius + 2)), fault, 0.025), Is.False);
            Assert.That(StarSmithFaults.Hits(P((ushort)(20000 + radius + 2), 20000), fault, 0.025), Is.False, "past the end");
        });
    }

    /// <summary>
    /// Slots by the smith's level: 1, 2 from 21, 3 from 50, 4 from 80.
    /// </summary>
    [TestCase(1, 1)]
    [TestCase(20, 1)]
    [TestCase(21, 2)]
    [TestCase(49, 2)]
    [TestCase(50, 3)]
    [TestCase(79, 3)]
    [TestCase(80, 4)]
    [TestCase(100, 4)]
    public void SlotsComeFromTheLevel(int level, int slots)
    {
        Assert.That(StarSmithSlots.For(level), Is.EqualTo(slots));
    }

    /// <summary>
    /// The layer record reads back what it wrote, and skips a broken entry.
    /// </summary>
    [Test]
    public void TheLayerRecordRoundTrips()
    {
        var layers = new[]
        {
            new StarSmithLayer(101, StarSmithSignTier.Chroma, 1, [P(1, 2), P(300, 4), P(5, 600)]),
            new StarSmithLayer(113, StarSmithSignTier.Hue, 13, [P(1000, 1000), P(2000, 1000), P(2000, 2000), P(1000, 2000)]),
        };
        var text = StarSmithLayers.Format(layers);
        Assert.Multiple(() =>
        {
            Assert.That(text, Is.EqualTo("101:5:1:1,2,300,4,5,600;113:1:13:1000,1000,2000,1000,2000,2000,1000,2000"));
            Assert.That(StarSmithLayers.Parse(text), Is.EqualTo(layers).Using<StarSmithLayer>(Same));
            Assert.That(StarSmithLayers.Parse("bad;101:5:1:1,2,300,4,5,600"), Has.Count.EqualTo(1));
            Assert.That(StarSmithLayers.Parse(null), Is.Empty);
        });
    }

    /// <summary>
    /// Draws a shape the way a player would: placements one after another, turning by the shape's turns.
    /// </summary>
    internal static List<StarSmithPoint> Draw(StarSmithGeometry geometry, double headingDegrees, double scale, bool extension, bool mirrored, double x = 0.5, double y = 0.5)
    {
        var sign = mirrored ? -1 : 1;
        var heading = headingDegrees * Math.PI / 180;
        var points = new List<(double X, double Y)> { (x, y) };
        var count = geometry.Placements + (extension ? 1 : 0);
        for (var k = 1; k < count; k++)
        {
            var length = k == geometry.Placements ? geometry.Extension!.Length : geometry.Lengths[k - 1];
            if (k >= 2)
            {
                var turn = k == geometry.Placements ? geometry.Extension!.Turn : geometry.Turns[k - 2];
                heading += sign * turn * Math.PI / 180;
            }

            var last = points[^1];
            points.Add((last.X + (scale * length * Math.Cos(heading)), last.Y - (scale * length * Math.Sin(heading))));
        }

        if (geometry.Closed)
        {
            points[geometry.Placements - 1] = points[0];
        }

        return points.Select(p => new StarSmithPoint((ushort)Math.Round(p.X * 65535), (ushort)Math.Round(p.Y * 65535))).ToList();
    }

    private static (StarSmithRecipe? Recipe, StarSmithMeasure? Measure) Best(IEnumerable<StarSmithRecipe> recipes, List<StarSmithPoint> points)
    {
        var closedAt = points.Count >= 4 ? points.FindIndex(1, p => p == points[0]) : -1;
        var timings = Clean(points.Count);
        return recipes
            .Select(r => (Recipe: (StarSmithRecipe?)r, Measure: (StarSmithMeasure?)StarSmithAccuracy.Measure(points, closedAt, timings, r.Geometry!)))
            .Where(m => m.Measure!.Matches)
            .OrderByDescending(m => m.Measure!.Score)
            .ThenBy(m => m.Recipe!.Number)
            .FirstOrDefault();
    }

    private static List<StarSmithTiming> Clean(int count) => Enumerable.Repeat(StarSmithTiming.Clean, count).ToList();

    private static StarSmithPoint P(ushort x, ushort y) => new(x, y);

    private static List<StarSmithPoint> Square(ushort x, ushort y, ushort size)
        => [P(x, y), P((ushort)(x + size), y), P((ushort)(x + size), (ushort)(y + size)), P(x, (ushort)(y + size))];

    private static bool Same(StarSmithLayer a, StarSmithLayer b)
        => a.Number == b.Number && a.Tier == b.Tier && a.Look == b.Look && a.Hull.SequenceEqual(b.Hull);
}
