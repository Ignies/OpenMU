// <copyright file="StarSmithAccuracy.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// How well a drawn figure fits a shape (Readme.md "PR 7a contract", "Accuracy, score and tier"): the accuracy of each
/// placement on a curve, the timing weight, the score and the tier. Pure: the same drawing on the same beat gives the
/// same result.
/// </summary>
public static class StarSmithAccuracy
{
    /// <summary>An angle error that still counts as perfect, in degrees.</summary>
    public const double AnglePerfect = 2;

    /// <summary>An angle error that misses, in degrees.</summary>
    public const double AngleMiss = 15;

    /// <summary>A length error that still counts as perfect, relative.</summary>
    public const double LengthPerfect = 0.02;

    /// <summary>A length error that misses, relative.</summary>
    public const double LengthMiss = 0.2;

    /// <summary>
    /// The accuracy curve: 1000 within the perfect limit, 0 from the miss, a cosine between.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="perfect">The perfect limit.</param>
    /// <param name="miss">The miss.</param>
    /// <returns>0 to 1000.</returns>
    public static int Curve(double error, double perfect, double miss)
    {
        if (error <= perfect)
        {
            return 1000;
        }

        if (error >= miss)
        {
            return 0;
        }

        var u = (error - perfect) / (miss - perfect);
        return (int)Math.Round(1000 * (1 + Math.Cos(Math.PI * u)) / 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// The turn from one heading to the next, in degrees in (-180, 180], left (counterclockwise on the screen, y down)
    /// positive.
    /// </summary>
    /// <param name="a">The heading into the corner.</param>
    /// <param name="b">The heading out of it.</param>
    /// <returns>The turn.</returns>
    public static double Turn((double X, double Y) a, (double X, double Y) b)
        => -Math.Atan2((a.X * b.Y) - (a.Y * b.X), (a.X * b.X) + (a.Y * b.Y)) * 180 / Math.PI;

    /// <summary>
    /// An angle in degrees, wrapped into (-180, 180].
    /// </summary>
    /// <param name="degrees">The angle.</param>
    /// <returns>The wrapped angle.</returns>
    public static double Wrap(double degrees)
    {
        var wrapped = degrees % 360;
        if (wrapped <= -180)
        {
            wrapped += 360;
        }

        if (wrapped > 180)
        {
            wrapped -= 360;
        }

        return wrapped;
    }

    /// <summary>
    /// A placement's timing weight: clean 1000, shaky 500, late 0.
    /// </summary>
    /// <param name="timing">The timing.</param>
    /// <returns>The weight.</returns>
    public static int Weight(StarSmithTiming timing) => timing switch
    {
        StarSmithTiming.Clean => 1000,
        StarSmithTiming.Shaky => 500,
        _ => 0,
    };

    /// <summary>
    /// The score: the mean accuracy of the measured placements times the mean timing weight of all base placements,
    /// in permille, rounded down.
    /// </summary>
    /// <param name="accuracy">The accuracy of each measured placement.</param>
    /// <param name="weights">The timing weight of each base placement.</param>
    /// <returns>0 to 1000.</returns>
    public static int Score(IReadOnlyList<int> accuracy, IReadOnlyList<int> weights)
    {
        if (accuracy.Count == 0 || weights.Count == 0)
        {
            return 0;
        }

        var geometry = accuracy.Sum(a => (long)a);
        var rhythm = weights.Sum(w => (long)w);
        return (int)Math.Clamp(geometry * rhythm / ((long)accuracy.Count * weights.Count * 1000), 0, 1000);
    }

    /// <summary>
    /// The tier of a score: Faint under 400, Hue under 600, Lustre under 800, Venom under 900, else Virulent, or
    /// Chroma with a perfect, clean extension.
    /// </summary>
    /// <param name="score">The score.</param>
    /// <param name="perfectExtension">Whether the extension was placed perfect and clean.</param>
    /// <returns>The tier.</returns>
    public static StarSmithSignTier Tier(int score, bool perfectExtension) => score switch
    {
        < 400 => StarSmithSignTier.Faint,
        < 600 => StarSmithSignTier.Hue,
        < 800 => StarSmithSignTier.Lustre,
        < 900 => StarSmithSignTier.Venom,
        _ => perfectExtension ? StarSmithSignTier.Chroma : StarSmithSignTier.Virulent,
    };

    /// <summary>
    /// What a placement measures: the turn at the node before it and its segment's length over the first. Placement 0
    /// measures nothing, placement 1 only its length (1).
    /// </summary>
    /// <param name="points">The figure's placements.</param>
    /// <param name="placement">The placement.</param>
    /// <returns>The turn in degrees and the ratio; zeros where there is nothing to measure.</returns>
    public static (double Turn, double Ratio) Measured(IReadOnlyList<StarSmithPoint> points, int placement)
    {
        if (placement < 1 || placement >= points.Count)
        {
            return (0, 0);
        }

        var first = points[0].DistanceTo(points[1]);
        var ratio = first > 0 ? points[placement - 1].DistanceTo(points[placement]) / first : 0;
        if (placement < 2)
        {
            return (0, ratio);
        }

        var a = Heading(points[placement - 2], points[placement - 1]);
        var b = Heading(points[placement - 1], points[placement]);
        return (a == (0, 0) || b == (0, 0) ? 0 : Turn(a, b), ratio);
    }

    /// <summary>
    /// Measures a figure against a shape.
    /// </summary>
    /// <param name="points">The figure's placements, in order.</param>
    /// <param name="closedAt">The placement that closed the figure, or -1.</param>
    /// <param name="timings">The timing of each placement.</param>
    /// <param name="geometry">The shape.</param>
    /// <returns>The measure.</returns>
    public static StarSmithMeasure Measure(IReadOnlyList<StarSmithPoint> points, int closedAt, IReadOnlyList<StarSmithTiming> timings, StarSmithGeometry geometry)
    {
        var count = points.Count;
        var accuracy = new int[count];
        var placements = geometry.Placements;
        var extended = geometry.Extension is not null;
        if (!geometry.IsWellFormed || count > placements + (extended ? 1 : 0))
        {
            return new StarSmithMeasure(false, false, false, accuracy, 0, StarSmithSignTier.Faint);
        }

        var fits = true;
        for (var k = 0; k < count; k++)
        {
            accuracy[k] = k < 2 ? 1000 : Accuracy(points, k, geometry.TurnAt(k), geometry.LengthAt(k));
            fits &= accuracy[k] > 0;
        }

        var closeFits = !geometry.Closed || (count >= placements ? closedAt == placements - 1 : closedAt < 0);
        var matches = fits && closeFits && (count == placements || (extended && count == placements + 1));
        var prefix = fits && closeFits && count >= 3 && count < placements;
        if (!matches)
        {
            return new StarSmithMeasure(false, prefix, false, accuracy, 0, StarSmithSignTier.Faint);
        }

        var weights = timings.Take(placements).Select(Weight).ToList();
        var score = Score(accuracy.Skip(2).Take(placements - 2).ToList(), weights);
        var withExtension = count == placements + 1;
        var perfectExtension = withExtension && accuracy[placements] == 1000 && timings.Count > placements && timings[placements] == StarSmithTiming.Clean;
        return new StarSmithMeasure(true, false, withExtension, accuracy, score, Tier(score, perfectExtension));
    }

    private static int Accuracy(IReadOnlyList<StarSmithPoint> points, int placement, double turn, double length)
    {
        var a = Heading(points[placement - 2], points[placement - 1]);
        var b = Heading(points[placement - 1], points[placement]);
        var first = points[0].DistanceTo(points[1]);
        if (a == (0, 0) || b == (0, 0) || first <= 0 || length <= 0)
        {
            return 0;
        }

        var angleError = Math.Abs(Wrap(Turn(a, b) - turn));
        var ratio = points[placement - 1].DistanceTo(points[placement]) / first;
        var lengthError = Math.Abs(ratio - length) / length;
        return Math.Min(Curve(angleError, AnglePerfect, AngleMiss), Curve(lengthError, LengthPerfect, LengthMiss));
    }

    private static (double X, double Y) Heading(StarSmithPoint from, StarSmithPoint to)
        => ((to.X - from.X) / StarSmithPoint.Box, (to.Y - from.Y) / StarSmithPoint.Box);
}
