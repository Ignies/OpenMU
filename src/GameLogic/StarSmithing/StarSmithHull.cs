// <copyright file="StarSmithHull.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The part of the item a constellation covers (Readme.md "PR 7a contract", "The hull and the layer record"): the
/// convex hull of its placements in box units, at most 8 points, and whether two hulls overlap. Integer arithmetic.
/// </summary>
public static class StarSmithHull
{
    /// <summary>The most points a hull keeps.</summary>
    public const int MaximumPoints = 8;

    /// <summary>
    /// The convex hull of points (Andrew's monotone chain): collinear and inner points dropped, in order around it,
    /// starting at the least x (least y on a tie). Above 8 points, the point whose triangle with its neighbours has the
    /// least area goes first, until 8 remain.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <returns>The hull.</returns>
    public static IReadOnlyList<StarSmithPoint> Of(IEnumerable<StarSmithPoint> points)
    {
        var sorted = points.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        if (sorted.Count <= 2)
        {
            return sorted;
        }

        var lower = new List<StarSmithPoint>();
        foreach (var point in sorted)
        {
            while (lower.Count >= 2 && Cross(lower[^2], lower[^1], point) <= 0)
            {
                lower.RemoveAt(lower.Count - 1);
            }

            lower.Add(point);
        }

        var upper = new List<StarSmithPoint>();
        for (var i = sorted.Count - 1; i >= 0; i--)
        {
            while (upper.Count >= 2 && Cross(upper[^2], upper[^1], sorted[i]) <= 0)
            {
                upper.RemoveAt(upper.Count - 1);
            }

            upper.Add(sorted[i]);
        }

        var hull = lower.Take(lower.Count - 1).Concat(upper.Take(upper.Count - 1)).ToList();
        while (hull.Count > MaximumPoints)
        {
            var least = 0;
            var leastArea = long.MaxValue;
            for (var i = 0; i < hull.Count; i++)
            {
                var area = Math.Abs(Cross(hull[(i + hull.Count - 1) % hull.Count], hull[i], hull[(i + 1) % hull.Count]));
                if (area < leastArea)
                {
                    leastArea = area;
                    least = i;
                }
            }

            hull.RemoveAt(least);
        }

        var start = hull.IndexOf(hull.OrderBy(p => p.X).ThenBy(p => p.Y).First());
        return hull.Skip(start).Concat(hull.Take(start)).ToList();
    }

    /// <summary>
    /// Whether two hulls share area: no edge normal of either separates them. Hulls that only touch do not overlap.
    /// </summary>
    /// <param name="a">One hull.</param>
    /// <param name="b">The other hull.</param>
    /// <returns>True when they overlap.</returns>
    public static bool Overlaps(IReadOnlyList<StarSmithPoint> a, IReadOnlyList<StarSmithPoint> b)
    {
        if (a.Count == 0 || b.Count == 0)
        {
            return false;
        }

        foreach (var axis in Axes(a).Concat(Axes(b)))
        {
            var (minA, maxA) = Project(a, axis);
            var (minB, maxB) = Project(b, axis);
            if (maxA <= minB || maxB <= minA)
            {
                return false;
            }
        }

        return true;
    }

    private static long Cross(StarSmithPoint o, StarSmithPoint a, StarSmithPoint b)
        => ((long)(a.X - o.X) * (b.Y - o.Y)) - ((long)(a.Y - o.Y) * (b.X - o.X));

    private static IEnumerable<(long X, long Y)> Axes(IReadOnlyList<StarSmithPoint> hull)
    {
        if (hull.Count == 1)
        {
            yield break;
        }

        for (var i = 0; i < hull.Count; i++)
        {
            var from = hull[i];
            var to = hull[(i + 1) % hull.Count];
            long dx = to.X - from.X;
            long dy = to.Y - from.Y;
            yield return (-dy, dx);
            if (hull.Count == 2)
            {
                yield return (dx, dy);
                yield break;
            }
        }
    }

    private static (long Min, long Max) Project(IReadOnlyList<StarSmithPoint> hull, (long X, long Y) axis)
    {
        var min = long.MaxValue;
        var max = long.MinValue;
        foreach (var point in hull)
        {
            var value = (point.X * axis.X) + (point.Y * axis.Y);
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return (min, max);
    }
}
